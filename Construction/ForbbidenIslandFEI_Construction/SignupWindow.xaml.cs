﻿using System;
using System.Collections.Generic;
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
    /// Lógica de interacción para LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private string[] GetTextFieldsData()
        {
            string username = txtBUsername.Text;
            string email = txtBEmail.Text;
            string password = pwdBPassword.Password;

            return new String[] { username, email, password };
        }

        private void signupButton_Click(object sender, RoutedEventArgs e)
        {
            string[] userData = GetTextFieldsData();
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
