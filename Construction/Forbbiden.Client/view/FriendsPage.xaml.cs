using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Forbbiden.Client.FriendsManager;
using Forbbiden.Client.ProfileManager;

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
                foreach (var friend in player.Friends)
                {
                    AddOnlineFriend(friend);
                }

                var friendsClient = new FriendsManagerClient();
                var requests = friendsClient.getFriendRequests(currentLoginUsername);
                if (requests.Length > 0)
                {
                    Storyboard storyboard = (Storyboard)FindResource("ShowNotification");
                    storyboard.Begin();
                }
            }
        }

        public void AddOnlineFriend(Player friend)
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

            string irishGoverFont = "pack://application:,,,/Fonts/#Irish Grover";

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

        public void AddOfflineFriend()
        {
            StackPanel friend = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(40, 0, 40, 0),
                Background = Brushes.LightGray
            };

            string projectPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\.."));
            string avatarsPath = System.IO.Path.Combine(projectPath, "avatars", "meme-pantene.jpg");

            ImageBrush avatarImg = new ImageBrush(new System.Windows.Media.Imaging.BitmapImage(new Uri(avatarsPath)));

            Ellipse avatar = new Ellipse
            {
                Width = 100,
                Height = 100,
                VerticalAlignment = VerticalAlignment.Center,
                Fill = avatarImg,
                Margin = new Thickness(20, 20, 0, 20)
            };

            string irishGoverFont = "pack://application:,,,/Fonts/#Irish Grover";

            TextBlock friendName = new TextBlock
            {
                Text = "Friend 1",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(50, 0, 0, 0),
                FontSize = 48,
                FontFamily = new FontFamily(irishGoverFont),
            };

            friend.Children.Add(avatar);
            friend.Children.Add(friendName);
            offlineStack.Children.Add(friend);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
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

        private async Task SendFriendRequest(string friendUsername)
        {
            var profileClient = new ProfileManagerClient();
            var searchPlayer = await profileClient.GetPlayerByUsernameAsync(friendUsername);
            if (searchPlayer.PlayerId != -1)
            {
                var friendsClient = new FriendsManagerClient();
                var requestStatus = await friendsClient.SendFriendRequestAsync(currentLoginUsername, searchPlayer.PlayerUsername);

                if (requestStatus)
                {
                    MessageBox.Show("Solicitud de amistad enviada!");
                }
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string friendUsername = searchtxtBx.Text.Trim();
            await SendFriendRequest(friendUsername);
            
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var blink = new DoubleAnimation
            {
                From = 10,
                To = 8,
                Duration = TimeSpan.FromSeconds(1),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };

            var verticalShine = new DoubleAnimation
            {
                From = 50,
                To = 55,
                Duration = TimeSpan.FromSeconds(1),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };

            var horizontalShine = new DoubleAnimation
            {
                From = 50,
                To = 55,
                Duration = TimeSpan.FromSeconds(1),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };

            led.BeginAnimation(Shape.StrokeThicknessProperty, blink);
            led.BeginAnimation(Shape.HeightProperty, verticalShine);
            led.BeginAnimation(Shape.WidthProperty, horizontalShine);
        }
    }
}
