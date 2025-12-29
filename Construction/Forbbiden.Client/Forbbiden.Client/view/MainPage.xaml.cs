using Forbbiden.Client.FriendsManager;
using Forbbiden.Client.logic;
using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.view;
using Forbbiden.Client.view.games;
using Forbbiden.Client.view.info;
using log4net;
using System;
using System.Globalization;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Forbbiden.Client
{
    public partial class MainPage : Page
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MainPage));
        private readonly ProfileManagerClient Client;
        public static FriendsNotificationSingleton CallbackManager { get; private set; }
        public static IFriendsManager FriendsProxy { get; private set; }

        public MainPage()
        {
            InitializeComponent();

            Client = new ProfileManagerClient();

            _ = SetLogin();
            SetBackground(background);
        }

        private async Task SetLogin()
        {
            int playerId = Properties.PlayerSettings.Default.CurrentPlayerId;

            if (playerId > 0)
            {
                ProfileManager.Player currentLogin = new ProfileManager.Player();

                try
                {
                    currentLogin = await Client.GetPlayerByIdAsync(playerId, false);
                }
                catch (FaultException<DBFault> ex)
                {
                    Log.Error("ERROR: MainPage.SetLogin", ex);
                    ViewUtils.ShowPullError(Window.GetWindow(this));
                }

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
                NavigationService?.Navigate(new BoardPage());
            }
            catch (Exception ex)
            {
                ViewUtils.HandlePageLoadError(Window.GetWindow(this));
                Log.Error("ERROR: MainPage.PlayButton_Click", ex);
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
                ViewUtils.HandlePageLoadError(Window.GetWindow(this));
                Log.Error("ERROR: MainPage.SettingsButton_Click", ex);
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
                ViewUtils.HandlePageLoadError(Window.GetWindow(this));
                Log.Error("ERROR: MainPage.ProfileButton_Click", ex);
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
                ViewUtils.HandlePageLoadError(Window.GetWindow(this));
                Log.Error("ERROR: MainPage.LogInButton_Click", ex);
            }
        }

        private async void ConnectPlayer(string username)
        {
            bool isConnected = false;

            try
            {
                isConnected = await Client.ConnectPlayerByUsernameAsync(username);
            }
            catch (FaultException<DBFault> dbFault)
            {
                Log.Error("ERROR: LoginPage.ConnectPlayer", dbFault);
                ViewUtils.ShowPushError(Window.GetWindow(this));
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
                ConnectPlayer(ClientSession.Username);
            }
            catch (Exception ex)
            {
                ViewUtils.HandlePageLoadError(Window.GetWindow(this));
                Log.Error("ERROR: MainPage.ReloadMainPage", ex);
            }
        }

        private void FriendsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new FriendsPage());
        }

        private void ShowVerificationWindow(ProfileManager.Player player)
        {
            var verificationWindow = new VerificationWindow(player.PlayerId)
            {
                Owner = Window.GetWindow(this)
            };

            verificationWindow.OnVerified += async () =>
            {
                var profileManager = new ProfileManagerClient();
                var updatedPlayer = await profileManager.GetPlayerByIdAsync(player.PlayerId, false);

                Dispatcher.Invoke(() =>
                {
                    ClientSession.SetPlayer(updatedPlayer);
                    ReloadMainPage(updatedPlayer);
                });
            };

            verificationWindow.ShowDialog();
        }
        
        private async void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            var player = ClientSession.GetPlayer();
            var result = await Client.SendEmailAsync(player.PlayerEmail, player.PlayerId);

            if (result)
            {
                string title = Properties.Langs.Resources.verification_token_sent_title;
                string message = Properties.Langs.Resources.verification_token_sent;
                ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));

                ShowVerificationWindow(player);
            }
        }
    }
}
