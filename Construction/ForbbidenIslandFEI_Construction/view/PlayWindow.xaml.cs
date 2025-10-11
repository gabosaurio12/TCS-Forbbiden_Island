using System.Windows;
using System.Windows.Controls;

namespace ForbbidenIslandFEI_Construction
{
    public partial class PlayWindow : Window
    {
        public PlayWindow()
        {
            InitializeComponent();
            ShowHostGame();
        }

        private void HostButton_Click(object sender, RoutedEventArgs e)
        {
            ShowHostGame();
        }

        private void OnlineButton_Click(object sender, RoutedEventArgs e)
        {
            ShowOnline();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionalidad de 'New Game' aún no implementada.");
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

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
