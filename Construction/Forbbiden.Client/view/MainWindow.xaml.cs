using System.Windows;
using System.Windows.Navigation;

namespace Forbbiden.Client.view
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : NavigationWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Source = new System.Uri("MainPage.xaml", System.UriKind.Relative);
        }
    }
}
