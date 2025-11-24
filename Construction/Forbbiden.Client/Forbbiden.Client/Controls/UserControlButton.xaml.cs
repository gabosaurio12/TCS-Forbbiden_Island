using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Forbbiden.Client.Controls
{
    /// <summary>
    /// Interaction logic for UserControlButton.xaml
    /// </summary>
    public partial class UserControlButton : UserControl
    {
        public UserControlButton()
        {
            InitializeComponent();
        }

        public void SetBackgroundColor(Color color)
        {
            Background = new SolidColorBrush(color);
        }

        public void SetForeground(Color color)
        {
            Foreground = new SolidColorBrush(color);
        }

        public void SetText(string text)
        {
            Content = text;
        }
    }
}
