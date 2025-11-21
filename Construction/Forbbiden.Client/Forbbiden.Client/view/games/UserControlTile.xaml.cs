using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Forbbiden.Client.view.games
{
    /// <summary>
    /// Interaction logic for UserControlTile.xaml
    /// </summary>
    public partial class UserControlTile : UserControl
    {
        public int Col { get; set; }
        public int Row { get; set; }
        public bool IsTreasure { get; set; }

        public UserControlTile()
        {
            InitializeComponent();
        }

        public void SetImage(BitmapImage image)
        {
            tileImage.Source = image;
            tileImage.Stretch = Stretch.UniformToFill;
        }

        public BitmapImage GetImage()
        {
            return (BitmapImage)tileImage.Source;
        }

        public void SetBorderBrush(Color color)
        {
            tile.BorderBrush = new SolidColorBrush(color);
        }

        public void AddAvatar(Ellipse avatar)
        {
            avatar.VerticalAlignment = VerticalAlignment.Center;
            avatar.HorizontalAlignment = HorizontalAlignment.Center;
            tileGrid.Children.Add(avatar);
        }
    }
}
