using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ForbbidenIslandFEI_Construction
{
    /// <summary>
    /// Interaction logic for ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LoginPage));

        private Player _player;
        public ProfilePage()
        {
            InitializeComponent();
        }

        public ProfilePage(Player player)
        {
            InitializeComponent();
            _player = player;
            txtBxUsername.Text = player.player_username;
            txtBxEmail.Text = player.player_email;
            txtBxName.Text = player.player_name;
        }

        private void BtnDiscard_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new MainPage());
        }
        private void ResetFieldColors()
        {
            txtBkUsername.Foreground = Brushes.Black;
            txtBkName.Foreground = Brushes.Black;
            txtBkEmail.Foreground = Brushes.Black;
        }

        private bool SetPlayer(ref Player player)
        {
            player.player_username = txtBxUsername.Text;
            player.player_email = txtBxEmail.Text;
            player.player_name = txtBxName.Text;

            bool isValid = true;
            PlayerValidation playerValidation = new PlayerValidation();

            if (player.player_username != _player.player_username)
            {
                if (!playerValidation.ValidateUsername(player))
                {
                    MessageBox.Show("El nombre de usuario ya existe.");
                    txtBkUsername.Foreground = Brushes.Red;
                    isValid = false;
                }               
            }

            if (player.player_email != _player.player_email)
            {
                if (!playerValidation.ValidateEmail(player))
                {
                    MessageBox.Show("El correo electrónico debe contener un @ o ya está registrado.");
                    txtBkEmail.Foreground = Brushes.Red;
                    isValid = false;
                }
            }

            return isValid;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            ResetFieldColors();
            using (var db = new Forbbiden_FEIEntities())
            {
                try
                {
                    Player updatePlayer = db.Player.Find(_player.player_id);
                    if (updatePlayer != null && SetPlayer(ref updatePlayer))
                    {
                        db.SaveChanges();
                        MessageBox.Show("Usuario actualizado!");
                    }
                }
                catch (DbEntityValidationException ex)
                {
                    MessageBox.Show("Error al cerrar sesión.");
                    log.Error("SignupWindow.xaml.cs", ex);
                }
                catch (DbUpdateException ex)
                {
                    MessageBox.Show("Error al cerrar sesión.");
                    log.Error("SignupWindow.xaml.cs", ex);
                }
            }
        }
    }
}
