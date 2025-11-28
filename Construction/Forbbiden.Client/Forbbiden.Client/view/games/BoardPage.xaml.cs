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
        private UserControlTile CurrentTile;
        private int ActionsRemain = 3;

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
            Board.TileClickedOnBoard += OnTileClickedFromBoard;
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

        private void SetBoard()
        {
            Board = new UserControlBoard();
            Grid.SetColumn(Board, 1);
            Grid.SetRow(Board, 0);

            mainGrid.Children.Add(Board);
        }

        private void SetPlayersAvatars()
        {
            CurrentTile = Board.AddPlayerAvatar(ClientSession.GetPlayer());
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
                ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
            }
        }

        private void IncreaseWaterLevel(Card card)
        {
            WaterLevelCount++;

            if (WaterLevelCount == 6)
            {
                string waterLevelImagePath = System.IO.Path.Combine(
                    ImagesPath, $"waterLevel-{WaterLevelCount}.png");
                waterLevel.Source = ViewUtils.GetBitmapImage(waterLevelImagePath);

                string title = Properties.Langs.Resources.game_over;
                string message = Properties.Langs.Resources.game_over_message;
                ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));

                NavigationService?.Navigate(new MainPage());
            }
            else
            {
                string waterLevelImagePath = System.IO.Path.Combine(
                    ImagesPath, $"waterLevel-{WaterLevelCount}.png");
                waterLevel.Source = ViewUtils.GetBitmapImage(waterLevelImagePath);

                var floodedTiles = Board.GetFloodedTiles();

                foreach (var tile in floodedTiles)
                {
                    tile.LoseTile();
                }

                FloodCards.Clear();
                FloodCards = BoardManager.GetFloodCards().ToList();
                DiscardStackTreasureCards.Add(card);
            }
        }

        private void FloodTile(Card card)
        {
            var cardImageFileName = card.ImagePath;
            
            foreach (var tile in Board.GetAllTilesFromGrid())
            {
                if (tile.ImageFileName == cardImageFileName)
                {
                    tile.FloodTile();
                    break;
                }
            }
        }

        private void ExecuteCardEffect(Card card)
        {
            string waterRiseCard = "water_rise_name";

            if (card.Name == waterRiseCard)
                IncreaseWaterLevel(card);
            else
            {
                AddPlayerACard(card);
            }
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
                var cardWindow = new HorizontalCardWindow(cardSettings)
                {
                    Owner = Window.GetWindow(this)
                };
                cardWindow.ShowDialog();
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





        private void Move_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ActionsRemain > 0)
            {
                var possibleTilesToMove = MatchLogic.GetPossibleTilesToMove(CurrentTile, Board);

                foreach (var possibleTile in possibleTilesToMove)
                {
                    possibleTile.SetMovementBorders();
                }
            }
            else
            {
                string title = Properties.Langs.Resources.actions_left;
                string message = Properties.Langs.Resources.no_more_actions;
                ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
            }
        }

        private void OnTileClickedFromBoard(object sender, TileClickedEventArgs e)
        {
            var moveToTile = Board.GetTile(e.Row, e.Column);
            Ellipse avatar = ViewUtils.GetAvatarEllipse(ClientSession.AvatarPath);
            
            CurrentTile.ClearAvatar();
            moveToTile.AddAvatar(avatar);

            var tiles = MatchLogic.GetPossibleTilesToMove(CurrentTile, Board);

            CurrentTile = moveToTile;

            ActionsRemain--;
            var actionImage = actionsRemainingStack.Children[ActionsRemain];
            actionImage.Visibility = Visibility.Hidden;
            MatchLogic.ResetTiles(tiles);
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

        private void EndTurnButton_Click(object sender, RoutedEventArgs e)
        {
            ActionsRemain = 3;
            foreach (Image action in actionsRemainingStack.Children)
            {
                action.Visibility = Visibility.Visible;
            }

            Random rand = new Random();
            int minRand = 0;
            int maxRand = FloodCards.Count - 1;
            for (int i = 0; i < 2; i++)
            {
                int randomNumber = rand.Next(minRand, maxRand);

                var floodCard = FloodCards[randomNumber];
                FloodTile(floodCard);
                FloodCards.Remove(floodCard);
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
