using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Forbbiden.Client.view.games
{
    /// <summary>
    /// Interaction logic for UserControlCard.xaml
    /// </summary>
    public partial class UserControlCard : UserControl
    {

        private readonly int BasicHeight = 120;
        private readonly int BasicWidth = 150;

        public UserControlCard()
        {
            InitializeComponent();
        }

        public void SetImage(BitmapImage image)
        {
            cardImage.Source = image;
            cardImage.Stretch = Stretch.Fill;
        }

        private void Card_MouseEnter(object sender, MouseEventArgs e)
        {
            var verticalZoom = new DoubleAnimation
            {
                From = (int)Height,
                To = (int)Height + 20,
                Duration = TimeSpan.FromSeconds(0.15)
            };

            var horizontalZoom = new DoubleAnimation
            {
                From = (int)Width,
                To = (int)Width + 20,
                Duration = TimeSpan.FromSeconds(0.15),
            };

            BeginAnimation(HeightProperty, verticalZoom);
            BeginAnimation(WidthProperty, horizontalZoom);
        }

        private void Card_MouseLeave(object sender, MouseEventArgs e)
        {
            var verticalZoom = new DoubleAnimation
            {
                From = (int)Height,
                To = BasicHeight,
                Duration = TimeSpan.FromSeconds(0.15)
            };

            var horizontalZoom = new DoubleAnimation
            {
                From = (int)Width,
                To = BasicWidth,
                Duration = TimeSpan.FromSeconds(0.15),
            };

            BeginAnimation(HeightProperty, verticalZoom);
            BeginAnimation(WidthProperty, horizontalZoom);
        }
    }
}
