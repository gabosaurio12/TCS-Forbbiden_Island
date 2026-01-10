using Forbbiden.Client.Exceptions;
using Forbbiden.Client.Logic;
using Forbbiden.Client.Logic;
using Forbbiden.Client.Logic.Validations;
using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.Repositories;
using Forbbiden.Client.TokenManager;
using Forbbiden.Client.View.info;
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
        private readonly ProfileRepository ProfileRepo;
        public SignupPage()
        {
            InitializeComponent();

            ProfileRepo = new ProfileRepository();
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

        private bool ValidatePlayerData(Player player)
        {
            bool isValid = true;

            var usernameValidationResults = ValidationUtils.ValidateUsername(player.PlayerUsername);
            if (!usernameValidationResults.IsValid)
            {
                TurnTextBlockRed(txtBkUsername);
                ErrorsNotificationManager.ShowUsernameValidationErrors(
                    usernameValidationResults.Errors, Window.GetWindow(this));
                isValid = false;
            }

            var emailValidationResults = ValidationUtils.ValidateEmail(player.PlayerEmail);
            if (!emailValidationResults.IsValid)
            {
                TurnTextBlockRed(txtBkEmail);
                ErrorsNotificationManager.ShowEmailValidationErrors(
                    emailValidationResults.Errors, Window.GetWindow(this));
                isValid = false;
            }

            var passwordValidationResults = ValidationUtils.ValidatePassword(player.PlayerPassword);
            if (!passwordValidationResults.IsValid)
            {
                TurnTextBlockRed(txtBkPassword);
                ErrorsNotificationManager.ShowPasswordValidationErrors(
                    passwordValidationResults.Errors, Window.GetWindow(this));
                isValid = false;
            }

            return isValid;
        }

        private async Task<bool> ValidatePlayer(Player player)
        {
            bool isValid = ValidatePlayerData(player);
            string title = Properties.Resources.invalid_input;

            if (isValid)
            {
                try
                {
                    if (!await ProfileRepo.IsUsernameAvailable(player.PlayerUsername))
                    {
                        string message = Properties.Resources.signup_username_already_used;
                        ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
                        TurnTextBlockRed(txtBkUsername);
                        isValid = false;
                    }

                    if (!await ProfileRepo.IsEmailAvailable(player.PlayerEmail))
                    {
                        string message = Properties.Resources.invalid_email_not_available;
                        ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
                        TurnTextBlockRed(txtBkEmail);
                        isValid = false;
                    }
                }
                catch (ViewException ex)
                {
                    ErrorsNotificationManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
                }
            }


            return isValid;
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
                int playerId = -1;
                try
                {
                    playerId = await ProfileRepo.SignupPlayer(player);
                }
                catch (ViewException ex)
                {
                    ErrorsNotificationManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
                }

                if (playerId != -1)
                {
                    string title = Properties.Resources.successful_signup;
                    string message = Properties.Resources.successful_signup_message;
                    ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
                    NavigationService?.Navigate(new LoginPage());
                }
                else
                {
                    string title = Properties.Resources.error;
                    string message = Properties.Resources.signup_error;
                    ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
                    _ = ProfileRepo.DeletePlayer(player.PlayerUsername);
                }
            }
            else
            {
                txtBkBoss.Text = Properties.Resources.boss_invalid_inputs;
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