using Forbbiden.Client.Controls;
using Forbbiden.Client.Exceptions;
using Forbbiden.Client.FriendsManager;
using Forbbiden.Client.Logic;
using Forbbiden.Client.Model;
using Forbbiden.Client.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml.Linq;

namespace Forbbiden.Client.View
{
    /// <summary>
    /// Interaction logic for FriendRequestsPage.xaml
    /// </summary>
    public partial class FriendRequestsPage : Page
    {
        private readonly ProfileRepository ProfileRepo;
        private readonly FriendsRepository FriendsRepo;
        private readonly string ErrorTitle = Properties.Resources.error;

        public FriendRequestsPage()
        {
            InitializeComponent();
            ViewUtils.SetBackground(background);

            ProfileRepo = new ProfileRepository();
            FriendsRepo = new FriendsRepository();

            FriendsNotificationSingleton.Instance.Subscribe(ClientSession.Username);
            FriendsNotificationSingleton.Instance.OnNewFriendRequest += OnFriendRequestReceived;

            _ = SetRequests();
        }

        public async void OnFriendRequestReceived(FriendsNotificationManager.FriendRequest friendRequest)
        {
            var request = new FriendRequest
            {
                ReceiverID = friendRequest.ReceiverID,
                SenderID = friendRequest.SenderID,
                Status = friendRequest.Status,
            };

            await AddRequest(request);
        }

        private async Task SetRequests()
        {
            var player = ClientSession.GetPlayer();

            if (player.PlayerId > 0)
            {
                var requests = new List<FriendRequest>();
                try
                {
                    requests = await FriendsRepo.GetFriendRequests(ClientSession.Username);
                }
                catch (ViewException ex)
                {
                    ExceptionViewManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
                }

                if (requests.Count > 0)
                {
                    foreach (var request in requests)
                    {
                        if (request.Status == 0)
                        {
                            _ = AddRequest(request);
                        }
                    }
                }
            }
        }

        private static void RemoveRequestStack(StackPanel stackRemoving, UserControlFriendRequest controlToRemove)
        {
            stackRemoving.Children.Remove(controlToRemove);
        }

        private async void AcceptButton_Click(Object sender, RoutedEventArgs e)
        {
            var buttonClicked = sender as Button;
            var requestControl = ViewUtils.FindParent<UserControlFriendRequest>(buttonClicked);
            string senderUsername = requestControl.friendUsernametxtBk.Text;

            if (!string.IsNullOrWhiteSpace(senderUsername))
            {
                var receiver = ClientSession.GetPlayer();

                if (receiver.PlayerId > 0)
                {
                    try
                    {
                        await FriendsRepo
                            .AcceptFriendRequest(senderUsername, receiver.PlayerUsername);
                        RemoveRequestStack(requestsStack, requestControl);
                    }
                    catch (ViewException ex)
                    {
                        ExceptionViewManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
                        NavigationService?.Navigate(new FriendsPage());
                    }
                }
            }
            else
            {
                string title = ErrorTitle;
                string message = Properties.Resources.unexpected_error;
                ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
            }
        }

        private async void RejectButton_Click(Object sender, RoutedEventArgs e)
        {
            var buttonClicked = sender as Button;
            var requestControl = ViewUtils.FindParent<UserControlFriendRequest>(buttonClicked);
            string senderUsername = requestControl.friendUsernametxtBk.Text;

            if (!string.IsNullOrEmpty(senderUsername))
            {
                var receiver = ClientSession.GetPlayer();

                if (receiver.PlayerId > 0)
                {
                    try
                    {
                        await FriendsRepo.CancelFriendRequest(senderUsername, receiver.PlayerUsername);
                        RemoveRequestStack(requestsStack, requestControl);
                    }
                    catch (ViewException ex)
                    {
                        ExceptionViewManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
                        NavigationService?.Navigate(new FriendsPage());
                    }
                }
            }
            else
            {
                string title = ErrorTitle;
                string message = Properties.Resources.unexpected_error;
                ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
            }
        }

        public async Task AddRequest(FriendRequest request)
        {
            UserControlFriendRequest requestControl = new UserControlFriendRequest();

            var friend = new ProfileManager.Player();
            try
            {
                friend = await ProfileRepository.GetPlayerById(request.SenderID, false);
            }
            catch (ViewException ex)
            {
                ExceptionViewManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));

                NavigationService?.Navigate(new FriendsPage());
            }

            var avatarImage = await AvatarsManager.Instance.GetAvatarBrushAsync(friend.PlayerUsername);
            requestControl.SetAvatarImage(requestControl.avatarEllipse, avatarImage);

            bool downloaded = await DownloadFriendImage(avatarPath);
            ImageBrush avatarImage;

            if (!downloaded)
            {
                avatarImage = ViewUtils.GetDefaultAvatarBrush();
            }
            else
            {
                avatarImage = ViewUtils.GetImageBrush(avatarPath);
            }

            requestControl.SetAvatarImage(requestControl.avatarEllipse, avatarImage);
            requestControl.SetFriendUsername(requestControl.friendUsernametxtBk, friend.PlayerUsername);

            requestControl.acceptBtn.Click += AcceptButton_Click;
            requestControl.rejectBtn.Click += RejectButton_Click;

            requestsStack.Children.Add(requestControl);
        }

        private async Task<bool> DownloadFriendImage(string avatarPath)
        {
            bool downloaded = false;
            if (!File.Exists(avatarPath))
            {
                var bytes = await ProfileRepository.DownloadAvatar(Path.GetFileName(avatarPath));
                downloaded = bytes.Length > 0;
            }

            return downloaded;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new FriendsPage());
        }

        private void SearchFriend_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
            var searchPlayer = new ProfileManager.Player();
            try
            {
                searchPlayer = await ProfileRepo.GetPlayerByUsername(friendUsername, false);
            }
            catch (ViewException ex)
            {
                ExceptionViewManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
                NavigationService?.Navigate(new FriendsPage());
            }

            if (searchPlayer.PlayerId > 0)
            {
                var requestStatus = false;
                try
                {
                    requestStatus = await FriendsRepo.SendFriendRequest(
                        ClientSession.Username, searchPlayer.PlayerUsername);
                }
                catch (ViewException ex)
                {
                    ExceptionViewManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
                    NavigationService?.Navigate(new FriendsPage());
                }

                if (requestStatus)
                {
                    string title = Properties.Resources.friend_request_sent_title;
                    string message = Properties.Resources.friend_request_sent_message + friendUsername;
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

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string friendUsername = searchtxtBx.Text.Trim();
            await SendFriendRequest(friendUsername);
        }

        private void Glass_MouseEnter(object sender, MouseEventArgs e)
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

        private void Glass_MouseLeave(object sender, MouseEventArgs e)
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

        private void Glass_MouseDown(object sender, MouseEventArgs e)
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

        private void Glass_MouseUp(object sender, MouseEventArgs e)
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
    }
}