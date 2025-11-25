using Forbbiden.Client.BoardManager;
using Forbbiden.Client.logic;
using Forbbiden.Client.model;
using Forbbiden.Client.view.info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using WpfAnimatedGif;

namespace Forbbiden.Client.view.games
{
    /// <summary>
    /// Interaction logic for Board.xaml
    /// </summary>
    public partial class BoardPage : Page
    {
        private readonly BoardManagerClient BoardManager = new BoardManagerClient();
        private List<Card> TreasureCards;
        private readonly List<Card> DiscardStackTreasureCards;
        private List<Card> FloodCards;
        private readonly List<Card> PlayerCards = new List<Card>();
        private int WaterLevelCount = 0;
        private readonly string ImagesPath;
        private readonly string CardsImagesPath;
        private UserControlBoard Board;

        public BoardPage()
        {
            InitializeComponent();

            TreasureCards = BoardManager.GetTreasureCards().ToList();
            FloodCards = BoardManager.GetFloodCards().ToList();
            DiscardStackTreasureCards = new List<Card>();

            string projectDir = ViewUtils.GetProjectDir();

            ImagesPath = System.IO.Path.Combine(
                projectDir, "Images");
            CardsImagesPath = System.IO.Path.Combine(
                projectDir, ImagesPath, "cards");

            InitGif();
            KeyDown += BoardPage_KeyDown;
            Focusable = true;
            Focus();

            SetBoard();
            SetPlayersAvatars();
        }

        private void InitGif()
        {
            string gifName = "board_background.gif";
            string gifPath = System.IO.Path.Combine(
                ImagesPath, gifName);
            var gif = ViewUtils.GetBitmapImage(gifPath);

            ImageBehavior.SetAnimatedSource(gifBackground, gif);
        }

        private void BoardPage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                NavigationService?.Navigate(new MainPage());
            }
        }

        private void PickTreasureCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PickTreasureCard();
            PickTreasureCard();
        }

        private void SetBoard()
        {
            Board = new UserControlBoard();
            Grid.SetColumn(Board, 1);
            Grid.SetRow(Board, 0);

            mainGrid.Children.Add(Board);
        }

        private void SetPlayersAvatars()
        {
            Board.AddPlayerAvatar(ClientSession.GetPlayer());
        }

        private void ShowCardOnBoard(Card card)
        {
            string cardImagePath = System.IO.Path.Combine(
                CardsImagesPath, card.ImagePath);

            var cardSettings = new CardWindowSettings
            {
                StrokeThickness = 15,
                StrokeColor = "#A81D0F",
                CardImage = ViewUtils.GetBitmapImage(cardImagePath)
            };
            if (card.Name == "water_rise_name")
            {
                var cardWindow = new HorizontalCardWindow(cardSettings)
                {
                    Owner = Window.GetWindow(this)
                };
                cardWindow.ShowDialog();
            }
            else
            {
                var cardWindow = new VerticalCardWindow(cardSettings)
                {
                    Owner = Window.GetWindow(this)
                };
                cardWindow.ShowDialog();
            } 
        }

        private void AddPlayerACard(Card card)
        {
            if (PlayerCards.Count < 6)
            {
                PlayerCards.Add(card);
                cardStack.Children.Add(new Rectangle
                {
                    Width = 140,
                    Height = 170,
                    Margin = new Thickness(15),
                    Fill = new ImageBrush
                    {
                        ImageSource = ViewUtils.GetBitmapImage(card.ImagePath),
                        Stretch = Stretch.UniformToFill
                    }
                });
            }
            else
            {
                string title = Properties.Langs.Resources.max_cards_exceed;
                string message = Properties.Langs.Resources.max_cards_exceed_message;
                var notificationWindow = new NotificationWindow(title, message) // TODO Cambiarla por notificación aceptar carta
                {
                    Owner = Window.GetWindow(this)
                };
                notificationWindow.ShowDialog();
            }
        }

        private void IncreaseWaterLevel(Card card)
        {
            WaterLevelCount++;
            string waterLevelImagePath = System.IO.Path.Combine(
                ImagesPath, $"waterLevel-{WaterLevelCount}.png");
            waterLevel.Source = ViewUtils.GetBitmapImage(waterLevelImagePath);
            FloodCards.Clear();
            FloodCards = BoardManager.GetFloodCards().ToList();
            DiscardStackTreasureCards.Add(card);
        }

        private void FloodTile(Card card)
        {
            // TODO
        }

        private void ExecuteCardEffect(Card card)
        {
            string waterRiseCard = "water_rise_name";
            string floodCard = "flood_name";

            if (card.Name == waterRiseCard)
                IncreaseWaterLevel(card);
            else if (card.Name == floodCard)
            {
                FloodTile(card);
            }
            else
            {
                AddPlayerACard(card);
            }
        }

        private Card PickTreasureCard()
        {
            int count = TreasureCards.Count;
            int emptyNumber = 0;

            if (count == emptyNumber)
            {
                TreasureCards = BoardManager.GetTreasureCards().ToList();
            }

            Random rand = new Random();
            count = TreasureCards.Count;
            int random = rand.Next(count);
            Card card = TreasureCards[random];
            TreasureCards.Remove(card);

            ShowCardOnBoard(card);

            ExecuteCardEffect(card);

            return card;
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

        private void CardStack_MouseEnter(object sender, MouseEventArgs e)
        {
            var verticalZoom = new DoubleAnimation
            {
                From = 200,
                To = 220,
                Duration = TimeSpan.FromSeconds(0.15)
            };

            var horizontalZoom = new DoubleAnimation
            {
                From = 170,
                To = 190,
                Duration = TimeSpan.FromSeconds(0.15),
            };

            Rectangle card = (Rectangle)sender;
            card.BeginAnimation(HeightProperty, verticalZoom);
            card.BeginAnimation(WidthProperty, horizontalZoom);
        }

        private void CardStack_MouseLeave(object sender, MouseEventArgs e)
        {
            var verticalZoom = new DoubleAnimation
            {
                From = 220,
                To = 200,
                Duration = TimeSpan.FromSeconds(0.15)
            };

            var horizontalZoom = new DoubleAnimation
            {
                From = 190,
                To = 170,
                Duration = TimeSpan.FromSeconds(0.15),
            };

            Rectangle card = (Rectangle)sender;
            card.BeginAnimation(HeightProperty, verticalZoom);
            card.BeginAnimation(WidthProperty, horizontalZoom);
        }
    }
}
