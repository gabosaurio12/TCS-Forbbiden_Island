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
using ForbbidenIslandFEI_Construction.model;

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
        
        private bool SetPlayer(out PlayerClient player)
        {
         
            player = new PlayerClient()
            {
                player_username = txtBUsername.Text,
                player_email = txtBEmail.Text,
                player_password = txtBPassword.Text
            };
            bool isValid = true;

            if (!player.ValidatePassword(player.player_password))
            {
                MessageBox.Show("La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un carácter especial.");
                isValid = false;
            }
            else
            {
                player.HashPassword(player);
            }
           
            return isValid;
        }

        private void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            PlayerClient player = new PlayerClient();

            SetPlayer(out player);
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