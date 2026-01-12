using Forbbiden.Client.Exceptions;
using Forbbiden.Client.Logic;
using Forbbiden.Client.Logic.Validations;
using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.Repositories;
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
        public SignupPage()
        {
            InitializeComponent();
            ViewUtils.SetBackground(background);
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
                ExceptionViewManager.ShowUsernameValidationErrors(
                    usernameValidationResults.Errors, Window.GetWindow(this));
                isValid = false;
            }

            var emailValidationResults = ValidationUtils.ValidateEmail(player.PlayerEmail);
            if (!emailValidationResults.IsValid)
            {
                TurnTextBlockRed(txtBkEmail);
                ExceptionViewManager.ShowEmailValidationErrors(
                    emailValidationResults.Errors, Window.GetWindow(this));
                isValid = false;
            }

            var passwordValidationResults = ValidationUtils.ValidatePassword(player.PlayerPassword);
            if (!passwordValidationResults.IsValid)
            {
                TurnTextBlockRed(txtBkPassword);
                ExceptionViewManager.ShowPasswordValidationErrors(
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
                    if (!await ProfileRepository.IsUsernameAvailable(player.PlayerUsername))
                    {
                        string message = Properties.Resources.signup_username_already_used;
                        ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
                        TurnTextBlockRed(txtBkUsername);
                        isValid = false;
                    }

                    if (!await ProfileRepository.IsEmailAvailable(player.PlayerEmail))
                    {
                        string message = Properties.Resources.invalid_email_not_available;
                        ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
                        TurnTextBlockRed(txtBkEmail);
                        isValid = false;
                    }
                }
                catch (ViewException ex)
                {
                    ExceptionViewManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
                }
            }


            return isValid;
        }

        private async void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            ResetTextBlocks(txtBkUsername, txtBkEmail, txtBkPassword);

            var player = new Player
            {
                PlayerUsername = txtBxUsername.Text.Trim(),
                PlayerEmail = txtBxEmail.Text.Trim(),
                PlayerPassword = txtBxPassword.Text.Trim()
            };

            if (await ValidatePlayer(player))
            {
                int playerId = -1;
                try
                {
                    playerId = await ProfileRepository.SignupPlayer(player);
                }
                catch (ViewException ex)
                {
                    ExceptionViewManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
                }

                if (playerId != -1)
                {
                    string title = Properties.Resources.successful_signup;
                    string message = Properties.Resources.successful_signup_message;
                    ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
                    NavigationService?.Navigate(new LoginPage());
                }
                else if (playerId == -2)
                {
                    string title = Properties.Resources.invalid_input;
                    string message = Properties.Resources.invaild_username_email_occupied;
                    ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
                    _ = ProfileRepository.DeletePlayer(player.PlayerUsername);
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