using Forbbiden.Client.BoardManager;
using Forbbiden.Client.Controls;
using Forbbiden.Client.logic;
using Forbbiden.Client.logic.board;
using Forbbiden.Client.logic.board.states;
using Forbbiden.Client.model;
using Forbbiden.Client.view.info;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfAnimatedGif;

namespace Forbbiden.Client.view.games
{
    /// <summary>
    /// Interaction logic for Board.xaml
    /// </summary>
    public partial class BoardPage : Page
    {
        // def fields //
        private static readonly ILog Log = LogManager.GetLogger(typeof(BoardPage));

        private BoardStateContext StateContext;
        private int PendingTreasureDraws;

        public UserControlTile CurrentTile;
        public UserControlBoard BoardControl;
        public int ActionsRemain = 3;
        public int TreasuresCaptured = 0;

        private List<Card> PlayerCards;
        private List<Card> TreasureStack;
        private List<Card> TreasureDiscardStack;
        private List<Card> FloodCards;
        private List<Card> FloodDiscardStack;

        public int CleanCodeCounter = 0;
        public int CubicleKeysCounter = 0;
        public int LucioCounter = 0;
        public int ParkingCardCounter = 0;

        // fut //

        private readonly BoardManagerClient BoardManager = new BoardManagerClient();

        public int WaterLevelCount = 0;

        private readonly string ImagesPath;
        private readonly string CardsImagesPath;


        public BoardPage()
        {
            InitializeComponent();

            StateContext = new BoardStateContext(this);

            PlayerCards = new List<Card>();
            TreasureStack = BoardManager.GetTreasureCards().ToList();
            FloodCards = BoardManager.GetFloodCards().ToList();
            TreasureDiscardStack = new List<Card>();
            FloodDiscardStack = new List<Card>();

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
            BoardControl.TileClickedOnBoard += OnTileClickedFromBoard;
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
            BoardControl = new UserControlBoard();
            Grid.SetColumn(BoardControl, 1);
            Grid.SetRow(BoardControl, 0);

            mainGrid.Children.Add(BoardControl);
        }

        private void SetPlayersAvatars()
        {
            CurrentTile = BoardControl.AddPlayerAvatar(ClientSession.GetPlayer());
        }

        // order
        // notifications
        // ui updates
        // move
        // shore
        // use treasure card
        // capture treasure
        // handlers

        public void NotifyNoActionsRemain()
        {
            string title = Properties.Langs.Resources.actions_left;
            string message = Properties.Langs.Resources.no_more_actions;
            ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
        }

        public void ShowInteractiveTiles(List<UserControlTile> tiles)
        {
            foreach (var tile in tiles)
            {
                tile.SetInteractionBorders();
            }
        }

        public void RefreshFloodTile(Card card)
        {
            var cardImageFileName = card.ImagePath;

            foreach (var tile in BoardControl.GetAllTilesFromGrid())
            {
                if (tile.ImageFileName == cardImageFileName)
                {
                    if (tile.IsFlood)
                    {
                        tile.LoseTile();
                    }
                    else
                    {
                        tile.FloodTile();
                    }
                    break;
                }
            }
        }

        public void ResetTiles()
        {
            var tiles = BoardControl.GetAllTilesFromGrid();
            MatchLogic.ResetTiles(tiles);
        }

        public void RefreshAvatarTile(TileClickedEventArgs tile)
        {
            ResetTiles();

            var moveToTile = BoardControl.GetTile(tile.Row, tile.Column);
            Ellipse avatar = ViewUtils.GetAvatarEllipse(ClientSession.AvatarPath);
            CurrentTile.ClearAvatar(); 
            moveToTile.AddAvatar(avatar);
            CurrentTile = moveToTile;
        }

        public void EndAction()
        {
            if (ActionsRemain > 0)
            {
                ActionsRemain--;
                var actionImage = actionsRemainingStack.Children[ActionsRemain];
                actionImage.Visibility = Visibility.Hidden;
            }
        }

        private void Move_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StateContext.CurrentState.OnMoveClicked();
        }

        private void Shore_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StateContext.CurrentState.OnShoreClicked();
        }

        private void UseTreasureCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StateContext.CurrentState.OnUseTreasureCardClicked();
        }

        private void CaptureTreasure_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (CurrentTile.IsTreasure)
            {
                StateContext.OnCaptureTreasureClicked();
            }
            else
            {
                string title = Properties.Langs.Resources.not_treasure_tile_title;
                string message = Properties.Langs.Resources.not_treasure_tile;
                ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
            }
        }

        private void EndTurnButton_Click(object sender, RoutedEventArgs e)
        {
            EndTurn();
        }

        private void OnTileClickedFromBoard(object sender, TileClickedEventArgs e)
        {
            StateContext.CurrentState.OnTileClicked(e);
        }

        private void OnCardClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is UserControlCard cardControl)
            {
                StateContext.CurrentState.OnCardClicked(cardControl.CardInfo);
            }
        }

        private void SubstractFromTreasureCardCounter(string treasureName)
        {
            switch (treasureName)
            {
                case "clean-code-name":
                    CleanCodeCounter--;
                    break;
                case "cubicle-keys-name":
                    CubicleKeysCounter--;
                    break;
                case "lucio-name":
                    LucioCounter--;
                    break;
                case "parking-card-name":
                    ParkingCardCounter--;
                    break;
            }
        }

        public void DiscardCardFromHand(Card card)
        {
            PlayerCards.Remove(card);
            SubstractFromTreasureCardCounter(card.Name);
            var cardControl = cardStack.Children
                .OfType<UserControlCard>()
                .FirstOrDefault(c => c.CardInfo.Name == card.Name);

            if (cardControl != null)
            {
                cardStack.Children.Remove(cardControl);
            }

            TreasureDiscardStack.Add(card);
        }

        private void DiscardCardByName(string cardName)
        {
            var card = PlayerCards.FirstOrDefault(c => c.Name == cardName);
            if (card != null)
            {
                DiscardCardFromHand(card);
            }
        }

        public void NotifyWin()
        {
            string title = Properties.Langs.Resources.win_title;
            string message = Properties.Langs.Resources.win_message;
            ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
            NavigationService?.Navigate(new MainPage());
        }

        public void NotifyLoose()
        {
            string title = Properties.Langs.Resources.game_over;
            string message = Properties.Langs.Resources.game_over_message;
            ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));

            NavigationService?.Navigate(new MainPage());
        }

        public void IncreaseWaterLevel(Card card)
        {
            WaterLevelCount++;

            string waterLevelImagePath = System.IO.Path.Combine(
                    ImagesPath, $"waterLevel-{WaterLevelCount}.png");
            waterLevel.Source = ViewUtils.GetBitmapImage(waterLevelImagePath);

            if (WaterLevelCount < 6)
            {
                var remainingFloodCards = new List<Card>(FloodCards);
                FloodCards.Clear();
                var shuffledDiscardStack = MatchLogic.ShuffleCards(FloodDiscardStack);
                FloodCards.AddRange(shuffledDiscardStack);
                FloodCards.AddRange(remainingFloodCards);
                FloodDiscardStack.Clear();

                TreasureDiscardStack.Add(card);
            }
            else
            {
                NotifyLoose();
            }
        }

        private void RefreshTreasureImage(Card treasure)
        {
            string fileName = treasure.ImagePath.Split('.')[0];
            string newFileName = String.Concat(fileName, "C", ".png");
            var projectDir = ViewUtils.GetProjectDir();
            string newFilePath = System.IO.Path.Combine(projectDir, "images", "cards", newFileName);
            BitmapImage treasureImageColor = ViewUtils.GetBitmapImage(newFilePath);

            var onBoardImage = new Dictionary<string, Image>
            {
                { "clean-code-name", cleanCodeImage },
                { "cubicle-keys-name", cubicleKeysImage },
                { "lucio-name", lucioImage },
                { "parking-card-name", parkingCardImage }
            };

            if (onBoardImage.TryGetValue(treasure.Name, out var image))
            {
                image.Source = treasureImageColor;
                var stack = (StackPanel)image.Parent;

                string treasureCapturedBlueHex = "#33689E";
                var treasureCapturedBlue = (Brush)new BrushConverter()
                    .ConvertFromString(treasureCapturedBlueHex);
                stack.Background = treasureCapturedBlue;
            }
            else
            {
                string classMethod = "BoardPage.CaptureTreasure";
                string error = "No mapped image for " + treasure.Name;
                var ex = new Exception(error);
                Log.Error(classMethod, ex);
            }
        }

        private bool CanCaptureTreasure(Card treasure)
        {
            return PlayerCards.Count(c => c.Name == treasure.Name) >= 2;
        }

        public void CaptureTreasure(Card treasure)
        {
            if (CanCaptureTreasure(treasure))
            {
                RefreshTreasureImage(treasure);

                DiscardCardByName(treasure.Name);
                DiscardCardByName(treasure.Name);

                TreasuresCaptured++;
                EndAction();
            }
            else
            {
                Log.Error("BoardPage.CaptureTreasure", new InvalidOperationException(
                    "Unexpected difference between treasure card counter and player's card hand"));
                string title = Properties.Langs.Resources.not_enough_treasure_cards_title;
                string message = Properties.Langs.Resources.not_enough_treasure_cards;
                ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
            }
        }

        public void EndTurn()
        {
            ActionsRemain = 3;
            foreach (Image action in actionsRemainingStack.Children)
            {
                action.Visibility = Visibility.Visible;
            }

            PickTreasureCard();
        }

        // possible stay methods //


        private void ShowFloodTile(Card floodCard)
        {
            var gray = "#5B677D";
            var tilesPath = System.IO.Path.Combine(
                ImagesPath, "tiles");
            var floodSetting = new CardWindowSettings
            {
                StrokeColor = gray.ToString(),
                StrokeThickness = 15,
                CardImage = ViewUtils.GetBitmapImage(
                    System.IO.Path.Combine(
                        tilesPath, floodCard.ImagePath))
            };
            var floodCardWindow = new HorizontalCardWindow(floodSetting)
            {
                Owner = Window.GetWindow(this)
            };
            floodCardWindow.ShowDialog();
        }

        private void PickFloodCard()
        {
            for (int i = 0; i < 2; i++)
            {
                int minRand = 0;
                int maxRand = FloodCards.Count - 1;
                int randomNumber = MatchLogic.Rand.Next(minRand, maxRand);

                if (FloodCards.Count == 0)
                {
                    var shuffledDiscardStack = MatchLogic.ShuffleCards(FloodDiscardStack);
                    FloodCards.AddRange(shuffledDiscardStack);
                    FloodDiscardStack.Clear();
                }

                var floodCard = FloodCards[randomNumber];
                RefreshFloodTile(floodCard);
                ShowFloodTile(floodCard);
                FloodCards.Remove(floodCard);
                FloodDiscardStack.Add(floodCard);
            }
        }

        private void PickTreasureCard()
        {
            PendingTreasureDraws = 2;
            ContinueTreasureDraw();
        }

        public void ContinueTreasureDraw()
        {
            if (PendingTreasureDraws > 0)
            {
                PendingTreasureDraws--;
                Card card = DrawCard();
                ShowCardDraw(card);
                ExecuteCardEffect(card);
            }
            else
            {
                PickFloodCard();
            }
        }

        private Card DrawCard()
        {
            if (TreasureStack.Count == 0)
            {
                var shuffledDiscardStack = MatchLogic.ShuffleCards(TreasureDiscardStack);
                TreasureStack.AddRange(shuffledDiscardStack);
                TreasureDiscardStack.Clear();
            }

            int index = MatchLogic.Rand.Next(0, TreasureStack.Count - 1);
            Card card = TreasureStack[index];
            TreasureStack.Remove(card);
            return card;
        }

        public void ShowCardDraw(Card card)
        {
            string cardImagePath = System.IO.Path.Combine(
                CardsImagesPath, card.ImagePath);

            var cardSettings = new CardWindowSettings
            {
                StrokeThickness = 15,
                StrokeColor = "#A81D0F",
                CardImage = ViewUtils.GetBitmapImage(cardImagePath)
            };
            var cardWindow = new HorizontalCardWindow(cardSettings)
            {
                Owner = Window.GetWindow(this)
            };
            cardWindow.ShowDialog();
        }

        private void ExecuteCardEffect(Card card)
        {
            string waterRiseCard = "water-rise-name";

            if (card.Name == waterRiseCard)
            {
                IncreaseWaterLevel(card);
                TreasureDiscardStack.Add(card);
                ContinueTreasureDraw();
            }
            else
            {
                AddCardToHand(card);
            }
        }

        public void AddCardToHand(Card card)
        {
            int maxCardsInHand = 5;
            if (PlayerCards.Count < maxCardsInHand)
            {
                PlayerCards.Add(card);
                string imagePath = System.IO.Path.Combine(CardsImagesPath, card.ImagePath);
                var image = ViewUtils.GetBitmapImage(imagePath);
                var cardControl = new UserControlCard(card);
                cardControl.SetImage(image);
                cardControl.MouseLeftButtonDown += OnCardClick;

                cardStack.Children.Add(cardControl);
                AddToTreasureCardsCounter(card.Name);

                ContinueTreasureDraw();
            }
            else
            {
                var notificationWindow = new NotificationCardExceedWindow();

                notificationWindow.OnDiscard += () =>
                {
                    StateContext.SetState(new DiscardCardState(StateContext, card));
                };

                notificationWindow.OnKeep += () =>
                {
                    TreasureDiscardStack.Add(card);
                    ContinueTreasureDraw();
                };

                notificationWindow.ShowDialog();
            }
        }

        private void AddToTreasureCardsCounter(string cardName)
        {
            switch (cardName)
            {
                case "clean-code-name":
                    CleanCodeCounter++;
                    break;
                case "cubicle-keys-name":
                    CubicleKeysCounter++;
                    break;
                case "lucio-name":
                    LucioCounter++;
                    break;
                case "parking-card-name":
                    ParkingCardCounter++;
                    break;
            }
        }

        // handlers //




        /*

        private void Move_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StateContext.CurrentState.OnMoveClicked();
        }

        private void UseTreasureCard(Card card)
        {
            switch (card.Name)
            {
                case "escape-q-name":
                    NavigationService?.Navigate(new MainPage());
                    break;
                case "antivirus-name":
                    ActionsRemain--;
                    break;
            }
        }

        private void ShoreTile(TileClickedEventArgs tile)
        {
            var shoreTile = Board.GetTile(tile.Row, tile.Column);
            var tiles = MatchLogic.GetPossibleTilesToMove(CurrentTile, Board);
            tiles.Add(CurrentTile);
            MatchLogic.ResetTiles(tiles);

            shoreTile.IsFlood = false;
            shoreTile.ResetBorder();

            ActionsRemain--;
            var actionImage = actionsRemainingStack.Children[ActionsRemain];
            actionImage.Visibility = Visibility.Hidden;

            ShoreMode = false;
        }
        */


        // animations //
        private void BoardPage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                NavigationService?.Navigate(new MainPage());
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
