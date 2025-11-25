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
using System.Windows;
using System.Windows.Controls;

namespace Forbbiden.Client
{
    public partial class MainPage : Page
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MainPage));
        private readonly ProfileManagerClient Client = new ProfileManagerClient();
        public MainPage()
        {
            InitializeComponent();
            SetLogin();
            SetBackground();
        }

        private async void SetLogin()
        {
            int playerId = Properties.PlayerSettings.Default.CurrentPlayerId;

            if (playerId > 0)
            {
                Player currentLogin = new Player();

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
                    logInButton.Visibility = Visibility.Hidden;
                }
                else
                {
                    profileButton.Visibility = Visibility.Hidden;
                }
            }
        }

        private void SetBackground()
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
                //NavigationService?.Navigate(new PlayPage());
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
                Player player = ClientSession.GetPlayer();

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

        public void ReloadMainPage(Player player)
        {
            try
            {
                if (player.IsVerified == 0)
                {
                    verifyButton.Visibility = Visibility.Visible;
                }

                txtBkUser.Text = player.PlayerUsername;

                string projectDir = ViewUtils.GetProjectDir();
                string avatarPath = System.IO.Path.Combine(projectDir, "avatars", player.PlayerAvatarPath);
                imgAvatar.Fill = ViewUtils.GetImageBrush(avatarPath);

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

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            var player = ClientSession.GetPlayer();
            var verificationWindow = new VerificationWIndow(player.PlayerId)
            {
                Owner = Window.GetWindow(this)
            };
            verificationWindow.ShowDialog();
        }
    }
}
