using Forbbiden.Client.Controls;
using Forbbiden.Client.FriendsManager;
using Forbbiden.Client.logic;
using Forbbiden.Client.ProfileManager;
using log4net;
using System;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Input;

namespace Forbbiden.Client.view
{
    /// <summary>
    /// Interaction logic for FriendsPage.xaml
    /// </summary>
    public partial class FriendsPage : Page
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FriendsPage));
        private readonly ProfileManagerClient ProfileClient;
        private readonly FriendsManagerClient FriendsClient;


        public FriendsPage()
        {
            InitializeComponent();

            ProfileClient = new ProfileManagerClient();
            FriendsClient = new FriendsManagerClient();

            FriendsNotificationSingleton.Instance.OnNewFriendRequest += OnFriendRequestReceived;
            FriendsNotificationSingleton.Instance.OnRefreshPage += RefreshFriends;

            _ = SetFriends();
            _ = SetFriendRequests();
        }

        private void OnFriendRequestReceived(FriendsNotificationManager.FriendRequest friendRequest)
        {
            Storyboard storyboard = (Storyboard)FindResource("ShowNotification");
            storyboard.Begin();
        }

        private void RefreshFriends(FriendsNotificationManager.FriendRequest friendRequest)
        {
            ReloadFriends();
        }

        private void ReloadFriends()
        {
            for (int i = 1; i < onlineStack.Children.Count; i++)
            {
                onlineStack.Children.RemoveAt(i);
            }

            for (int i = 1; i < offlineStack.Children.Count; i++)
            {
                offlineStack.Children.RemoveAt(i);
            }

            _ = SetFriends();
        }

        private async Task SetFriends()
        {
            var player = new ProfileManager.Player();

            try
            {
                player = await ProfileClient.GetPlayerByUsernameAsync(ClientSession.Username, true);
            }
            catch (FaultException<Fault> fault)
            {
                Log.Error("ERROR: FriendsPage.SetFriends", fault);
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

        private async void DeleteFriend_MouseLeftButtonDownAsync(Object sender, MouseButtonEventArgs e)
        {
            var imageClicked = sender as Image;
            var requestControl = ViewUtils.FindParent<UserControlFriend>(imageClicked);
            string friendUsername = requestControl.usernameTxtBk.Text;

            if (!string.IsNullOrEmpty(friendUsername))
            {
                bool isDeleted = false;

                try
                {
                    isDeleted = await FriendsClient.DeleteFriendAsync(friendUsername, ClientSession.Username);
                }
                catch (FaultException<Fault>)
                {
                    ViewUtils.ShowPushError(Window.GetWindow(this));
                }
                catch (TimeoutException ex)
                {
                    Log.Error("FriendsPage.DeleteFriend_MouseLeftButtonDownAsync", ex);

                }

                if (isDeleted)
                {
                    ReloadFriends();
                    string title = Properties.Resources.friend_deleted_title;
                    string message = Properties.Resources.friend_deleted_message + friendUsername;
                    ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
                }
            }
            else
            {
                string title = Properties.Resources.error;
                string message = Properties.Resources.unexpected_error;
                ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
            }
            
        }

        private async void SeeFriendProfile(Object sender, MouseButtonEventArgs e)
        {
            var imageClicked = sender as Image;
            var requestControl = ViewUtils.FindParent<UserControlFriend>(imageClicked);
            string friendUsername = requestControl.usernameTxtBk.Text;

            try
            {
                var friend = await ProfileClient.GetPlayerByUsernameAsync(friendUsername, false);
                if (friend.PlayerId != -1)
                {
                    NavigationService?.Navigate(new FriendProfilePage(friend));
                }
            }
            catch (FaultException<Fault> ex)
            {
                string classMethod = "FriendsPage.SeeFriendProfile";
                Log.Error(classMethod, ex);
                ViewUtils.ShowPullError(Window.GetWindow(this));
            }
        }

        public void AddOnlineFriend(ProfileManager.Player friend)
        {
            var friendControl = new UserControlFriend
            {
                ContextMenu = new ContextMenu()
            };

            string projectDir = ViewUtils.GetProjectDir();
            string avatarPath = System.IO.Path.Combine(projectDir, "avatars", friend.PlayerAvatarPath);
            ImageBrush avatarImage = ViewUtils.GetImageBrush(avatarPath);
            friendControl.SetAvatarImage(friendControl.avatarEllipse, avatarImage);

            friendControl.profileImage.MouseLeftButtonDown += SeeFriendProfile;
            friendControl.recycleBin.MouseLeftButtonDown += DeleteFriend_MouseLeftButtonDownAsync;

            friendControl.SetFriendUsername(friendControl.usernameTxtBk, friend.PlayerUsername);
            onlineStack.Children.Add(friendControl);
        }

        public void AddOfflineFriend(ProfileManager.Player friend)
        {
            var friendControl = new UserControlFriend();

            string projectDir = ViewUtils.GetProjectDir();
            string avatarPath = System.IO.Path.Combine(projectDir, "avatars", friend.PlayerAvatarPath);
            ImageBrush avatarImage = ViewUtils.GetImageBrush(avatarPath);
            friendControl.SetAvatarImage(friendControl.avatarEllipse, avatarImage);

            friendControl.profileImage.MouseLeftButtonDown += SeeFriendProfile;
            friendControl.recycleBin.MouseLeftButtonDown += DeleteFriend_MouseLeftButtonDownAsync;

            friendControl.SetFriendUsername(friendControl.usernameTxtBk, friend.PlayerUsername);
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
            catch (FaultException<Fault> fault)
            {
                Log.Error("FriendsPage.SendFriendRequest", fault);
                ViewUtils.ShowPullError(Window.GetWindow(this));
            }

            if (receiver.PlayerId != -1)
            {
                bool requestStatus = false;

                try
                {
                    requestStatus = await FriendsClient.SendFriendRequestAsync(ClientSession.Username, receiver.PlayerUsername);
                }
                catch (FaultException<Fault> fault)
                {
                    Log.Error("ERROR: FriendsPage.SendFriendRequest", fault);
                    ViewUtils.ShowPushError(Window.GetWindow(this));
                }

                if (requestStatus)
                {
                    string title = Properties.Resources.friend_request_sent_title;
                    string message = Properties.Resources.friend_request_sent_message + receiverUsername;
                    ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
                }
                else
                {
                    string title = Properties.Resources.error;
                    string message = Properties.Resources.friend_request_not_sent;
                    ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
                }
            }
        }

        private async void SendFriendRequestButton_Click(object sender, RoutedEventArgs e)
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
                From = glass.Height,
                To = glass.Height + 5,
                Duration = TimeSpan.FromSeconds(0.15)
            };

            var horizontalZoom = new DoubleAnimation
            {
                From = glass.Height,
                To = glass.Height + 5,
                Duration = TimeSpan.FromSeconds(0.15),
            };

            glass.BeginAnimation(HeightProperty, verticalZoom);
            glass.BeginAnimation(WidthProperty, horizontalZoom);
        }

        private void Glass_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var verticalZoom = new DoubleAnimation
            {
                From = glass.Height,
                To = glass.Height - 5,
                Duration = TimeSpan.FromSeconds(0.15)
            };

            var horizontalZoom = new DoubleAnimation
            {
                From = glass.Height,
                To = glass.Height - 5,
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
