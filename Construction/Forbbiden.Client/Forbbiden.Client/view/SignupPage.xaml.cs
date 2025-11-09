using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.view.info;
using log4net;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Forbbiden.Client
{
    /// <summary>
    /// LoginWindow.xaml logic interaction
    /// </summary>
    public partial class SignupPage : Page
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SignupPage));
        private const int PasswordMinLength = 7;
        public SignupPage()
        {
            InitializeComponent();
        }

        private void OpenNotification(string title, string message)
        {
            var notificationWindow = new NotificationWindow(title, message)
            {
                Owner = Window.GetWindow(this)
            };
            notificationWindow.ShowDialog();
        }

        private static bool ValidatePassword(string password)
        {
            if (!string.IsNullOrWhiteSpace(password) && password.Length > PasswordMinLength)
            {
                var passwordUpperCase = Regex.IsMatch(password, @"[A-Z]", 
                    RegexOptions.None, TimeSpan.FromMilliseconds(100));
                if (!passwordUpperCase) return false;
                var passwordLowerCase = Regex.IsMatch(password, @"[a-z]", 
                    RegexOptions.None, TimeSpan.FromMilliseconds(100));
                if (!passwordLowerCase) return false;
                var passwordNumbers = Regex.IsMatch(password, @"[0-9]", 
                    RegexOptions.None, TimeSpan.FromMilliseconds(100));
                if (!passwordNumbers) return false;
                var passwordSpecialChar = Regex.IsMatch(password, @"[\W_]", 
                    RegexOptions.None, TimeSpan.FromMilliseconds(100));
                if (!passwordSpecialChar) return false;
            }
            else
            {
                return false;
            }
            return true;
        }

        private static void TurnTextBlockRed(TextBlock textBlock)
        {
            textBlock.Foreground = Brushes.Red;
        }

        private static void ResetTextBlocks(TextBlock txtBlockUsername, TextBlock txtBlockEmail, TextBlock txtBlockPassword)
        {
            txtBlockUsername.Foreground = Brushes.White;
            txtBlockEmail.Foreground = Brushes.White;
            txtBlockPassword.Foreground = Brushes.White;
        }

        private bool ValidatePlayer(ref Player player, ProfileManagerClient client)
        {
            bool isValid = true;
            string title = Properties.Langs.Resources.invalid_input;

            if (!ValidatePassword(player.PlayerPassword))
            {
                string message = Properties.Langs.Resources.signup_invalid_password;
                OpenNotification(title, message);
                isValid = false;
                TurnTextBlockRed(txtBkPassword);
            }
            if (string.IsNullOrWhiteSpace(player.PlayerUsername))
            {
                string message = Properties.Langs.Resources.signup_empty_username;
                OpenNotification(title, message);
                isValid = false;
                TurnTextBlockRed(txtBkUsername);
            }
            if (player.PlayerUsername.Contains(" "))
            {
                string message = Properties.Langs.Resources.signup_space_username;
                OpenNotification(title, message);
                isValid = false;
                TurnTextBlockRed(txtBkUsername);
            }
            if (!client.IsUsernameAvailable(player.PlayerUsername))
            {
                string message = Properties.Langs.Resources.signup_username_already_used;
                OpenNotification(title, message);
                isValid = false;
                TurnTextBlockRed(txtBkUsername);
            }
            if (!client.ValidateEmail(player.PlayerEmail))
            {
                string message = Properties.Langs.Resources.signup_invalid_email;
                OpenNotification(title, message);
                isValid = false;
                TurnTextBlockRed(txtBkEmail);
            }
           
            return isValid;
        }

        private void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            ResetTextBlocks(txtBkUsername, txtBkEmail, txtBkPassword);

            var client = new ProfileManagerClient();

            var player = new Player
            {
                PlayerUsername = txtBxUsername.Text,
                PlayerEmail = txtBxEmail.Text,
                PlayerPassword = txtBxPassword.Text
            };

            if (ValidatePlayer(ref player, client))
            {
                player.PlayerPassword = BCrypt.Net.BCrypt.HashPassword(player.PlayerPassword);
                var result = client.SignUp(player);
                if (result)
                {
                    string title = Properties.Langs.Resources.successful_signup;
                    string message = Properties.Langs.Resources.successful_signup_message;
                    OpenNotification(title, message);
                    NavigationService.Navigate(new LoginPage());
                }
                else
                {
                    string title = Properties.Langs.Resources.error;
                    string message = Properties.Langs.Resources.signup_error;
                    OpenNotification(title, message);
                }
            }
            else
            {
                txtBkBoss.Text = Properties.Langs.Resources.boss_invalid_inputs;
            }
        }

        private void Login_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new LoginPage());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new MainPage());
        }
    }
}