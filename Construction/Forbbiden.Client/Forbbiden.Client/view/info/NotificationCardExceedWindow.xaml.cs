using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Forbbiden.Client.view.info
{
    /// <summary>
    /// Interaction logic for NotificationCardExceedWindow.xaml
    /// </summary>
    public partial class NotificationCardExceedWindow : Window
    {
        private bool DiscardMode;

        public NotificationCardExceedWindow()
        {
            InitializeComponent();
        }

        public NotificationCardExceedWindow(ref bool discardMode)
        {
            InitializeComponent();
            DiscardMode = discardMode;
        }

        private void DiscardButton_Click(object sender, RoutedEventArgs e)
        {
            DiscardMode = true;
            Close();
        }

        private void KeepButton_Click(object sender, RoutedEventArgs e)
        {
            DiscardMode = false;
            Close();
        }
    }
}
