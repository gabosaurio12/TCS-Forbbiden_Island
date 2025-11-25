using Forbbiden.Client.Controls;
using Forbbiden.Client.FriendsManager;
using Forbbiden.Client.logic;
using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.view.info;
using log4net;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.ServiceModel;
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
        private static readonly ILog Log = LogManager.GetLogger(typeof(FriendsPage));
        private readonly ProfileManagerClient ProfileClient = new ProfileManagerClient();
        private readonly FriendsManagerClient FriendsClient = new FriendsManagerClient();


        public FriendsPage()
        {
            InitializeComponent();
            _ = SetFriends();
            _ = SetFriendRequests();
        }

        private async Task SetFriends()
        {
            var player = new ProfileManager.Player();

            try
            {
                player = await ProfileClient.GetPlayerByUsernameAsync(ClientSession.Username, true);
            }
            catch (FaultException<DBFault> dbFault)
            {
                Log.Error("ERROR: FriendsPage.SetFriends", dbFault);
                ViewUtils.ShowPullError(Window.GetWindow(this));
            }
            if (player.PlayerId != -1)
            {
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
            }
        }

        private async Task SetFriendRequests()
        {
            var requests = await FriendsClient.GetFriendRequestsAsync(ClientSession.Username);
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

        public void AddOnlineFriend(ProfileManager.Player friend)
        {
            var friendControl = new UserControlFriend();

            string projectDir = ViewUtils.GetProjectDir();
            string avatarPath = System.IO.Path.Combine(projectDir, "avatars", friend.PlayerAvatarPath);
            ImageBrush avatarImage = ViewUtils.GetImageBrush(avatarPath);
            friendControl.SetAvatarImage(avatarImage);

            friendControl.SetFriendUsername(friend.PlayerUsername);
            onlineStack.Children.Add(friendControl);
        }

        public void AddOfflineFriend(ProfileManager.Player friend)
        {
            var friendControl = new UserControlFriend();

            string projectDir = ViewUtils.GetProjectDir();
            string avatarPath = System.IO.Path.Combine(projectDir, "avatars", friend.PlayerAvatarPath);
            ImageBrush avatarImage = ViewUtils.GetImageBrush(avatarPath);
            friendControl.SetAvatarImage(avatarImage);

            friendControl.SetFriendUsername(friend.PlayerUsername);
            offlineStack.Children.Add(friendControl);
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
            var receiver = new ProfileManager.Player();
            try
            {
                receiver = await ProfileClient.GetPlayerByUsernameAsync(receiverUsername, true);
            }
            catch (FaultException<DBFault> dbFault)
            {
                Log.Error("ERROR: FriendsPage.SendFriendRequest", dbFault);
                ViewUtils.ShowPullError(Window.GetWindow(this));
            }
            if (receiver.PlayerId != -1)
            {
                var friendsClient = new FriendsManagerClient();

                bool requestStatus = false; ;

                try
                {
                    requestStatus = await friendsClient.SendFriendRequestAsync(ClientSession.Username, receiver.PlayerUsername);
                }
                catch (FaultException<DBFault> dbFault)
                {
                    Log.Error("ERROR: FriendsPage.SendFriendRequest", dbFault);
                    ViewUtils.ShowPushError(Window.GetWindow(this));
                }

                if (requestStatus)
                {
                    string title = Properties.Langs.Resources.friend_request_sent_title;
                    string message = Properties.Langs.Resources.friend_request_sent_message + receiverUsername;
                    ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
                }
                else
                {
                    string title = Properties.Langs.Resources.error;
                    string message = Properties.Langs.Resources.friend_request_not_sent;
                    ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
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
