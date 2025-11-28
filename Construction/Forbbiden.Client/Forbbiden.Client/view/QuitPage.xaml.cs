using Forbbiden.Client.logic;
using Forbbiden.Client.ProfileManager;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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

namespace Forbbiden.Client.view
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
        }

        private async void DisconnectPlayer(string username)
        {
            var client = new ProfileManagerClient();

            try
            {
                await client.DisconnectPlayerByUsernameAsync(username);
            }
            catch (FaultException<DBFault> dbFault)
            {
                Log.Error("ERROR: LoginPage.ConnectPlayer", dbFault);
                ViewUtils.ShowPushError(Window.GetWindow(this));
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
            DisconnectPlayer(ClientSession.Username);
            Application.Current.Shutdown();
            Log.Info("App closed");
        }

        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            DisconnectPlayer(ClientSession.Username);
            Properties.PlayerSettings.Default.CurrentPlayerId = 0;
            Properties.PlayerSettings.Default.Save();
            Application.Current.Shutdown();
            Log.Info("App closed");
            
        }
    }
}
