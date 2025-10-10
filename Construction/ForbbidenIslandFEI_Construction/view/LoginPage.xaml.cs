using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
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
using System.Windows.Shapes;

namespace ForbbidenIslandFEI_Construction
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

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            ResetFieldColors();

            Player player = new Player();
            using (var db = new Forbbiden_FEIEntities())
            {
                player = db.Player.FirstOrDefault(p => p.player_username == txtBUsername.Text);
            }

            if (player == null)
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

                if (BCrypt.Net.BCrypt.Verify(password, player.player_password))
                {
                    using (var db = new Forbbiden_FEIEntities())
                    {
                        db.LoginPlayer.Add(new LoginPlayer
                        {
                            login_player_id = player.player_id,
                        });
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (DbEntityValidationException ex)
                        {
                            MessageBox.Show("Error al iniciar sesión.");
                            log.Error("SignupWindow.xaml.cs", ex);
                        }
                        catch (DbUpdateException ex)
                        {
                            MessageBox.Show("Error al iniciasr sesión.");
                            log.Error("SignupWindow.xaml.cs", ex);
                        }
                    }

                    var logWindow = Window.GetWindow(this) as LogWindow;
                    if (logWindow != null)
                    {
                        logWindow.Close();
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
            this.NavigationService.Navigate(new SignupPage());
        }
    }
}