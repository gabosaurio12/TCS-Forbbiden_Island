using System;
using System.Windows;
using System.Windows.Controls;

namespace Forbbiden.Client
{
    /// <summary>
    /// SelectLanguageWindow.xaml interaction logic
    /// </summary>
    public partial class SelectLanguagePage : Page
    {
        public SelectLanguagePage()
        {
            InitializeComponent();
        }
        private void EnglishButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.languageCode = "en-US";
            Properties.Settings.Default.Save();

            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
        private void SpanishButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.languageCode = "es-MX";
            Properties.Settings.Default.Save();

            if (this.NavigationService != null && this.NavigationService.CanGoBack)
            {
                this.NavigationService.GoBack();
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService != null && this.NavigationService.CanGoBack)
            {
                this.NavigationService.GoBack();
            }
        }
    }
}
