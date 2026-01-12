using Forbbiden.Client.Exceptions;
using Forbbiden.Client.Logic;
using Forbbiden.Client.Model;
using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.Repositories;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Forbbiden.Client
{
    /// <summary>
    /// LoginWindow.xaml interaction logic
    /// </summary>
    public partial class LoginPage : Page
    {
        private bool PasswordVisible = false;
        public LoginPage()
        {
            InitializeComponent();
            ViewUtils.SetBackground(background);
        }

        private static void ResetFields(TextBlock txtBkUser, TextBlock txtBkPassword, TextBlock txtBkBoss)
        {
            txtBkBoss.Text = Properties.Resources.bossLogin;
            txtBkUser.Foreground = Brushes.White;
            txtBkPassword.Foreground = Brushes.White;
        }

        private static void BrushTextBlock(TextBlock txtBk)
        {
            txtBk.Foreground = Brushes.Red;
        }

        private Player GetPlayerInput()
        {
            string username = txtBxUsername.Text.Trim();

            string password;
            if (chkPassword.IsChecked == true)
            {
                password = txtBxPasswordVisible.Text.Trim();
            }
            else
            {
                password = pwdBxPassword.Password.Trim();
            }

            return new Player
            {
                PlayerUsername = username,
                PlayerPassword = password,
            };
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            ResetFields(txtBkUser, txtBkPassword, txtBkBoss);

            Player player = GetPlayerInput();

            try
            {
                player = await ProfileRepository.LoginPlayer(player.PlayerUsername, player.PlayerPassword);
            }
            catch (ViewException ex)
            {
                ExceptionViewManager.ShowViewExceptionNotification(
                    ex, Window.GetWindow(this));
            }

            if (player.PlayerId > 0)
            {
                ClientSession.SetPlayer(player);
                Properties.PlayerSettings.Default.CurrentPlayerId = ClientSession.CurrentPlayerId;
                Properties.PlayerSettings.Default.Save();

                NavigationService?.Navigate(new MainPage());
            }
            else if (player.PlayerId == -1)
            {
                BrushTextBlock(txtBkUser);
                txtBkBoss.Text = Properties.Resources.usernameNoExists;
            }
            else
            {
                BrushTextBlock(txtBkPassword);
                txtBkBoss.Text = Properties.Resources.wrongPassword;
            }
        }

        private void TogglePassword_Click(object sender, RoutedEventArgs e)
        {
            string projectPath = ViewUtils.GetProjectDir();

            if (!PasswordVisible)
            {
                txtBxPasswordVisible.Text = pwdBxPassword.Password;
                txtBxPasswordVisible.Visibility = Visibility.Visible;
                pwdBxPassword.Visibility = Visibility.Collapsed;
                PasswordVisible = true;
                
                string darkBossPath = Path.Combine(projectPath, "Images", "bossdark.png");
                bossImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(darkBossPath));
            }
            else
            {
                pwdBxPassword.Password = txtBxPasswordVisible.Text;
                txtBxPasswordVisible.Visibility = Visibility.Collapsed;
                pwdBxPassword.Visibility = Visibility.Visible;
                PasswordVisible = false;

                string bossPath = Path.Combine(projectPath, "Images", "boss.png");
                bossImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(bossPath));
            }
        }

        private void Signup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new SignupPage());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new MainPage());
        }
    }
}