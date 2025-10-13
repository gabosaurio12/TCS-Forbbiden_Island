using System;
using System.Windows;

namespace Forbbiden.Client
{
    /// <summary>
    /// Lógica de interacción para SelectLanguageWindow.xaml
    /// </summary>
    public partial class SelectLanguageWindow : Window
    {
        public SelectLanguageWindow()
        {
            InitializeComponent();
        }
        private void EnglishButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.languageCode = "en-US";
            Properties.Settings.Default.Save();

            Close();
        }
        private void SpanishButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.languageCode = "es-MX";
            Properties.Settings.Default.Save();

            Close();
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
