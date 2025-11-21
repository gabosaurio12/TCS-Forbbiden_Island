using Forbbiden.Client.logic;
using Forbbiden.Client.ProfileManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Forbbiden.Client.view.games
{
    /// <summary>
    /// Interaction logic for UserControlBoard.xaml
    /// </summary>
    public partial class UserControlBoard : UserControl
    {
        private readonly int NumberOfTreasures = 4;
        private readonly string AvatarImagesPath;
        private readonly string TilesImagesPath;
        private readonly string ImagesPath;

        public UserControlBoard()
        {
            InitializeComponent();

            string projectDir = Directory.GetParent(
                AppDomain.CurrentDomain.BaseDirectory).
                Parent.Parent.FullName;

            ImagesPath = System.IO.Path.Combine(
                projectDir, "Images");
            TilesImagesPath = System.IO.Path.Combine(
                projectDir, ImagesPath, "tiles");

            GenerateBoard();
        }

        public void GenerateBoard()
        {
            BuildBoard();
            SetTreasureTiles();
            FillFreeTiles();
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
                boardGrid.Children.Add(tile);
                Grid.SetColumn(tile, tile.Col);
                Grid.SetRow(tile, tile.Row);
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
                    boardGrid.Children.Add(tile);
                }
            }
        }

        private List<UserControlTile> GetAllTilesFromGrid()
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

        private void SetTreasureTiles()
        {
            List<UserControlTile> tiles = GetInnerTilesFromGrid();
            Console.WriteLine(tiles.Count);
            Random rand = new Random();

            var shuffledTiles = tiles.OrderBy(x => rand.Next()).Take(NumberOfTreasures).ToList();

            for (int i = 0; i < shuffledTiles.Count; i++)
            {
                string treasureImage = $"treasure{i + 1}.png";
                string treasureImagePath = System.IO.Path.Combine(TilesImagesPath, treasureImage);
                var treasureBitmap = ViewUtils.GetImage(treasureImagePath);

                UserControlTile tile = shuffledTiles[i];
                string redColorCode = "#A81D0F";
                Color redColor = (Color)ColorConverter.ConvertFromString(redColorCode);
                tile.SetBorderBrush(redColor);
                tile.SetImage(treasureBitmap);
                tile.IsTreasure = true;
            }
        }

        private void FillFreeTiles()
        {
            string freeTileImage = "free.jpg";
            string imagePath = System.IO.Path.Combine(
                TilesImagesPath, freeTileImage);
            var freeTileBitmap = ViewUtils.GetImage(imagePath);

            List<UserControlTile> tiles = GetAllTilesFromGrid();
            foreach (var tile in tiles)
            {
                if (!tile.IsTreasure)
                    tile.SetImage(freeTileBitmap);
            }
        }

        private Ellipse GetAvatarEllipse(string avatarPath)
        {

            var avatarBitmap = ViewUtils.GetImage(avatarPath);

            Ellipse ellipse = new Ellipse
            {
                Width = 100,
                Height = 100,
                Stroke = Brushes.LightGray,
                StrokeThickness = 5,
                Margin = new Thickness(0, 0, 0, 0),
                Fill = new ImageBrush
                {
                    ImageSource = avatarBitmap,
                    Stretch = Stretch.UniformToFill
                }
            };

            return ellipse;
        }

        public void AddPlayerAvatar(Player player)
        {
            string projectDir = Directory.GetParent(
                    AppDomain.CurrentDomain.BaseDirectory).
                    Parent.Parent.FullName;
            string avatarPath = System.IO.Path.Combine(projectDir, "avatars", player.PlayerAvatarPath);
            Ellipse boardAvatar = GetAvatarEllipse(avatarPath);

            var tiles = GetAllTilesFromGrid();
            bool avatarPlaced = false;
            do
            {
                int spawnTileIndex = new Random().Next(tiles.Count);
                var spawnTile = tiles[spawnTileIndex];
                if (!spawnTile.IsTreasure)
                {
                    spawnTile.tileGrid.Children.Add(boardAvatar);
                    avatarPlaced = true;
                }
            } while (!avatarPlaced);
        }
    }
}
