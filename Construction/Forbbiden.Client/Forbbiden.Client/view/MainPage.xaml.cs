using Forbbiden.Client.Exceptions;
using Forbbiden.Client.FriendsManager;
using Forbbiden.Client.logic;
using Forbbiden.Client.Logic;
using Forbbiden.Client.Repositories;
using Forbbiden.Client.view;
using Forbbiden.Client.view.info;
using log4net;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

            ProfileRepo = new ProfileRepository();

            _ = SetLogin();
            SetBackground(background);
        }

        private async Task SetLogin()
        {
            int playerId = Properties.PlayerSettings.Default.CurrentPlayerId;

            if (playerId > 0)
            {
                ProfileManager.Player currentLogin = await ProfileRepo.GetPlayerById(playerId, false);

                if (currentLogin.PlayerId > 0)
                {
                    ClientSession.SetPlayer(currentLogin);
                    ReloadMainPage(currentLogin);
                    profileButton.Visibility = Visibility.Visible;
                    friendsButton.Visibility = Visibility.Visible;

                    FriendsNotificationSingleton.Instance.Subscribe(ClientSession.Username);
                }
            }
            else
            {
                ClientSession.SetGuestSession();
                logInButton.Visibility = Visibility.Visible;
            }
        }

        private static void SetBackground(ImageBrush background)
        {
            DateTime currentTime = DateTime.Now;
            string ampm = currentTime.ToString("tt", CultureInfo.InvariantCulture).ToLower();
            if (ampm == "pm")
            {
                string darkBackground = "FEI MainPage3.png";
                string projectDir = Directory.GetParent(
                AppDomain.CurrentDomain.BaseDirectory).
                Parent.Parent.FullName;
                string imagesPath = Path.Combine(
                    projectDir, "Images");
                string backgroundPath = Path.Combine(
                    imagesPath, darkBackground);
                background.ImageSource = ViewUtils.GetBitmapImage(backgroundPath);
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
                ErrorsNotificationManager.HandlePageLoadError(Window.GetWindow(this));
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
                ErrorsNotificationManager.HandlePageLoadError(Window.GetWindow(this));
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
                ErrorsNotificationManager.HandlePageLoadError(Window.GetWindow(this));
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
                ErrorsNotificationManager.HandlePageLoadError(Window.GetWindow(this));
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
                ErrorsNotificationManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
            }

            if (isConnected)
            {
                ClientSession.Status = 1;
            }
        }

        public void ReloadMainPage(ProfileManager.Player player)
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

                string projectDir = ViewUtils.GetProjectDir();
                string avatarPath = Path.Combine(projectDir, "avatars", player.PlayerAvatarPath);
                imgAvatar.Fill = ViewUtils.GetImageBrush(avatarPath);
                _ = ConnectPlayer(ClientSession.Username);
            }
            catch (Exception ex)
            {
                Log.Error("MainPage.ReloadMainPage", ex);
                ErrorsNotificationManager.HandlePageLoadError(Window.GetWindow(this));
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
                    updatedPlayer = await new ProfileRepository().GetPlayerById(
                        player.PlayerId, false);
                }
                catch (ViewException ex)
                {
                    ErrorsNotificationManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
                }

                if (updatedPlayer != null && updatedPlayer.PlayerId != -1)
                {
                    Dispatcher.Invoke(() =>
                    {
                        ClientSession.SetPlayer(updatedPlayer);
                        ReloadMainPage(updatedPlayer);
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
                ErrorsNotificationManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
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
