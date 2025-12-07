using System.Globalization;
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

            var currentCulture = CultureInfo.CurrentUICulture;

            Properties.Settings.Default.languageCode = currentCulture.Name;
            Properties.Settings.Default.Save();

            Source = new System.Uri("MainPage.xaml", System.UriKind.Relative);
        }
    }
}
