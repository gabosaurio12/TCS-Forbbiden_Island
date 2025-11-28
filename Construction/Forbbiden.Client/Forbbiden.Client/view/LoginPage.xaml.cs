using Forbbiden.Client.logic;
using Forbbiden.Client.ProfileManager;
using log4net;
using System;
using System.IO;
using System.ServiceModel;
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
        private static readonly ILog Log = LogManager.GetLogger(typeof(LoginPage));
        private bool PasswordVisible = false;
        public LoginPage()
        {
            InitializeComponent();
        }

        private static void ResetFields(TextBlock txtBkUser, TextBlock txtBkPassword, TextBlock txtBkBoss)
        {
            txtBkBoss.Text = Properties.Langs.Resources.bossLogin;
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
                var client = new ProfileManagerClient();
                player = await client.LoginAsync(player.PlayerUsername, player.PlayerPassword);
            }
            catch (FaultException<DBFault> dbFault)
            {
                Log.Error("ERROR: LoginPage.BtnLogin_Click", dbFault);
                ViewUtils.ShowPullError(Window.GetWindow(this));
            }

            if (player.PlayerId > 0)
            {
                ClientSession.SetPlayer(player);
                Properties.PlayerSettings.Default.CurrentPlayerId = ClientSession.CurrentPlayerId;
                Properties.PlayerSettings.Default.Save();

                NavigationService?.Navigate(new MainPage());
                Log.Info("Player logged in.");
            }
            else if (player.PlayerId == -1)
            {
                BrushTextBlock(txtBkUser);
                txtBkBoss.Text = Properties.Langs.Resources.usernameNoExists;
            }
            else
            {
                BrushTextBlock(txtBkPassword);
                txtBkBoss.Text = Properties.Langs.Resources.wrongPassword;
            }
        }

        private void TogglePassword_Click(object sender, RoutedEventArgs e)
        {
            string projectPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\.."));

            if (!PasswordVisible)
            {
                string darkBossPath = Path.Combine(projectPath, "Images", "bossdark.png");
                bossImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(darkBossPath));

                txtBxPasswordVisible.Text = pwdBxPassword.Password;
                txtBxPasswordVisible.Visibility = Visibility.Visible;
                pwdBxPassword.Visibility = Visibility.Collapsed;
                PasswordVisible = true;
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