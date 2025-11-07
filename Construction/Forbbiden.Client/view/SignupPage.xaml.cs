using Forbbiden.Client.ProfileManager;
using Forbbiden.Contracts;
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

            if (!ValidatePassword(player.PlayerPassword))
            {
                MessageBox.Show("La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un carácter especial.");
                isValid = false;
                TurnTextBlockRed(txtBkPassword);
            }
            if (string.IsNullOrWhiteSpace(player.PlayerUsername))
            {
                MessageBox.Show("El nombre de usuario no puede estar vacío.");
                isValid = false;
                TurnTextBlockRed(txtBkUsername);
            }
            if (player.PlayerUsername.Contains(" "))
            {
                MessageBox.Show("El nombre de usuario no puede contener espacios.");
                isValid = false;
                TurnTextBlockRed(txtBkUsername);
            }
            if (!client.IsUsernameAvailable(player.PlayerUsername))
            {
                MessageBox.Show("El nombre de usuario ya está en uso.");
                isValid = false;
                TurnTextBlockRed(txtBkUsername);
            }
            if (!client.ValidateEmail(player.PlayerEmail))
            {
                MessageBox.Show("El correo electrónico es inválido o ya está en uso.");
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
                if (client.SignUp(player))
                {
                    log.Info("Usuario {player.PlayerUsername} sent.");
                    MessageBox.Show("Usuario registrado exitosamente.");
                    NavigationService.Navigate(new LoginPage());
                }
                else
                {
                    MessageBox.Show("Error al registrar el usuario. Inténtelo de nuevo más tarde.");
                }
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