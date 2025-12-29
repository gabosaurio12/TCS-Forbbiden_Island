using Forbbiden.Client.BoardManager;
using Forbbiden.Client.logic;
using System;
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
        public int Col { get; set; }
        public int Row { get; set; }
        public bool IsTreasure { get; set; }
        public bool IsFlood { get; set; }
        public bool IsLost { get; set; }
        public bool IsEscapeTile { get; set; }
        public Card TreasureCard;

        public Color DefaultGreen { get; }
        public Color EscapeBlue { get; }
        public Color Border { get; set; }
        public Color EnterBorder { get; set; }
        public Color RedColor { get; set; }

        public string ImageFileName;

        public UserControlTile()
        {
            InitializeComponent();

            string defaultGreenHex = "#03A300";
            DefaultGreen = (Color)ColorConverter.ConvertFromString(defaultGreenHex);
            string defaultBlueHex = "#102E78";
            EscapeBlue = (Color)ColorConverter.ConvertFromString(defaultBlueHex);
            string redColorHex = "#A81D0F";
            RedColor = (Color)ColorConverter.ConvertFromString(redColorHex);
            Border = DefaultGreen;
            EnterBorder = DefaultGreen;

            IsHitTestVisible = false;
            Cursor = Cursors.Arrow;
        }

        public void SetTileAsTreasure(BitmapImage treasureBitmap, Card treasureCard)
        {
            TreasureCard = treasureCard;
            Border = EnterBorder = RedColor;
            SetImage(treasureBitmap);
            UpdateBorderBrush();
            IsTreasure = true;
        }

        public void SetTileAsEscape(BitmapImage escapeBitmap)
        {
            Border = EnterBorder = EscapeBlue;
            SetImage(escapeBitmap);
            UpdateBorderBrush();
            IsEscapeTile = true;
        }

        public void SetInteractionBorders()
        {
            string yellow = "#FFEB00";
            string orange = "#DE7A14";
            Border = (Color)ColorConverter.ConvertFromString(yellow);
            EnterBorder = (Color)ColorConverter.ConvertFromString(orange);
            UpdateBorderBrush();
            ActivateMovement();
        }

        public void UpdateBorderBrush()
        {
            tile.BorderBrush = new SolidColorBrush(Border);
        }

        public void ResetBorderToDefault()
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
            else
            {
                color = DefaultGreen;
            }

            Border = EnterBorder = color;
            tile.BorderBrush = new SolidColorBrush(color);
        }

        public void ResetBorder()
        {
            if (IsFlood)
            {
                FloodTile();
            }
            else
            {
                ResetBorderToDefault();
            }
        }

        public void FloodTile()
        {
            IsFlood = true;
            string gray = "#5B677D";
            Border = (Color)ColorConverter.ConvertFromString(gray);
            EnterBorder = (Color)ColorConverter.ConvertFromString(gray);
            UpdateBorderBrush();
        }

        public void LoseTile()
        {
            IsLost = true;
            string black = "#000000";
            Border = (Color)ColorConverter.ConvertFromString(black);
            EnterBorder = (Color)ColorConverter.ConvertFromString(black);
            DesactivateMovement();
            UpdateBorderBrush();
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

        public void DesactivateMovement()
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
