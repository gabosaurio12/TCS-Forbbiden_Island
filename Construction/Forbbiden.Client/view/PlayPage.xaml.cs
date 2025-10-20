using Forbbiden.Client.view;
using log4net;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Forbbiden.Client
{
    public partial class PlayPage : Page
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LoginPage));
        public PlayPage()
        {
            InitializeComponent();
            ShowHostGame();
        }

        private void HostButton_Click(object sender, RoutedEventArgs e)
        {
            ShowHostGame();
            HostButton.IsDefault = true;
            OnlineButton.IsDefault = false;
        }

        private void OnlineButton_Click(object sender, RoutedEventArgs e)
        {
            ShowOnline();
            OnlineButton.IsDefault = true;
            HostButton.IsDefault = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.NavigationService?.Navigate(new LobbyPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir la configuración.");
                log.Error("PlayPage.xaml.cs - LobbyButton_Click", ex);
            }
        }
      
            

        private void ShowHostGame()
        {
            TabContent.Content = new HostGameControl();
            HostButton.Background = System.Windows.Media.Brushes.LightGray;
            OnlineButton.Background = System.Windows.Media.Brushes.Gainsboro;
        }

        private void ShowOnline()
        {
            TabContent.Content = new TextBlock
            {
                Text = "Online mode (en construcción)",
                FontSize = 32,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            OnlineButton.Background = System.Windows.Media.Brushes.LightGray;
            HostButton.Background = System.Windows.Media.Brushes.Gainsboro;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService != null && this.NavigationService.CanGoBack)
                this.NavigationService.GoBack();
        }
    }
}
