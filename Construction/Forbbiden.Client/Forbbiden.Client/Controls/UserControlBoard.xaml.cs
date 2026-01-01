using Forbbiden.Client.BoardManager;
using Forbbiden.Client.logic;
using Forbbiden.Client.ProfileManager;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel;
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
        private static readonly ILog Log = LogManager.GetLogger(typeof(UserControlBoard));

        public readonly int NumberOfTreasures = 4;
        private readonly string TilesImagesPath;
        private readonly string ImagesPath;
        private readonly BoardManagerClient Client = new BoardManagerClient();

        public UserControlBoard()
        {
            InitializeComponent();

            string projectDir = ViewUtils.GetProjectDir();

            ImagesPath = System.IO.Path.Combine(
                projectDir, "Images");
            TilesImagesPath = System.IO.Path.Combine(
                ImagesPath, "tiles");

            GenerateBoard();
        }

        public void GenerateBoard()
        {
            BuildBoard();
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

        public UserControlTile GetTile(int row, int col)
        {
            UserControlTile tile = new UserControlTile
            {
                Row = -1,
                Col = -1
            };
            foreach (UserControlTile childTile in boardGrid.Children)
            {
                if (childTile.Row == row && childTile.Col == col)
                {
                    tile = childTile;
                    break;
                }
            }
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

        private void SetTreasureTiles()
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
                    treasureCard = Client.GetCard(treasureImage);
                }
                catch (FaultException<DBFault> ex)
                {
                    string classMethod = "UserControlBoard.SetTreasureTiles";
                    Log.Error(classMethod, ex);
                    ViewUtils.ShowPullError(Window.GetWindow(this));
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

        private void SetNonTreasureTiles()
        {
            List<UserControlTile> tiles = GetAllTilesFromGrid();
            Card[] tilesCards;
            try
            {
                tilesCards = Client.GetFloodCards();
            }
            catch (FaultException<DBFault> ex)
            {
                string classMethod = "UserControlBoard.SetNonTreasureTiles";
                Log.Error(classMethod, ex);
                ViewUtils.ShowPullError(Window.GetWindow(this));
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

        public UserControlTile AddPlayerAvatar(Player player)
        {
            Ellipse boardAvatar = ViewUtils.GetAvatarEllipse(player.PlayerAvatarPath);

            var tiles = GetAllTilesFromGrid();
            bool avatarPlaced = false;
            UserControlTile spawnTile;
            do
            {
                int spawnTileIndex = MatchLogic.Rand.Next(tiles.Count);
                spawnTile = tiles[spawnTileIndex];
                if (!spawnTile.IsTreasure)
                {
                    spawnTile.tileGrid.Children.Add(boardAvatar);
                    avatarPlaced = true;
                }
            } while (!avatarPlaced);

            return spawnTile;
        }

        public event EventHandler<TileClickedEventArgs> TileClickedOnBoard;

        private void OnTileClicked(object sender, TileClickedEventArgs e)
        {
            TileClickedOnBoard?.Invoke(this, e);
        }
    }
}
