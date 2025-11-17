using Forbbiden.Client.ProfileManager;
using System;
using System.IO;
using System.Linq;
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
        public BoardPage()
        {
            InitializeComponent();
            KeyDown += BoardPage_KeyDown;
            Focusable = true;
            Focus();
            var currentPlayer = new ProfileManagerClient().GetCurrentLogin();
            SetPlayersAvatars(currentPlayer);
        }


        private Ellipse GetPlayerAvatarEllipse(string avatarPath)
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri(avatarPath, UriKind.Absolute);
            bmp.EndInit();

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

        private void AddPlayerAvatar(Player player)
        {
            string projectDir = Directory.GetParent(
                    AppDomain.CurrentDomain.BaseDirectory).
                    Parent.Parent.FullName;
            string avatarPath = System.IO.Path.Combine(projectDir, "avatars", player.PlayerAvatarPath);
            Ellipse avatar = GetPlayerAvatarEllipse(avatarPath);

            Ellipse boardAvatar = GetPlayerAvatarEllipse(avatarPath);
            c2r0.Children.Add(boardAvatar);
        }

        private void SetPlayersAvatars(Player player)
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
    }
}
