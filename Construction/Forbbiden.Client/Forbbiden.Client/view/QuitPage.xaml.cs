using Forbbiden.Client.Exceptions;
using Forbbiden.Client.Logic;
using Forbbiden.Client.Model;
using Forbbiden.Client.Repositories;
using log4net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Forbbiden.Client.View
{
    /// <summary>
    /// Interaction logic for QuitPage.xaml
    /// </summary>
    public partial class QuitPage : Page
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(QuitPage));

        public QuitPage()
        {
            InitializeComponent();
            ViewUtils.SetBackground(background);

            if (ClientSession.CurrentPlayerId > 0)
            {
                logOutButton.Visibility = Visibility.Visible;
            }
        }

        private async Task DisconnectPlayer(string username)
        {

            try
            {
                await ProfileRepository.DisconnectPlayer(username);
            }
            catch (ViewException ex)
            {
                ExceptionViewManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            _ = DisconnectPlayer(ClientSession.Username);
            Application.Current.Shutdown();
            Log.Info("App closed");
        }

        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            _= DisconnectPlayer(ClientSession.Username);
            ClientSession.SetGuestSession();
            Properties.PlayerSettings.Default.CurrentPlayerId = 0;
            Properties.PlayerSettings.Default.Save();
            NavigationService?.Navigate(new MainPage());
            Log.Info("Player logged out");
        }
    }
}
