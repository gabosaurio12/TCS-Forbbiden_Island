using Forbbiden.Client.ProfileManager;
using log4net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Forbbiden.Client
{
    /// <summary>
    /// LoginWindow.xaml logic interaction
    /// </summary>
    public partial class SignupPage : Page
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SignupPage));
        public SignupPage()
        {
            InitializeComponent();
        }
        private bool ValidatePassword(string password)
        {
            if (!string.IsNullOrWhiteSpace(password) && password.Length > 7)
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]")) return false;
                if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-z]")) return false;
                if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[0-9]")) return false;
                if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[\W_]")) return false;
            }
            else
            {
                return false;
            }
            return true;
        }

        private bool ValidatePlayer(ref Player player, ProfileManagerClient client)
        {
            bool isValid = true;

            if (!ValidatePassword(player.PlayerPassword))
            {
                MessageBox.Show("La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un carácter especial.");
                isValid = false;
            }
            if (string.IsNullOrWhiteSpace(player.PlayerUsername))
            {
                MessageBox.Show("El nombre de usuario no puede estar vacío.");
                isValid = false;
            }
            if (player.PlayerUsername.Contains(" "))
            {
                MessageBox.Show("El nombre de usuario no puede contener espacios.");
                isValid = false;
            }
            if (!client.IsUsernameAvailable(player.PlayerUsername))
            {
                MessageBox.Show("El nombre de usuario ya está en uso.");
                isValid = false;
            }
            if (!client.ValidateEmail(player.PlayerEmail))
            {
                MessageBox.Show("El correo electrónico es inválido o ya está en uso.");
                isValid = false;
            }
           
            return isValid;
        }

        private void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            var client = new ProfileManagerClient();

            var player = new Player
            {
                PlayerUsername = txtBUsername.Text,
                PlayerEmail = txtBEmail.Text,
                PlayerPassword = txtBPassword.Text
            };

            if (ValidatePlayer(ref player, client))
            {
                player.PlayerPassword = BCrypt.Net.BCrypt.HashPassword(player.PlayerPassword);
                if (client.SignUp(player))
                {
                    log.Info($"Usuario {player.PlayerUsername} sent.");
                    MessageBox.Show("Usuario registrado exitosamente.");
                    NavigationService.Navigate(new LoginPage());
                }
                else
                {
                    MessageBox.Show("Error al registrar el usuario. Inténtelo de nuevo más tarde.");
                }
            }         
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Login_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new LoginPage());
        }
    }
}