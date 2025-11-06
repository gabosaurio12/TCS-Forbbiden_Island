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
            ShowHostGame(TabContent, HostButton, OnlineButton);
        }

        private void HostButton_Click(object sender, RoutedEventArgs e)
        {
            ShowHostGame(TabContent, HostButton, OnlineButton);
            HostButton.IsDefault = true;
            OnlineButton.IsDefault = false;
        }

        private void OnlineButton_Click(object sender, RoutedEventArgs e)
        {
            ShowOnline(TabContent, HostButton, OnlineButton);
            OnlineButton.IsDefault = true;
            HostButton.IsDefault = false;
        }

        private static void ShowHostGame(ContentControl TabContent, Button HostButton, Button OnlineButton)
        {
            TabContent.Content = new HostGameControl();
            HostButton.Background = System.Windows.Media.Brushes.LightGray;
            OnlineButton.Background = System.Windows.Media.Brushes.Gainsboro;
        }

        private static void ShowOnline(ContentControl TabContent, Button HostButton, Button OnlineButton)
        {
            TabContent.Content = new JoinGameControl();
            OnlineButton.Background = System.Windows.Media.Brushes.LightGray;
            HostButton.Background = System.Windows.Media.Brushes.Gainsboro;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        private void FriendsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new FriendsPage());
        }
    }
}
