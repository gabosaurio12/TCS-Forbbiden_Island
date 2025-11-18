using Forbbiden.Client.BoardManager;
using Forbbiden.Client.FriendsManager;
using Forbbiden.Client.model;
using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.view.info;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Forbbiden.Client.view.games
{
    /// <summary>
    /// Interaction logic for Board.xaml
    /// </summary>
    public partial class BoardPage : Page
    {
        private BoardManagerClient boardManager = new BoardManagerClient();
        private List<Card> treasureCards;
        private List<Card> discardStackTreasureCards;
        private List<Card> floodCards;
        private List<Card> playerCards = new List<Card>();
        private int waterLevelCount = 0;

        public BoardPage()
        {
            InitializeComponent();
            var currentPlayer = new ProfileManagerClient().GetCurrentLogin();
            treasureCards = boardManager.GetTreasureCards().ToList();
            floodCards = boardManager.GetFloodCards().ToList();

            KeyDown += BoardPage_KeyDown;
            Focusable = true;
            Focus();
            FillFreeTiles();
            SetTreasureTiles();
            SetPlayersAvatars(currentPlayer);
        }

        private List<Grid> GetTilesFromGrid()
        {
            List<Grid> tiles = new List<Grid>();
            foreach (var child in boardTiles.Children)
            {
                if (child is Grid grid && grid.Name.StartsWith("tile"))
                {
                    tiles.Add(grid);
                }
            }
            return tiles;
        }

        private BitmapImage GetTilesImage(string tileImage)
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            string projectDir = Directory.GetParent(
                    AppDomain.CurrentDomain.BaseDirectory).
                    Parent.Parent.FullName;
            string avatarPath = System.IO.Path.Combine(projectDir, "Images", "tiles", tileImage);
            bmp.UriSource = new Uri(avatarPath, UriKind.Absolute);
            bmp.EndInit();

            return bmp;
        }

        private BitmapImage GetImage(string image)
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            string projectDir = Directory.GetParent(
                    AppDomain.CurrentDomain.BaseDirectory).
                    Parent.Parent.FullName;
            string avatarPath = System.IO.Path.Combine(projectDir, "Images", image);
            bmp.UriSource = new Uri(avatarPath, UriKind.Absolute);
            bmp.EndInit();

            return bmp;
        }

        private void FillFreeTiles()
        {
            string freeTileImage = "free.jpg";
            var bmp = GetTilesImage(freeTileImage);

            List<Grid> tiles = GetTilesFromGrid();
            foreach (var tile in tiles)
            {
                Rectangle rectangle = tile.Children.OfType<Rectangle>().FirstOrDefault();
                if (rectangle != null)
                {
                    rectangle.Fill = new ImageBrush
                    {
                        ImageSource = bmp,
                        Stretch = Stretch.UniformToFill
                    };
                }
            }
        }

        private void SetTreasureTiles()
        {
            List<Grid> tiles = GetTilesFromGrid();
            Random rand = new Random();
            for (int i = 0; i< 4; i++)
            {
                int index = rand.Next(tiles.Count);
                Grid tile = tiles[index];
                tiles.RemoveAt(index);
                string treasureImage = $"treasure{i + 1}.png";
                var bmp = GetTilesImage(treasureImage);
                Rectangle rectangle = tile.Children.OfType<Rectangle>().FirstOrDefault();
                if (rectangle != null)
                {
                    rectangle.Fill = new ImageBrush
                    {
                        ImageSource = bmp,
                        Stretch = Stretch.UniformToFill
                    };
                }
            }
        }

        private BitmapImage GetAvatarImage(string avatarPath)
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri(avatarPath, UriKind.Absolute);
            bmp.EndInit();

            return bmp;
        }

        private Ellipse GetPlayerAvatarEllipse(string avatarPath)
        {
            var bmp = GetAvatarImage(avatarPath);

            Ellipse ellipse = new Ellipse
            {
                Width = 100,
                Height = 100,
                Stroke = Brushes.LightGray,
                StrokeThickness = 5,
                Margin = new System.Windows.Thickness(0, 10, 0, 0),
                Fill = new ImageBrush
                {
                    ImageSource = bmp,
                    Stretch = Stretch.UniformToFill
                }
            };

            return ellipse;
        }

        private bool IsTreasureTile(Grid tile)
        {
            bool band = false;
            Rectangle rectangle = tile.Children.OfType<Rectangle>().FirstOrDefault();
            if (rectangle != null && rectangle.Fill is ImageBrush imageBrush)
            {
                BitmapImage bitmapImage = imageBrush.ImageSource as BitmapImage;
                if (bitmapImage != null)
                {
                    string imagePath = bitmapImage.UriSource.LocalPath;
                    string fileName = System.IO.Path.GetFileName(imagePath);
                    band = fileName.StartsWith("treasure");
                }
            }
            return band;
        }

        private void AddPlayerAvatar(ProfileManager.Player player)
        {
            string projectDir = Directory.GetParent(
                    AppDomain.CurrentDomain.BaseDirectory).
                    Parent.Parent.FullName;
            string avatarPath = System.IO.Path.Combine(projectDir, "avatars", player.PlayerAvatarPath);
            Ellipse boardAvatar = GetPlayerAvatarEllipse(avatarPath);

            var tiles = GetTilesFromGrid();
            bool avatarPlaced = false;
            do
            {
                int spawnTileIndex = new Random().Next(tiles.Count);
                var spawnTile = tiles[spawnTileIndex];
                if (!IsTreasureTile(spawnTile))
                {
                    spawnTile.Children.Add(boardAvatar);
                    avatarPlaced = true;
                }
            } while (!avatarPlaced);
        }

        private void SetPlayersAvatars(ProfileManager.Player player)
        {
            AddPlayerAvatar(player);
        }

        private void BoardPage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                NavigationService?.Navigate(new MainPage());
            }
        }

        private void ColorStroke_MouseEnter(object sender, MouseEventArgs e)
        {
            Rectangle rectangle = sender as Rectangle;

            if (rectangle != null)
            {
                rectangle.Stroke = Brushes.Gold;
            }
        }

        private void ColorStroke_MouseLeave(object sender, MouseEventArgs e)
        {
            Rectangle rectangle = sender as Rectangle;

            if (rectangle != null)
            {
                rectangle.Stroke = Brushes.DarkBlue;
            }
        }

        private void Moves_MouseEnter(object sender, MouseEventArgs e)
        {
            var grid = (Grid)sender;
            var image = grid.Children.OfType<Image>().FirstOrDefault();
            if (image == null) return;

            var verticalZoom = new DoubleAnimation
            {
                From = 130,
                To = 180,
                Duration = TimeSpan.FromSeconds(0.15)
            };

            var horizontalZoom = new DoubleAnimation
            {
                From = 400,
                To = 450,
                Duration = TimeSpan.FromSeconds(0.15),
            };

            image.BeginAnimation(HeightProperty, verticalZoom);
            image.BeginAnimation(WidthProperty, horizontalZoom);
        }

        private void Moves_MouseLeave(object sender, MouseEventArgs e)
        {
            var grid = (Grid)sender;
            var image = grid.Children.OfType<Image>().FirstOrDefault();
            if (image == null) return;

            var verticalZoom = new DoubleAnimation
            {
                From = 180,
                To = 130,
                Duration = TimeSpan.FromSeconds(0.15)
            };

            var horizontalZoom = new DoubleAnimation
            {
                From = 450,
                To = 400,
                Duration = TimeSpan.FromSeconds(0.15),
            };

            image.BeginAnimation(HeightProperty, verticalZoom);
            image.BeginAnimation(WidthProperty, horizontalZoom);
        }

        private void Card_MouseEnter(object sender, MouseEventArgs e)
        {
            var verticalZoom = new DoubleAnimation
            {
                From = 170,
                To = 190,
                Duration = TimeSpan.FromSeconds(0.15)
            };

            var horizontalZoom = new DoubleAnimation
            {
                From = 140,
                To = 160,
                Duration = TimeSpan.FromSeconds(0.15),
            };

            Rectangle card = (Rectangle)sender;
            card.BeginAnimation(HeightProperty, verticalZoom);
            card.BeginAnimation(WidthProperty, horizontalZoom);
        }

        private void Card_MouseLeave(object sender, MouseEventArgs e)
        {
            var verticalZoom = new DoubleAnimation
            {
                From = 190,
                To = 170,
                Duration = TimeSpan.FromSeconds(0.15)
            };

            var horizontalZoom = new DoubleAnimation
            {
                From = 160,
                To = 140,
                Duration = TimeSpan.FromSeconds(0.15),
            };

            Rectangle card = (Rectangle)sender;
            card.BeginAnimation(HeightProperty, verticalZoom);
            card.BeginAnimation(WidthProperty, horizontalZoom);
        }

        private void ShowCardOnBoard(Card card)
        {
            var cardSettings = new CardWindowSettings
            {
                StrokeThickness = 15,
                StrokeColor = "#A81D0F",
                CardImage = GetTilesImage(card.ImagePath)
            };
            var cardWindow = new VerticalCardWindow(cardSettings)
            {
                Owner = Window.GetWindow(this)
            };
            cardWindow.ShowDialog();
        }

        private void AddPlayerACard(Card card)
        {
            if (playerCards.Count < 6)
            {
                playerCards.Add(card);
                cardStack.Children.Add(new Rectangle
                {
                    Width = 140,
                    Height = 170,
                    Margin = new Thickness(15),
                    Fill = new ImageBrush
                    {
                        ImageSource = GetTilesImage(card.ImagePath),
                        Stretch = Stretch.UniformToFill
                    }
                });
            }
            else
            {
                string title = Properties.Langs.Resources.max_cards_exceed;
                string message = Properties.Langs.Resources.max_cards_exceed_message;
                var notificationWindow = new NotificationWindow(title, message) // Cambiarla por notificación aceptar carta
                {
                    Owner = Window.GetWindow(this)
                };
                notificationWindow.ShowDialog();
            }
        }

        private void IncreaseWaterLevel(Card card)
        {
            waterLevelCount++;
            waterLevel.Source = GetImage($"waterLevel{waterLevelCount}.png");
            floodCards.Clear();
            floodCards = boardManager.GetFloodCards().ToList();
            discardStackTreasureCards.Add(card);
        }

        private List<Card> PickTwoTreasureCards()
        {
            List<Card> cards = new List<Card>();
            Random rand = new Random();

            Card firstCard = cards[rand.Next(treasureCards.Count)];
            treasureCards.Remove(firstCard);
            Card secondCard = cards[rand.Next(treasureCards.Count)];
            treasureCards.Remove(secondCard);

            ShowCardOnBoard(firstCard);
            ShowCardOnBoard(secondCard);

            if (firstCard.Name != "Water Rises!") AddPlayerACard(firstCard);
            else IncreaseWaterLevel(firstCard);
            if (secondCard.Name != "Water Rises!") AddPlayerACard(secondCard);
            else IncreaseWaterLevel(secondCard);

            return cards;
        }

        private void PickTreasureCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PickTwoTreasureCards();

            /*
             * TODO
             * 
             * Card card = GetFloodCard();
             * ShowCardOnBoard(card);
             * var flooded = BoardManager.FloodCard(card);
             * 
             * if (flooded == null) 
             * {
             *     card.brush = Brushes.Black;
             *     Grid tile = board.GetTileByCoord(string coord);
             *     foreach (var child in tile.Children)
             *     {
             *         if (child is Ellipse ellipse)
             *         {
             *             MoveAvatarToClosesTile(ellipse);
             *         }
             *     }
             * }
             *             
             * else card.brush = Brushes.Gray;
             *     
             */
        }
    }
}
