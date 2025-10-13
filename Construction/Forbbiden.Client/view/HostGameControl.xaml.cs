using System.Windows;
using System.Windows.Controls;

namespace Forbbiden.Client
{
    public partial class HostGameControl : UserControl
    {
        public HostGameControl()
        {
            InitializeComponent();

            // Inicializar visibilidad al cargar el control
            this.Loaded += HostGameControl_Loaded;
        }

        private void HostGameControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Inicializar dificultad
            if (NormalButton != null && HardButton != null &&
                NormalMessage != null && HardMessage != null)
            {
                NormalMessage.Visibility = Visibility.Visible;
                HardMessage.Visibility = Visibility.Collapsed;
                NormalButton.Background = System.Windows.Media.Brushes.LightGreen;
                HardButton.Background = System.Windows.Media.Brushes.LightCoral;
            }

            // Inicializar toggle público/privado
            if (PublicToggle != null && PublicMessage != null && PrivateMessage != null)
            {
                if (PublicToggle.IsChecked == true)
                {
                    PublicMessage.Visibility = Visibility.Visible;
                    PrivateMessage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    PublicMessage.Visibility = Visibility.Collapsed;
                    PrivateMessage.Visibility = Visibility.Visible;
                }
            }
        }

        // Botones de dificultad
        private void NormalButton_Click(object sender, RoutedEventArgs e)
        {
            if (NormalMessage != null) NormalMessage.Visibility = Visibility.Visible;
            if (HardMessage != null) HardMessage.Visibility = Visibility.Collapsed;

            if (NormalButton != null) NormalButton.Background = System.Windows.Media.Brushes.LightGreen;
            if (HardButton != null) HardButton.Background = System.Windows.Media.Brushes.LightCoral;
        }

        private void HardButton_Click(object sender, RoutedEventArgs e)
        {
            if (NormalMessage != null) NormalMessage.Visibility = Visibility.Collapsed;
            if (HardMessage != null) HardMessage.Visibility = Visibility.Visible;

            if (HardButton != null) HardButton.Background = System.Windows.Media.Brushes.LightGreen;
            if (NormalButton != null) NormalButton.Background = System.Windows.Media.Brushes.LightGray;
        }

        // Toggle público/privado
        private void PublicToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (PublicMessage != null) PublicMessage.Visibility = Visibility.Visible;
            if (PrivateMessage != null) PrivateMessage.Visibility = Visibility.Collapsed;
        }

        private void PublicToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (PublicMessage != null) PublicMessage.Visibility = Visibility.Collapsed;
            if (PrivateMessage != null) PrivateMessage.Visibility = Visibility.Visible;
        }
    }
}
