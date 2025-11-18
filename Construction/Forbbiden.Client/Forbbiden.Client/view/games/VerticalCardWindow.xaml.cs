using Forbbiden.Client.model;
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

namespace Forbbiden.Client.view.games
{
    /// <summary>
    /// Interaction logic for VerticalCardWindow.xaml
    /// </summary>
    public partial class VerticalCardWindow : Window
    {
        public VerticalCardWindow()
        {
            InitializeComponent();
            KeyDown += BoardPage_KeyDown;
        }

        public VerticalCardWindow(CardWindowSettings settings)
        {
            InitializeComponent();
            cardGrid.Background = new ImageBrush(settings.CardImage);
            cardRectangle.StrokeThickness = settings.StrokeThickness;
            cardRectangle.Stroke = (SolidColorBrush)(new BrushConverter().ConvertFromString(settings.StrokeColor));
            KeyDown += BoardPage_KeyDown;
        }

        private void BoardPage_KeyDown(object sender, KeyEventArgs e)
        {
            Key[] keys = { Key.Escape, Key.Enter, Key.Space, Key.Return };
            if (keys.Contains(e.Key))
            {
                Close();
            }
        }
    }
}
