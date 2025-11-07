using System.Windows;
using System.Windows.Navigation;

namespace Forbbiden.Client.view
{
    /// <summary>
    /// MainWindow.xaml interaction logic
    /// </summary>
    
    public partial class MainWindow : NavigationWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Source = new System.Uri("MainPage.xaml", System.UriKind.Relative);
        }
    }
}
