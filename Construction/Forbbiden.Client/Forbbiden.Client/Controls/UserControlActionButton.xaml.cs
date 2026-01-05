using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Forbbiden.Client.Controls
{
    /// <summary>
    /// Interaction logic for UserControlActionButton.xaml
    /// </summary>
    public partial class UserControlActionButton : UserControl
    {
        public string ActionText { get; set; }
        public UserControlActionButton()
        {
            InitializeComponent();
        }

        private void Moves_MouseEnter(object sender, MouseEventArgs e)
        {
            ZoomIn(sender);
        }

        private void Moves_MouseLeave(object sender, MouseEventArgs e)
        {
            ZoomOut(sender);
        }

        private void ZoomIn(object sender)
        {
            Animate(sender, (130, 180), (400, 450));
        }

        private void ZoomOut(object sender)
        {
            Animate(sender, (180, 130), (450, 400));
        }

        private void Animate(object sender,
            (double from, double to) height,
            (double from, double to) width)
        {
            var grid = (Grid)sender;
            var image = grid.Children.OfType<Image>().FirstOrDefault();
            if (image != null)
            {
                var verticalZoom = new DoubleAnimation
                {
                    From = height.from,
                    To = height.to,
                    Duration = TimeSpan.FromSeconds(0.15)
                };

                var horizontalZoom = new DoubleAnimation
                {
                    From = width.from,
                    To = width.to,
                    Duration = TimeSpan.FromSeconds(0.15),
                };

                image.BeginAnimation(HeightProperty, verticalZoom);
                image.BeginAnimation(WidthProperty, horizontalZoom);
            }
        }
    }
}
