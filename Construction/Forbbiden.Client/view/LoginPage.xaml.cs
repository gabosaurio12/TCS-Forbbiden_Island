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

        private bool passwordVisible = false;
        public LoginPage()
        {
            InitializeComponent();
        }

        private static void ResetFieldColors(TextBlock txtBkUser, TextBlock txtBkPassword)
        {
            txtBkUser.Foreground = Brushes.White;
            txtBkPassword.Foreground = Brushes.White;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            ResetFieldColors(txtBkUser, txtBkPassword);

            var client = new ProfileManagerClient();

            string username = txtBxUsername.Text.Trim();

            var searchPlayer = client.GetPlayerByUsername(username);

            if (searchPlayer.PlayerId == -1)
            {
                MessageBox.Show(Properties.Langs.Resources.usernameNoExists);
                txtBkUser.Foreground = Brushes.Red;
            }
            else
            {
                string password = "";
                if (chkPassword.IsChecked == true)
                {
                    password = txtBxPasswordVisible.Text;
                }
                else
                {
                    password = pwdBxPassword.Password;
                }

                if (BCrypt.Net.BCrypt.Verify(password, searchPlayer.PlayerPassword))
                {
                    if (client.Login(searchPlayer))
                    {

                        NavigationService.GoBack();
                        
                        log.Info("User '{searchPlayer.PlayerUsername}' logged in.");
                    }
                    else
                    {
                        log.Warn("Login failed for user '{searchPlayer.PlayerUsername}'.");
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

            if (!passwordVisible)
            {
                string darkBossPath = Path.Combine(projectPath, "Images", "bossdark.png");
                bossImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(darkBossPath));

                txtBxPasswordVisible.Text = pwdBxPassword.Password;
                txtBxPasswordVisible.Visibility = Visibility.Visible;
                pwdBxPassword.Visibility = Visibility.Collapsed;
                passwordVisible = true;
            }
            else
            {
                pwdBxPassword.Password = txtBxPasswordVisible.Text;
                txtBxPasswordVisible.Visibility = Visibility.Collapsed;
                pwdBxPassword.Visibility = Visibility.Visible;
                passwordVisible = false;

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