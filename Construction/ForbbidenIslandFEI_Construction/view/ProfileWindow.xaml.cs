using ForbbidenIslandFEI_Construction.ProfileManager;
using log4net;
using System.Windows;
using System.Windows.Media;

namespace ForbbidenIslandFEI_Construction
{
    /// <summary>
    /// Interaction logic for ProfileWindow.xaml
    /// </summary>
    public partial class ProfileWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LoginPage));

        private Player _player;
        public ProfileWindow()
        {
            InitializeComponent();
        }

        public ProfileWindow(Player player)
        {
            InitializeComponent();
            _player = player;
            txtBxUsername.Text = player.PlayerUsername;
            txtBxEmail.Text = player.PlayerEmail;
            txtBxName.Text = player.PlayerName;
        }

        private void BtnDiscard_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ResetFieldColors()
        {
            txtBkUsername.Foreground = Brushes.Black;
            txtBkName.Foreground = Brushes.Black;
            txtBkEmail.Foreground = Brushes.Black;
        }

        private void ValidateUsername(string username, ref bool isValid)
        {
            var client = new ProfileManagerClient();

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("El nombre de usuario no puede estar vacío.");
                txtBkUsername.Foreground = Brushes.Red;
                isValid = false;
            }
            else
            {
                if (!client.IsUsernameAvailable(username))
                {
                    MessageBox.Show("El nombre de usuario ya existe.");
                    txtBkUsername.Foreground = Brushes.Red;
                    isValid = false;
                }
            }
        }

        private void ValidateEmail(string email, ref bool isValid)
        {
            var client = new ProfileManagerClient();

            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("El correo no puede estar vació.");
                txtBkEmail.Foreground = Brushes.Red;
                isValid = false;
            }
            else
            {
                if (!client.IsEmailAvailable(email))
                {
                    MessageBox.Show("El correo electrónico debe contener un @ o ya está registrado.");
                    txtBkEmail.Foreground = Brushes.Red;
                    isValid = false;
                }
            }
        }

        private bool SetPlayer(ref Player player)
        {
            player.PlayerUsername = txtBxUsername.Text;
            player.PlayerEmail = txtBxEmail.Text;
            player.PlayerName = txtBxName.Text;

            bool isValid = true;

            if (player.PlayerUsername != _player.PlayerUsername)
            {
                ValidateUsername(player.PlayerUsername, ref isValid);
            }

            if (player.PlayerEmail != _player.PlayerEmail)
            {
                ValidateEmail(player.PlayerEmail, ref isValid);
            }

            return isValid;
        }
        

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            ResetFieldColors();
            var client = new ProfileManagerClient();

            Player updatedPlayer = new Player();
            
            if (SetPlayer(ref updatedPlayer))
            {
                updatedPlayer.PlayerId = _player.PlayerId;
                if (client.UpdatePlayer(updatedPlayer))
                {
                    MessageBox.Show("Perfil actualizado correctamente.");
                }
                else
                {
                    MessageBox.Show("Error al actualizar el perfil.");
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
