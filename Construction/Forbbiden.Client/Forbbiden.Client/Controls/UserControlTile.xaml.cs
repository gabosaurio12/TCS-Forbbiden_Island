using Forbbiden.Client.BoardManager;
using Forbbiden.Client.logic;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Forbbiden.Client.Controls
{
    /// <summary>
    /// Interaction logic for UserControlTile.xaml
    /// </summary>
    public partial class UserControlTile : UserControl
    {
        public Card TreasureCard { get; set; }
        public int Col { get; set; }
        public int Row { get; set; }
        public bool IsTreasure { get; set; }
        public bool IsFlood { get; set; }
        public bool IsLost { get; set; }
        public bool IsEscapeTile { get; set; }

        public Color DefaultWhite { get; }
        public Color EscapeBlue { get; }
        public Color FloodGray { get; }
        public Color Border { get; set; }
        public Color EnterBorder { get; set; }
        public Color RedColor { get; set; }

        public string ImageFileName { get; set; }

        public UserControlTile()
        {
            InitializeComponent();

            string whiteHex = "#EDEDED";
            DefaultWhite = (Color)ColorConverter.ConvertFromString(whiteHex);
            string escapeBlueHex = "#102E78";
            EscapeBlue = (Color)ColorConverter.ConvertFromString(escapeBlueHex);
            string floodGrayHex = "#454E5F";
            FloodGray = (Color)ColorConverter.ConvertFromString(floodGrayHex);
            string redColorHex = "#A81D0F";
            RedColor = (Color)ColorConverter.ConvertFromString(redColorHex);

            Border = DefaultWhite;
            EnterBorder = DefaultWhite;

            IsHitTestVisible = false;
            Cursor = Cursors.Arrow;
        }

        public void SetTile(UserControlTile tile)
        {
            TreasureCard = tile.TreasureCard;
            IsTreasure = tile.IsTreasure;
            IsFlood = tile.IsFlood;
            IsLost = tile.IsLost;
            IsEscapeTile = tile.IsEscapeTile;
            Border = tile.Border;
            EnterBorder = tile.EnterBorder;
            ImageFileName = tile.ImageFileName;
            BorderBrush = tile.BorderBrush;

            var image = ViewUtils.GetBitmapImage(ImageFileName);
            SetImage(image);
            ResetBorder();
        }

        public void SetTileAsTreasure(BitmapImage treasureBitmap, Card treasureCard)
        {
            TreasureCard = treasureCard;
            Border = EnterBorder = RedColor;
            SetImage(treasureBitmap);
            RefreshBorderBrush();
            IsTreasure = true;
        }

        public void SetTileAsEscape()
        {
            Border = EnterBorder = EscapeBlue;
            RefreshBorderBrush();
            IsEscapeTile = true;
        }

        public void SetInteractionBorders()
        {
            string yellow = "#FFEB00";
            string orange = "#DE7A14";
            Border = (Color)ColorConverter.ConvertFromString(yellow);
            EnterBorder = (Color)ColorConverter.ConvertFromString(orange);
            RefreshBorderBrush();
            ActivateMovement();
        }

        public void RefreshBorderBrush()
        {
            tile.BorderBrush = new SolidColorBrush(Border);
        }

        public void ResetBorder()
        {
            Color color;
            if (IsEscapeTile)
            {
                color = EscapeBlue;
            } 
            else if (IsTreasure)
            {
                color = RedColor;
            }
            else if (IsFlood)
            {
                color = FloodGray;
            }
            else
            {
                color = DefaultWhite;
            }

            Border = EnterBorder = color;
            RefreshBorderBrush();
        }

        public void FloodTile()
        {
            IsFlood = true;
            Border = EnterBorder = FloodGray;
            RefreshBorderBrush();
        }

        public void LoseTile()
        {
            IsLost = true;
            DeactivateMovement();
            Visibility = Visibility.Hidden;
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
        
        public void ActivateMovement()
        {
            Cursor = Cursors.Hand;
            IsHitTestVisible = true;
        }

        public void DeactivateMovement()
        {
            Cursor = Cursors.Arrow;
            IsHitTestVisible = false;
        }

        public void AddAvatar(Ellipse avatar)
        {
            avatar.VerticalAlignment = VerticalAlignment.Center;
            avatar.HorizontalAlignment = HorizontalAlignment.Center;
            tileGrid.Children.Add(avatar);
        }

        public void ClearAvatar()
        {
            var avatar = tileGrid.Children[1];
            tileGrid.Children.Remove(avatar);
        }

        private void ColorStroke_MouseEnter(object sender, MouseEventArgs e)
        {
            tile.BorderBrush = new SolidColorBrush(EnterBorder);
        }

        private void ColorStroke_MouseLeave(object sender, MouseEventArgs e)
        {
            tile.BorderBrush = new SolidColorBrush(Border);
        }

        public event EventHandler<TileClickedEventArgs> TileClicked;

        private void MoveAvatar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TileClicked?.Invoke(this, new TileClickedEventArgs(Row, Col));
        }
    }
}
