using Forbbiden.Client.FriendsManager;
using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.view.info;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Forbbiden.Client.view
{
    /// <summary>
    /// Interaction logic for FriendsPage.xaml
    /// </summary>
    public partial class FriendsPage : Page
    {
        private string currentLoginUsername;
        public FriendsPage()
        {
            InitializeComponent();
            SetFriends();
        }

        private void SetFriends()
        {
            var profileManager = new ProfileManagerClient();

            var player = profileManager.GetCurrentLogin();
            if (player.PlayerId != -1)
            {
                currentLoginUsername = player.PlayerUsername;

                var onlineFriends = player.Friends
                    .Where(fs => fs.Friend.Status == 1)
                    .Select(fs => fs.Friend).ToList();
                var offlineFriends = player.Friends
                    .Where(fs => fs.Friend.Status == 0)
                    .Select(fs => fs.Friend).ToList();

                foreach (var friendShip in onlineFriends)
                {
                    AddOnlineFriend(friendShip);
                }

                foreach (var friendShip in offlineFriends)
                {
                    AddOfflineFriend(friendShip);
                }

                var friendsClient = new FriendsManagerClient();
                var requests = friendsClient.getFriendRequests(currentLoginUsername);
                if (requests.Length > 0)
                {
                    Storyboard storyboard = (Storyboard)FindResource("ShowNotification");
                    storyboard.Begin();
                }
                else
                {
                    Storyboard storyboard = (Storyboard)FindResource("HideNotification");
                    storyboard.Begin();
                }
            }
        }

        private void OpenNotification(string title, string message)
        {
            var notificationWindow = new NotificationWindow(title, message)
            {
                Owner = Window.GetWindow(this)
            };
            notificationWindow.ShowDialog();
        }

        public void AddOnlineFriend(ProfileManager.Player friend)
        {
            StackPanel friendStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(40,0,40,0),
                Background = Brushes.LightGray
            };

            ImageBrush avatarImg = new ImageBrush(new System.Windows.Media.Imaging.BitmapImage(new Uri(friend.PlayerAvatarPath)));

            Ellipse avatar = new Ellipse
            {
                Width = 100,
                Height = 100,
                VerticalAlignment = VerticalAlignment.Center,
                Fill = avatarImg,
                Margin = new Thickness(20,20,0,20)
            };

            string irishGoverFont = "{StaticResource IrishGrover}";

            TextBlock friendName = new TextBlock
            {
                Text = friend.PlayerUsername,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(50, 0, 0, 0),
                FontSize = 48,
                FontFamily = new FontFamily(irishGoverFont),
            };

            friendStack.Children.Add(avatar);
            friendStack.Children.Add(friendName);
            onlineStack.Children.Add(friendStack);
        }

        public void AddOfflineFriend(ProfileManager.Player friend)
        {
            StackPanel friendStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(40, 0, 40, 0),
                Background = Brushes.LightGray
            };

            string projectDir = Directory.GetParent(
                    AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
            string avatarPath = System.IO.Path.Combine(projectDir, "avatars", friend.PlayerAvatarPath);

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri(avatarPath, UriKind.Absolute);
            bmp.EndInit();

            Ellipse avatar = new Ellipse
            {
                Width = 100,
                Height = 100,
                VerticalAlignment = VerticalAlignment.Center,
                Fill = new ImageBrush(bmp),
                Margin = new Thickness(20, 20, 0, 20)
            };

            string irishGoverFont = "{StaticResource IrishGrover}";

            TextBlock friendName = new TextBlock
            {
                Text = friend.PlayerUsername,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(50, 0, 0, 0),
                FontSize = 48,
                FontFamily = new FontFamily(irishGoverFont),
            };

            friendStack.Children.Add(avatar);
            friendStack.Children.Add(friendName);
            offlineStack.Children.Add(friendStack);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new MainPage());
        }

        private void SearchFriend_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (searchtxtBx.Width == 0)
            {
                Storyboard barStoryBoard = (Storyboard)FindResource("ShowSearchBar");
                barStoryBoard.Begin();
                Storyboard buttonStoryBoard = (Storyboard)FindResource("ShowAddButton");
                buttonStoryBoard.Begin();
            }
            else
            {
                Storyboard storyBoard = (Storyboard)FindResource("HideSearchBar");
                storyBoard.Begin();
                Storyboard buttonStoryBoard = (Storyboard)FindResource("HideAddButton");
                buttonStoryBoard.Begin();
            }
        }

        private async Task SendFriendRequest(string receiverUsername)
        {
            var profileClient = new ProfileManagerClient();

            var receiver = await profileClient.GetPlayerByUsernameAsync(receiverUsername, true);
            if (receiver.PlayerId != -1)
            {
                var friendsClient = new FriendsManagerClient();
                var requestStatus = await friendsClient.SendFriendRequestAsync(currentLoginUsername, receiver.PlayerUsername);

                if (requestStatus)
                {
                    string title = Properties.Langs.Resources.friend_request_sent_title;
                    string message = Properties.Langs.Resources.friend_request_sent_message + receiverUsername;
                    OpenNotification(title, message);
                }
                else
                {
                    string title = Properties.Langs.Resources.error;
                    string message = Properties.Langs.Resources.friend_request_not_sent;
                    OpenNotification(title, message);
                }
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string receiverUsername = searchtxtBx.Text.Trim();
            await SendFriendRequest(receiverUsername);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var blink = new DoubleAnimation
            {
                From = 10,
                To = 6,
                Duration = TimeSpan.FromSeconds(0.8),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };

            var verticalShine = new DoubleAnimation
            {
                From = 50,
                To = 55,
                Duration = TimeSpan.FromSeconds(0.8),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };

            var horizontalShine = new DoubleAnimation
            {
                From = 50,
                To = 55,
                Duration = TimeSpan.FromSeconds(0.8),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };

            led.BeginAnimation(Shape.StrokeThicknessProperty, blink);
            led.BeginAnimation(HeightProperty, verticalShine);
            led.BeginAnimation(WidthProperty, horizontalShine);
        }

        private void Glass_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var verticalZoom = new DoubleAnimation
            {
                From = 100,
                To = 105,
                Duration = TimeSpan.FromSeconds(0.15)
            };

            var horizontalZoom = new DoubleAnimation
            {
                From = 100,
                To = 105,
                Duration = TimeSpan.FromSeconds(0.15),
            };

            glass.BeginAnimation(HeightProperty, verticalZoom);
            glass.BeginAnimation(WidthProperty, horizontalZoom);
        }

        private void Glass_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var verticalZoom = new DoubleAnimation
            {
                From = 105,
                To = 100,
                Duration = TimeSpan.FromSeconds(0.15)
            };

            var horizontalZoom = new DoubleAnimation
            {
                From = 105,
                To = 100,
                Duration = TimeSpan.FromSeconds(0.15),
            };

            glass.BeginAnimation(HeightProperty, verticalZoom);
            glass.BeginAnimation(WidthProperty, horizontalZoom);
        }

        private void Glass_MouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var verticalZoom = new DoubleAnimation
            {
                From = glass.Height,
                To = glass.Height + 7.5,
                Duration = TimeSpan.FromSeconds(0.15)
            };

            var horizontalZoom = new DoubleAnimation
            {
                From = glass.Width,
                To = glass.Width + 7.5,
                Duration = TimeSpan.FromSeconds(0.15),
            };

            glass.BeginAnimation(HeightProperty, verticalZoom);
            glass.BeginAnimation(WidthProperty, horizontalZoom);
        }

        private void Glass_MouseUp(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var verticalZoom = new DoubleAnimation
            {
                From = glass.Height,
                To = glass.Height - 7.5,
                Duration = TimeSpan.FromSeconds(0.15)
            };

            var horizontalZoom = new DoubleAnimation
            {
                From = glass.Width,
                To = glass.Width - 7.5,
                Duration = TimeSpan.FromSeconds(0.15),
            };

            glass.BeginAnimation(HeightProperty, verticalZoom);
            glass.BeginAnimation(WidthProperty, horizontalZoom);
        }

        private void NotificationGrid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new FriendRequestsPage());
        }
    }
}
