using log4net;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ForbbidenIslandFEI_Construction
{
    /// <summary>
    /// Lógica de interacción para LoginWindow.xaml
    /// </summary>
    public partial class SignupPage : Page
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SignupPage));
        public SignupPage()
        {
            InitializeComponent();
        }
        
        private bool SetPlayer(Player player)
        {
            player = new Player()
            {
                player_username = txtBUsername.Text,
                player_email = txtBEmail.Text,
                player_password = txtBPassword.Text
            };
            bool isValid = true;

            if (!player.ValidateUsername())
            {
                MessageBox.Show("El nombre de usuario ya existe.");
                isValid = false;
            }

            if (!player.ValidateEmail())
            {
                MessageBox.Show("El correo electrónico debe contener un @ o ya está registrado.");
                isValid = false;
            }

            if (!player.ValidatePassword())
            {
                MessageBox.Show("La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un carácter especial.");
                isValid = false;
            }
            else
            {
                player.hashPassword();
            }

            return isValid;
        }

        private void signupButton_Click(object sender, RoutedEventArgs e)
        {
            Player player = new Player();

            if (SetPlayer(player))
            {
                using (var db = new Forbbiden_FEIEntities())
                {
                    try
                    {
                        db.Player.Add(player);
                        db.SaveChanges();
                        MessageBox.Show("Éxito!");
                    }
                    catch (DbEntityValidationException ex)
                    {
                        MessageBox.Show("Error al registrar el usuario.");
                        log.Error("SignupWindow.xaml.cs", ex);
                    }
                    catch (DbUpdateException ex)
                    {
                        MessageBox.Show("Error al registrar el usuario.");
                        log.Error("SignupWindow.xaml.cs", ex);
                    }
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.NavigationService.Navigate(new LoginPage());
        }
    }
}