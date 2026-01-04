using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Forbbiden.Client.Controls
{
    /// <summary>
    /// Interaction logic for UserControlFriend.xaml
    /// </summary>
    public partial class UserControlFriend : UserControl
    {
        private readonly int imageHeight = 70;
        private readonly int imageWidth = 60;
        public UserControlFriend()
        {
            InitializeComponent();
        }

        public void SetAvatarImage(Ellipse ellipse, ImageBrush avatarImage)
        {
            ellipse.Fill = avatarImage;
        }

        public void SetFriendUsername(TextBlock textBlock, string username)
        {
            textBlock.Text = username;
        }

        private void Image_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var image = sender as Image;
            var verticalZoom = new DoubleAnimation
            {
                From = imageHeight,
                To = image.Height + 5,
                Duration = TimeSpan.FromSeconds(0.15)
            };

            var horizontalZoom = new DoubleAnimation
            {
                From = imageWidth,
                To = image.Width + 5,
                Duration = TimeSpan.FromSeconds(0.15),
            };

            image.BeginAnimation(HeightProperty, verticalZoom);
            image.BeginAnimation(WidthProperty, horizontalZoom);
        }

        private void Image_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var image = sender as Image;
            var verticalZoom = new DoubleAnimation
            {
                From = image.Height,
                To = imageHeight,
                Duration = TimeSpan.FromSeconds(0.15)
            };

            var horizontalZoom = new DoubleAnimation
            {
                From = image.Width,
                To = imageWidth,
                Duration = TimeSpan.FromSeconds(0.15),
            };

            image.BeginAnimation(HeightProperty, verticalZoom);
            image.BeginAnimation(WidthProperty, horizontalZoom);
        }
    }
}
