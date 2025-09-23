using System;
using System.Windows;

namespace ForbbidenIslandFEI_Construction
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
        private void englishButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.languageCode = "en-US";
            Properties.Settings.Default.Save();

            new MainWindow().ShowDialog();
            this.Close();
        }
        private void spanishButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.languageCode = "es-MX";
            Properties.Settings.Default.Save();

            new MainWindow().ShowDialog();
            this.Close();
        }
        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
