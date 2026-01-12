using Forbbiden.Client.Controls;
using Forbbiden.Client.FriendsManager;
using Forbbiden.Client.Logic;
using Forbbiden.Client.Repositories;
using Forbbiden.Client.Exceptions;
using log4net;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Collections.Generic;
using Forbbiden.Client.Model;
using System.IO;

namespace Forbbiden.Client.View
{
    /// <summary>
    /// Interaction logic for FriendsPage.xaml
    /// </summary>
    public partial class FriendsPage : Page
    {
        public FriendsPage()
        {
            InitializeComponent();
            ViewUtils.SetBackground(background);

            FriendsNotificationSingleton.Instance.OnNewFriendRequest += OnFriendRequestReceived;
            FriendsNotificationSingleton.Instance.OnRefreshPage += RefreshFriends;
            FriendsNotificationSingleton.Instance.Subscribe(ClientSession.Username);

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
            ProfileManager.Player player = new ProfileManager.Player();
            try
            {
                player = await ProfileRepository.GetPlayerByUsername(ClientSession.Username, true);
            }
            catch (ViewException ex)
            {
                ExceptionViewManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
            }

            if (player.PlayerId > 0)
            {
                var onlineFriends = player.Friends
                    .Where(fs => fs.Friend.Status == 1)
                    .Select(fs => fs.Friend).ToList();
                var offlineFriends = player.Friends
                    .Where(fs => fs.Friend.Status == 0)
                    .Select(fs => fs.Friend).ToList();

                var tasks = new List<Task>();
                foreach (var friendShip in onlineFriends)
                {
                    tasks.Add(AddOnlineFriend(friendShip));
                }

                foreach (var friendShip in offlineFriends)
                {
                    tasks.Add(AddOfflineFriend(friendShip));
                }

                await Task.WhenAll(tasks);
            }
        }

        private async Task<ImageBrush> GetAvatarBrushAsync(ProfileManager.Player friend)
        {
            try
            {
                if (friend?.PlayerAvatarBytes != null && friend.PlayerAvatarBytes.Length > 0)
                {
                    return ViewUtils.GetImageBrushFromBytes(friend.PlayerAvatarBytes);
                }

                return await AvatarsManager.Instance.GetAvatarBrushAsync(friend?.PlayerUsername);
            }
            catch
            {
                return await AvatarsManager.Instance.GetAvatarBrushAsync(friend?.PlayerUsername);
            }
        }

        private async Task SetFriendRequests()
        {
            List<FriendRequest> requests = new List<FriendRequest>();
            try
            {
                requests = await FriendsRepository.GetFriendRequests(ClientSession.Username);
            }
            catch (ViewException ex)
            {
                ExceptionViewManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
            }
            if (requests.Count > 0)
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

            if (!string.IsNullOrWhiteSpace(friendUsername))
            {
                bool isDeleted = false;

                try
                {
                    isDeleted = await FriendsRepository.DeleteFriend(friendUsername, ClientSession.Username);
                }
                catch (ViewException ex)
                {
                    ExceptionViewManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
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

            ProfileManager.Player friend = new ProfileManager.Player();
            try
            {
                friend = await ProfileRepository.GetPlayerByUsername(friendUsername, false);
            }
            catch (ViewException ex)
            {
                ExceptionViewManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
            }

            if (friend.PlayerId > 0)
            {
                NavigationService?.Navigate(new FriendProfilePage(friend));
            }
        }

        public async Task AddOnlineFriend(ProfileManager.Player friend)
        {
            var friendControl = new UserControlFriend
            {
                ContextMenu = new ContextMenu()
            };

            var avatarImage = await GetAvatarBrushAsync(friend);
            friendControl.SetAvatarImage(friendControl.avatarEllipse, avatarImage);

            friendControl.profileImage.MouseLeftButtonDown += SeeFriendProfile;
            friendControl.recycleBin.MouseLeftButtonDown += DeleteFriend_MouseLeftButtonDownAsync;

            friendControl.SetFriendUsername(friendControl.usernameTxtBk, friend.PlayerUsername);
            onlineStack.Children.Add(friendControl);
        }

        public async Task AddOfflineFriend(ProfileManager.Player friend)
        {
            var friendControl = new UserControlFriend();

            var avatarImage = await GetAvatarBrushAsync(friend);
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
            ProfileManager.Player receiver = new ProfileManager.Player();

            try
            {
                receiver = await ProfileRepository.GetPlayerByUsername(receiverUsername, true);
            }
            catch (ViewException ex)
            {
                ExceptionViewManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
            }

            if (receiver.PlayerId > 0)
            {
                bool requestStatus = false;

                try
                {
                    requestStatus = await FriendsRepository.SendFriendRequest(
                        ClientSession.Username, receiver.PlayerUsername);
                }
                catch (ViewException ex)
                {
                    ExceptionViewManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));

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