using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.view.info;
using log4net;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

        private static void ResetFields(TextBlock txtBkUser, TextBlock txtBkPassword, TextBlock txtBkBoss)
        {
            txtBkBoss.Text = Properties.Langs.Resources.bossLogin;
            txtBkUser.Foreground = Brushes.White;
            txtBkPassword.Foreground = Brushes.White;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            ResetFields(txtBkUser, txtBkPassword, txtBkBoss);

            var client = new ProfileManagerClient();

            string username = txtBxUsername.Text.Trim();

            var searchPlayer = client.GetPlayerByUsername(username);

            if (searchPlayer.PlayerId == -1)
            {
                txtBkBoss.Text = Properties.Langs.Resources.usernameNoExists;
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

                        NavigationService?.Navigate(new MainPage());
                        
                        log.Info("User '{searchPlayer.PlayerUsername}' logged in.");
                    }
                    else
                    {
                        log.Warn("Login failed for user '{searchPlayer.PlayerUsername}'.");
                        string title = Properties.Langs.Resources.error;
                        string message = Properties.Langs.Resources.loginError;
                        var notificationWindow = new NotificationWindow(title, message)
                        {
                            Owner = Window.GetWindow(this)
                        };
                        notificationWindow.ShowDialog();
                    }
                }
                else
                {
                    txtBkPassword.Foreground = Brushes.Red;
                    txtBkBoss.Text = Properties.Langs.Resources.wrongPassword;
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
            NavigationService?.Navigate(new MainPage());
        }
    }
}