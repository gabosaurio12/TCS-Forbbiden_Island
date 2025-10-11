using System.Windows;
using System.Windows.Controls;

namespace ForbbidenIslandFEI_Construction
{
    public partial class PlayPage : Page
    {
        public PlayPage()
        {
            InitializeComponent();
            // Al cargar, mostramos por defecto Host Game
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
            // Aquí se implementará la lógica para "New Game"
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
            // Por ahora un placeholder, luego se sustituye con OnlineControl
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
    }
}
