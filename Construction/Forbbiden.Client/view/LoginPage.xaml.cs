using log4net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Forbbiden.Client.ProfileManager;

namespace Forbbiden.Client
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
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
                MessageBox.Show("El nombre de usuario no existe.");
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

                        // Cierra la ventana que contiene esta página
                        var parentWindow = Window.GetWindow(this);
                        parentWindow?.Close();

                        log.Info($"Usuario '{searchPlayer.PlayerUsername}' logged in.");
                    }
                    else
                    {
                        log.Warn($"Login failed for user '{searchPlayer.PlayerUsername}'.");
                        MessageBox.Show("Error al iniciar sesión. Por favor, inténtelo de nuevo.");
                    }
                }
                else
                {
                    txtBkPassword.Foreground = Brushes.Red;
                    MessageBox.Show("Contraseña incorrecta");
                }
            }
        }

        private void TogglePassword_Click(object sender, RoutedEventArgs e)
        {
            if (!_passwordVisible)
            {
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
            }
        }

        private void Signup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new SignupPage());
        }
    }
}