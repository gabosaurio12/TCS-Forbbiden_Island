using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.view;
using Forbbiden.Client.view.games;
using log4net;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Forbbiden.Client
{
    public partial class MainPage : Page
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LoginPage));

        public MainPage()
        {
            InitializeComponent();

            var client = new ProfileManagerClient();

            if (client.GetCurrentLogin().PlayerId != -1)
            {
                ReloadMainPage(txtBkUser, imgAvatar);
                logInButton.Visibility = Visibility.Hidden;
            }
            else
            {
                profileButton.Visibility = Visibility.Hidden;
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
                MessageBox.Show("Error al abrir la página de juego.");
                log.Error("MainPage.xaml.cs - PlayButton_Click", ex);
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
                MessageBox.Show("Error al abrir la configuración.");
                log.Error("MainPage.xaml.cs - SettingsButton_Click", ex);
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
                var client = new ProfileManagerClient();
                Player player = client.GetCurrentLogin();

                if (NavigationService != null)
                {
                    if (player == null)
                    {
                        NavigationService.Navigate(new ProfilePage());
                    }
                    else
                    {
                        NavigationService.Navigate(new ProfilePage(player));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir el perfil.");
                log.Error("MainPage.xaml.cs - ProfileButton_Click", ex);
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
                MessageBox.Show("Error al abrir la página de login.");
                log.Error("MainPage.xaml.cs - LogInButton_Click", ex);
            }
        }

        public static void ReloadMainPage(TextBlock txtBkUser, Ellipse imgAvatar)
        {
            try
            {
                var client = new ProfileManagerClient();
                Player player = client.GetCurrentLogin();

                if (player != null)
                {
                    txtBkUser.Text = player.PlayerUsername;

                    string projectDir = Directory.GetParent(
                    AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
                    string avatarPath = System.IO.Path.Combine(projectDir, "avatars", player.PlayerAvatarPath);

                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.UriSource = new Uri(avatarPath, UriKind.Absolute);
                    bmp.EndInit();
                    imgAvatar.Fill = new ImageBrush(bmp);

                }
            }
            catch (Exception ex)
            {
                log.Error("MainPage.xaml.cs - ReloadMainPage", ex);
            }
        }

        private void FriendsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new FriendsPage());
        }
    }
}
