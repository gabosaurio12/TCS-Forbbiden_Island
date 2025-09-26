using log4net;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Net.Mail;
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
        
        private bool SetPlayer(out Player player)
        {
            player = new Player()
            {
                player_username = txtBUsername.Text,
                player_email = txtBEmail.Text,
                player_password = txtBPassword.Text
            };
            bool isValid = true;
            PlayerValidation playerValidation = new PlayerValidation();

            try
            {
                if (!playerValidation.ValidateUsername(player))
                {
                    MessageBox.Show("El nombre de usuario ya existe.");
                    isValid = false;
                }

                if (!playerValidation.ValidateEmail(player))
                {
                    MessageBox.Show("El correo electrónico debe contener un @ o ya está registrado.");
                    isValid = false;
                }

                if (!playerValidation.ValidatePassword(player))
                {
                    MessageBox.Show("La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un carácter especial.");
                    isValid = false;
                }
                else
                {
                    playerValidation.hashPassword(player);
                }
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

            return isValid;
        }

        private void SendEmail()
        {
            string receiver = txtBEmail.Text;
            string emisor = "forbbidenislandfei@gmail.com";
            MailMessage message = new MailMessage(emisor, receiver);
            message.Subject = "Register confirmation";
            message.Body = "Your account has been succesfully created, welcome to Forbbiden Island FEI Edition. Enjoy the adventure!";
            SmtpClient client = new SmtpClient("smtp.gmail.com");
            client.Port = 587;
            client.Credentials = new System.Net.NetworkCredential(emisor, "uqeosliojdotaitq");
            client.EnableSsl = true;

            try
            {
                client.Send(message);
                MessageBox.Show("Correo de confirmación enviado.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al enviar el correo de confirmación.");
                log.Error("SignupWindow.xaml.cs", ex);
            }
        }

        private void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            Player player = new Player();

            if (SetPlayer(out player))
            {
                using (var db = new Forbbiden_FEIEntities())
                {
                    try
                    {
                        db.Player.Add(player);
                        db.SaveChanges();
                        SendEmail();
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

        private void Login_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.NavigationService.Navigate(new LoginPage());
        }
    }
}