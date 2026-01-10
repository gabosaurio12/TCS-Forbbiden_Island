using Forbbiden.Client.BoardManager;
using Forbbiden.Client.Exceptions;
using Forbbiden.Client.Logic;
using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Forbbiden.Client.Controls
{
    /// <summary>
    /// Interaction logic for UserControlBoard.xaml
    /// </summary>
    public partial class UserControlBoard : UserControl
    {
        public readonly int NumberOfTreasures = 4;
        private readonly string TilesImagesPath;
        private readonly string ImagesPath;

        public UserControlBoard()
        {
            InitializeComponent();

            string projectDir = ViewUtils.GetProjectDir();

            ImagesPath = System.IO.Path.Combine(
                projectDir, "Images");
            TilesImagesPath = System.IO.Path.Combine(
                ImagesPath, "tiles");

            BuildBoard();
        }

        public void GenerateBoard()
        {
            SetTreasureTiles();
            SetNonTreasureTiles();
        }

        private void BuildBoard()
        {
            int boardRows = 5;
            int boardCols = 5;

            var borderTilesXY = new HashSet<(int x, int y)>
            {
                (0, 2), (0, 3), (5, 2), (5, 3),
                (2, 0), (3, 0), (2, 5), (3, 5)
            };

            foreach (var (x, y) in borderTilesXY)
            {
                UserControlTile tile = new UserControlTile
                {
                    Col = x,
                    Row = y
                };
                Grid.SetColumn(tile, tile.Col);
                Grid.SetRow(tile, tile.Row);
                tile.TileClicked += OnTileClicked;
                boardGrid.Children.Add(tile);
            }

            for (int row = 1; row < boardRows; row++)
            {
                for (int col = 1; col < boardCols; col++)
                {
                    UserControlTile tile = new UserControlTile
                    {
                        Col = col,
                        Row = row
                    };
                    Grid.SetColumn(tile ,tile.Col);
                    Grid.SetRow(tile,tile.Row);
                    tile.TileClicked += OnTileClicked;
                    boardGrid.Children.Add(tile);
                }
            }
        }

        public void SetTile(UserControlTile newTile)
        {
            var oldTile = GetTile(newTile.Row, newTile.Col);
            oldTile?.SetTile(newTile);
        }

        public UserControlTile GetTile(int row, int col)
        {
            UserControlTile tile = boardGrid.Children.
                OfType<UserControlTile>().
                FirstOrDefault(t => t.Row == row && t.Col == col);
            return tile;
        }

        public List<UserControlTile> GetAllTilesFromGrid()
        {
            return boardGrid.Children
                .OfType<UserControlTile>()
                .ToList();
        }

        private List<UserControlTile> GetInnerTilesFromGrid()
        {
            return boardGrid.Children
                .OfType<UserControlTile>()
                .Where(t => t.Col != 0 && t.Col != 5 && t.Row != 0 && t.Row != 5)
                .ToList();
        }

        public List<UserControlTile> GetFloodedTiles()
        {
            return boardGrid.Children
                .OfType<UserControlTile>()
                .Where(t => t.IsFlood).ToList();
        }

        private async void SetTreasureTiles()
        {
            List<UserControlTile> innerTiles = GetInnerTilesFromGrid();
            var shuffledTiles = innerTiles.OrderBy(
                x => MatchLogic.Rand.Next()).Take(NumberOfTreasures).ToList();

            for (int i = 0; i < NumberOfTreasures; i++)
            {
                string treasureImage = $"treasure{i + 1}.png";
                Card treasureCard = null;
                try
                {
                    treasureCard = await BoardRepository.GetCard(treasureImage);
                }
                catch (ViewException ex)
                {
                    ExceptionViewManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
                }

                string treasureImagePath = System.IO.Path.Combine(TilesImagesPath, treasureImage);
                var treasureBitmap = ViewUtils.GetBitmapImage(treasureImagePath);
                shuffledTiles[i].SetTileAsTreasure(treasureBitmap, treasureCard);
            }
        }

        private void AddTileImageAndCardInfo(Card cardTile, UserControlTile tile)
        {
            string escapeCardName = "entrance-name";
            if (cardTile.Name == escapeCardName)
            {
                tile.SetTileAsEscape();
            }

            string tileImage = cardTile.ImagePath;
            string tileImagePath = System.IO.Path.Combine(TilesImagesPath, tileImage);
            var tileBitmap = ViewUtils.GetBitmapImage(tileImagePath);

            tile.ImageFileName = tileImage;
            tile.SetImage(tileBitmap);               
        }

        private async void SetNonTreasureTiles()
        {
            List<UserControlTile> tiles = GetAllTilesFromGrid();
            List<Card> tilesCards;
            try
            {
                tilesCards = await BoardRepository.GetFloodCards();
            }
            catch (ViewException ex)
            {
                ExceptionViewManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
                return;
            }
            var shuffledTilesCards = MatchLogic.ShuffleCards(tilesCards.ToList());

            int tileIndex = 0;
            foreach (var tile in tiles)
            {
                if (!tile.IsTreasure)
                {
                    AddTileImageAndCardInfo(shuffledTilesCards[tileIndex], tile);
                    tileIndex++;
                }
            }          
        }

        public void AddPlayerAvatar(Player player, UserControlTile tile)
        {
            Ellipse boardAvatar = ViewUtils.GetAvatarEllipse(player.PlayerAvatarPath);

            var spawnTile = GetTile(tile.Row, tile.Col);
            spawnTile.tileGrid.Children.Add(boardAvatar);
        }

        public event EventHandler<TileClickedEventArgs> TileClickedOnBoard;

        private void OnTileClicked(object sender, TileClickedEventArgs e)
        {
            TileClickedOnBoard?.Invoke(this, e);
        }

        public void RefreshBoardTiles(List<Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                var tileToRefresh = GetTile(tile.Row, tile.Column);
                if (tileToRefresh != null)
                {
                    SetTileDataToUserControlTile(tile, tileToRefresh);
                    tileToRefresh.RefreshTile();
                }
            }
        }

        private void SetTileDataToUserControlTile(Tile tile, UserControlTile controlTile)
        {
            controlTile.Row = tile.Row;
            controlTile.Col = tile.Column;
            controlTile.IsFlood = tile.IsFlood;
            controlTile.IsEscapeTile = tile.IsEscape;
            controlTile.IsTreasure = tile.IsTreasure;
            controlTile.IsLost = tile.IsLost;
            controlTile.ImageFileName = tile.ImageFileName;
            controlTile.TreasureCard = tile.TreasureCard;
        }
    }
}
