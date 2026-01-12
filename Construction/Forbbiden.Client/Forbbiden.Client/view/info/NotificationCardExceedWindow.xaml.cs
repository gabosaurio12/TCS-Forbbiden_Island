using System;
using System.Windows;

namespace Forbbiden.Client.View.info
{
    /// <summary>
    /// Interaction logic for NotificationCardExceedWindow.xaml
    /// </summary>
    public partial class NotificationCardExceedWindow : Window
    {
        public event Action OnDiscard;
        public event Action OnKeep;

        public NotificationCardExceedWindow()
        {
            InitializeComponent();
        }

        private void DiscardButton_Click(object sender, RoutedEventArgs e)
        {
            OnDiscard?.Invoke();
            Close();
        }

        private void KeepButton_Click(object sender, RoutedEventArgs e)
        {
            OnKeep?.Invoke();
            Close();
        }
    }
}
