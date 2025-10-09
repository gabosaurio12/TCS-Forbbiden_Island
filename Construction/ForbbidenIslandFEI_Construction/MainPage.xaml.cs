using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Numerics;
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
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LoginPage));

        public MainPage()
        {
            InitializeComponent();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Navega a la página PlayPage
                NavigationService.Navigate(new PlayPage());
                log.Info("Navegación a PlayPage desde MainPage.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir la página de juego.");
                log.Error("MainPage.xaml.cs - PlayButton_Click", ex);
            }
        }


        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SelectLanguageWindow selectLanguageWindow = new SelectLanguageWindow();
            selectLanguageWindow.ShowDialog();
        }

        private void ClearCurrentLogin()
        {
            using (var db = new Forbbiden_FEIEntities())
            {
                try
                {
                    var loggedInPlayers = db.LoginPlayer.ToList();
                    db.LoginPlayer.RemoveRange(loggedInPlayers);
                    db.SaveChanges();
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

        private void QuitGameButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
            ClearCurrentLogin();            
            log.Info("App clossed");
        }

        private Player GetCurrentLogin()
        {
            Player player = new Player();
            using (var db = new Forbbiden_FEIEntities())
            {
                try
                {
                    int current_id = db.LoginPlayer.Select(lp => lp.login_player_id).SingleOrDefault();
                    player = db.Player.Find(current_id);
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show("Error al leer usuario de la base de datos.");
                    log.Error("MainPage.xaml.cs", ex);
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show("Error al cargar el perfil.");
                    log.Error("MainPage.xaml.cs", ex);
                }
            }
            return player;
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            Player player = GetCurrentLogin();
            if (player == null)
            {
                NavigationService.Navigate(new ProfilePage());
            }
            else
            {
                NavigationService.Navigate(new ProfilePage(player));
            }
        }

        private void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            LogWindow login = new LogWindow();
            login.Show();
        }
    }
}
