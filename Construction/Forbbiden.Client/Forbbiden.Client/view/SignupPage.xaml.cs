using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.view.info;
using log4net;
using System;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        private static readonly ILog Log = LogManager.GetLogger(typeof(SignupPage));
        private const int PasswordMinLength = 7;
        private readonly ProfileManagerClient Client;
        public SignupPage()
        {
            InitializeComponent();

            Client = new ProfileManagerClient();
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

        private async Task<bool> ValidatePlayer(Player player)
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
            if (!await Client.IsUsernameAvailableAsync(player.PlayerUsername))
            {
                string message = Properties.Langs.Resources.signup_username_already_used;
                OpenNotification(title, message);
                isValid = false;
                TurnTextBlockRed(txtBkUsername);
            }
            if (!await Client.ValidateEmailAsync(player.PlayerEmail))
            {
                string message = Properties.Langs.Resources.signup_invalid_email;
                OpenNotification(title, message);
                isValid = false;
                TurnTextBlockRed(txtBkEmail);
            }
           
            return isValid;
        }

        private async Task VerifyPlayer(string username)
        {
            var player = await Client.GetPlayerByUsernameAsync(username, false);
            var verificationWindow = new VerificationWIndow(player.PlayerId)
            {
                Owner = Window.GetWindow(this)
            };
            verificationWindow.ShowDialog();
        }

        private async void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            ResetTextBlocks(txtBkUsername, txtBkEmail, txtBkPassword);

            var player = new Player
            {
                PlayerUsername = txtBxUsername.Text,
                PlayerEmail = txtBxEmail.Text,
                PlayerPassword = txtBxPassword.Text
            };

            if (await ValidatePlayer(player))
            {
                player.PlayerPassword = BCrypt.Net.BCrypt.HashPassword(player.PlayerPassword);
                var playerId = await Client.SignUpAsync(player);
                if (playerId != -1)
                {
                    if (await Client.SendEmailAsync(player.PlayerEmail, playerId))
                    {
                        string title = Properties.Langs.Resources.successful_signup;
                        string message = Properties.Langs.Resources.successful_signup_message;
                        OpenNotification(title, message);
                        await VerifyPlayer(player.PlayerUsername);
                        NavigationService?.Navigate(new LoginPage());
                    }
                    else
                    {
                        string title = Properties.Langs.Resources.error;
                        string message = Properties.Langs.Resources.send_email_error;
                        OpenNotification(title, message);
                    }
                    
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