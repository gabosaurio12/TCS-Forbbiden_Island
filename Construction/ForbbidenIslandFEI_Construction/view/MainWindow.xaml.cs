using log4net;
using System;
using System.Windows;
using ForbbidenIslandFEI_Construction.ProfileManager;

namespace ForbbidenIslandFEI_Construction
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LoginPage));

        public MainWindow()
        {
            InitializeComponent();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Hide();
                var ventana = new PlayWindow();
                ventana.ShowDialog();
                Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir la página de juego.");
                log.Error("MainPage.xaml.cs - PlayButton_Click", ex);
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            var selectLanguageWindow = new SelectLanguageWindow();
            selectLanguageWindow.ShowDialog();
            Show();
        }

        private void QuitGameButton_Click(object sender, RoutedEventArgs e)
        {
            var client = new ProfileManagerClient();
            if (!client.ClearCurrentLogin())
            {
                MessageBox.Show("Error al cerrar sesión en la base de datos.");
            }
            Application.Current.Shutdown();
            log.Info("App clossed");
        }       

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {

            var client = new ProfileManagerClient();
            Player player = client.GetCurrentLogin();
            Hide(); 

            Window ventana;

            if (player == null)
            {
                ventana = new ProfileWindow(); 
            }
            else
            {
                ventana = new ProfileWindow(player); 
            }

            ventana.ShowDialog(); 
            Show(); 
        }

        private void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            LogWindow login = new LogWindow();
            login.Show();
        }
    }
}
