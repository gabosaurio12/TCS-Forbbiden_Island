using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.view;
using log4net;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Forbbiden.Client
{
    public partial class MainPage : Page
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LoginPage));

        public MainPage()
        {
            InitializeComponent();

            if (Properties.Settings.Default.rememberLogin)
            {
                ReloadMainPage();
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NavigationService != null)
                {
                    NavigationService.Navigate(new PlayPage());
                }
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
                if (NavigationService != null)
                {
                    NavigationService.Navigate(new LoginPage());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir la página de login.");
                log.Error("MainPage.xaml.cs - LogInButton_Click", ex);
            }
        }

        public void ReloadMainPage()
        {
            try
            {
                var client = new ProfileManagerClient();
                Player player = client.GetCurrentLogin();

                if (player != null)
                {
                    txtBkUser.Text = player.PlayerUsername;

                    ImageBrush avatar = new ImageBrush(
                        new System.Windows.Media.Imaging.BitmapImage(
                            new Uri(player.PlayerAvatarPath, UriKind.RelativeOrAbsolute)
                        )
                    );

                    imgAvatar.Fill = avatar;
                }
            }
            catch (Exception ex)
            {
                log.Error("MainPage.xaml.cs - ReloadMainPage", ex);
            }
        }
    }
}
