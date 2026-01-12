using Forbbiden.Client.Exceptions;
using Forbbiden.Client.FriendsManager;
using Forbbiden.Client.Logic;
using Forbbiden.Client.Model;
using Forbbiden.Client.Repositories;
using Forbbiden.Client.View;
using Forbbiden.Client.View.info;
using log4net;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Forbbiden.Client
{
    public partial class MainPage : Page
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MainPage));
        private readonly ProfileRepository ProfileRepo;
        public static FriendsNotificationSingleton CallbackManager { get; private set; }
        public static IFriendsManager FriendsProxy { get; private set; }

        public MainPage()
        {
            InitializeComponent();
            ViewUtils.SetBackground(background);

            ProfileRepo = new ProfileRepository();

            _ = SetLogin();
        }

        private async Task SetLogin()
        {
            int playerId = Properties.PlayerSettings.Default.CurrentPlayerId;

            if (playerId > 0)
            {
                ProfileManager.Player currentLogin = await ProfileRepository.GetPlayerById(playerId, false);

                if (currentLogin.PlayerId > 0)
                {
                    ClientSession.SetPlayer(currentLogin);
                    await ReloadMainPage(currentLogin);
                    profileButton.Visibility = Visibility.Visible;
                    friendsButton.Visibility = Visibility.Visible;
                }
            }
            else
            {
                ClientSession.SetGuestSession();
                logInButton.Visibility = Visibility.Visible;
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new PlayPage());
            }
            catch (Exception ex)
            {
                Log.Error("MainPage.PlayButton_Click", ex);
                ExceptionViewManager.HandlePageLoadError(Window.GetWindow(this));
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new SelectLanguagePage());
            }
            catch (Exception ex)
            {
                Log.Error("MainPage.SettingsButton_Click", ex);
                ExceptionViewManager.HandlePageLoadError(Window.GetWindow(this));
            }
        }

        private void QuitGameButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new QuitPage());
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ProfileManager.Player player = ClientSession.GetPlayer();

                NavigationService?.Navigate(new ProfilePage(player));
            }
            catch (Exception ex)
            {
                ExceptionViewManager.HandlePageLoadError(Window.GetWindow(this));
                Log.Error("MainPage.ProfileButton_Click", ex);
            }
        }

        private void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new LoginPage());
            }
            catch (Exception ex)
            {
                ExceptionViewManager.HandlePageLoadError(Window.GetWindow(this));
                Log.Error("MainPage.LogInButton_Click", ex);
            }
        }

        private async Task ConnectPlayer(string username)
        {
            bool isConnected = false;

            try
            {
                isConnected = await ProfileRepo.ConnectPlayer(username);
            }
            catch (ViewException ex)
            {
                ExceptionViewManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
            }

            if (isConnected)
            {
                ClientSession.Status = 1;
            }
        }

        public async Task ReloadMainPage(ProfileManager.Player player)
        {
            try
            {
                if (player.IsVerified == 0)
                {
                    verifyButton.Visibility = Visibility.Visible;
                }
                else
                {
                    verifyButton.Visibility = Visibility.Hidden;
                }

                txtBkUser.Text = player.PlayerUsername;
                AvatarsManager.Instance.UpdateCache(player.PlayerUsername, player.PlayerAvatarBytes);

                var avatarBrush = await AvatarsManager.Instance.GetAvatarBrushAsync(player.PlayerUsername);
                imgAvatar.Fill = avatarBrush ?? new ImageBrush();

                _ = ConnectPlayer(ClientSession.Username);
            }
            catch (Exception ex)
            {
                Log.Error("MainPage.ReloadMainPage", ex);
                ExceptionViewManager.HandlePageLoadError(Window.GetWindow(this));
            }
        }

        private void FriendsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new FriendsPage());
        }

        private void ShowVerificationWindow(ProfileManager.Player player)
        {
            var verificationWindow = new VerificationWindow(player.PlayerId, false)
            {
                Owner = Window.GetWindow(this)
            };

            verificationWindow.OnVerified += async () =>
            {
                ProfileManager.Player updatedPlayer = null;
                try
                {
                    updatedPlayer = await ProfileRepository.GetPlayerById(
                        player.PlayerId, false);
                }
                catch (ViewException ex)
                {
                    ExceptionViewManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
                }

                if (updatedPlayer != null && updatedPlayer.PlayerId != -1)
                {
                    _ = Dispatcher.Invoke(async () =>
                    {
                        ClientSession.SetPlayer(updatedPlayer);
                        await ReloadMainPage(updatedPlayer);
                    });
                }
            };

            verificationWindow.ShowDialog();
        }

        private async void VerifyButtonAsync_Click(object sender, RoutedEventArgs e)
        {
            var tokenRepo = new TokenRepository();
            bool result = false;
            try
            {
                var token = await tokenRepo.GenerateToken(ClientSession.CurrentPlayerId);
                result = await ProfileRepo.SendSignupEmail(ClientSession.Email, token.TokenString);
            }
            catch (ViewException ex)
            {
                ExceptionViewManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
            }

            if (result)
            {
                var player = ClientSession.GetPlayer();
                string title = Properties.Resources.verification_token_sent_title;
                string message = Properties.Resources.verification_token_sent;
                ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));

                ShowVerificationWindow(player);
            }
        }
    }
}