using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.view.info;
using log4net;
using System;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;
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
        private static readonly ILog log = LogManager.GetLogger(typeof(LoginPage));
        private readonly ProfileManagerClient client = new ProfileManagerClient();
        private bool passwordVisible = false;
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

        private static bool ValidatePassword(string passwordTry, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(passwordTry, hashedPassword);
        }

        private async Task LoginPlayer(Player player)
        {
            bool loggedIn = false;
            try
            {
                loggedIn = await client.LoginAsync(player);
            }
            catch (FaultException<DBFault> dbFault)
            {
                log.Error(dbFault.Detail);
                string title = Properties.Langs.Resources.error;
                string message = Properties.Langs.Resources.loginError;
                var notificationWindow = new NotificationWindow(title, message)
                {
                    Owner = Window.GetWindow(this)
                };
                notificationWindow.ShowDialog();
            }

            if (loggedIn)
            {

                NavigationService?.Navigate(new MainPage());

                log.Info("User '{searchPlayer.PlayerUsername}' logged in.");
            }
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            ResetFields(txtBkUser, txtBkPassword, txtBkBoss);

            string username = txtBxUsername.Text.Trim();

            var searchPlayer = await client.GetPlayerByUsernameAsync(username, false);

            if (searchPlayer.PlayerId == -1)
            {
                txtBkBoss.Text = Properties.Langs.Resources.usernameNoExists;
                BrushTextBlock(txtBkUser);
            }
            else
            {
                string password = "";
                if (chkPassword.IsChecked == true)
                {
                    password = txtBxPasswordVisible.Text;
                }
                else
                {
                    password = pwdBxPassword.Password;
                }

                if (ValidatePassword(password, searchPlayer.PlayerPassword))
                {
                    await LoginPlayer(searchPlayer);
                }
                else
                {
                    BrushTextBlock(txtBkPassword);
                    txtBkBoss.Text = Properties.Langs.Resources.wrongPassword;
                }
            }
        }

        private void TogglePassword_Click(object sender, RoutedEventArgs e)
        {
            string projectPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\.."));

            if (!passwordVisible)
            {
                string darkBossPath = Path.Combine(projectPath, "Images", "bossdark.png");
                bossImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(darkBossPath));

                txtBxPasswordVisible.Text = pwdBxPassword.Password;
                txtBxPasswordVisible.Visibility = Visibility.Visible;
                pwdBxPassword.Visibility = Visibility.Collapsed;
                passwordVisible = true;
            }
            else
            {
                pwdBxPassword.Password = txtBxPasswordVisible.Text;
                txtBxPasswordVisible.Visibility = Visibility.Collapsed;
                pwdBxPassword.Visibility = Visibility.Visible;
                passwordVisible = false;

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