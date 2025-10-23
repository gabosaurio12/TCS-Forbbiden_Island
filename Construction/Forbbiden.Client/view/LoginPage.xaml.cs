using log4net;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Forbbiden.Client.ProfileManager;
using System.IO;

namespace Forbbiden.Client
{
    /// <summary>
    /// LoginWindow.xaml interaction logic
    /// </summary>
    public partial class LoginPage : Page
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LoginPage));

        private bool _passwordVisible = false;
        public LoginPage()
        {
            InitializeComponent();
        }

        private void ResetFieldColors()
        {
            txtBkUser.Foreground = Brushes.Black;
            txtBkPassword.Foreground = Brushes.Black;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            ResetFieldColors();

            var client = new ProfileManagerClient();

            Player searchPlayer = client.GetPlayerByUsername(txtBUsername.Text);

            if (searchPlayer == null)
            {
                MessageBox.Show(Properties.Langs.Resources.usernameNoExists);
                txtBkUser.Foreground = Brushes.Red;
            }
            else
            {
                string password = "";
                if (chkPassword.IsChecked == true)
                {
                    password = txtBPasswordVisible.Text;
                }
                else
                {
                    password = pwdBPassword.Password;
                }

                if (BCrypt.Net.BCrypt.Verify(password, searchPlayer.PlayerPassword))
                {
                    if (client.Login(searchPlayer))
                    {
                        Properties.Settings.Default.rememberLogin = chkRememberMe.IsChecked == true;
                        Properties.Settings.Default.Save();

                        NavigationService.GoBack();
                        
                        log.Info($"User '{searchPlayer.PlayerUsername}' logged in.");
                    }
                    else
                    {
                        log.Warn($"Login failed for user '{searchPlayer.PlayerUsername}'.");
                        MessageBox.Show(Properties.Langs.Resources.loginError);
                    }
                }
                else
                {
                    txtBkPassword.Foreground = Brushes.Red;
                    MessageBox.Show(Properties.Langs.Resources.wrongPassword);
                }
            }
        }

        private void TogglePassword_Click(object sender, RoutedEventArgs e)
        {
            string projectPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\.."));

            if (!_passwordVisible)
            {
                string darkBossPath = Path.Combine(projectPath, "Images", "bossdark.png");
                bossImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(darkBossPath));

                txtBPasswordVisible.Text = pwdBPassword.Password;
                txtBPasswordVisible.Visibility = Visibility.Visible;
                pwdBPassword.Visibility = Visibility.Collapsed;
                _passwordVisible = true;
            }
            else
            {
                pwdBPassword.Password = txtBPasswordVisible.Text;
                txtBPasswordVisible.Visibility = Visibility.Collapsed;
                pwdBPassword.Visibility = Visibility.Visible;
                _passwordVisible = false;

                string bossPath = Path.Combine(projectPath, "Images", "boss.png");
                bossImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(bossPath));
            }
        }

        private void Signup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new SignupPage());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}