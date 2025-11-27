using Forbbiden.Client.Controls;
using Forbbiden.Client.FriendsManager;
using Forbbiden.Client.logic;
using Forbbiden.Client.ProfileManager;
using log4net;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Forbbiden.Client.view
{
    /// <summary>
    /// Interaction logic for FriendRequestsPage.xaml
    /// </summary>
    public partial class FriendRequestsPage : Page, IFriendsManagerCallback
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FriendRequestsPage));
        private readonly ProfileManagerClient ProfileManager = new ProfileManagerClient();
        private readonly FriendsManagerClient FriendsClient;
        private readonly string ErrorTitle = Properties.Langs.Resources.error;

        public FriendRequestsPage()
        {
            InitializeComponent();

            var callbackManager = new InstanceContext(this);
            FriendsClient = new FriendsManagerClient(callbackManager);

            _ = SetRequests();
        }

        public async void OnFriendRequestReceived(FriendRequest request)
        {
            await AddRequest(request);
        }

        private async Task SetRequests()
        {
            var player = ClientSession.GetPlayer();

            if (player.PlayerId != -1)
            {
                var requests = new FriendRequest[] { };
                try
                {
                    requests = await FriendsClient.GetFriendRequestsAsync(ClientSession.Username);
                }
                catch (FaultException<DBFault> dbFault)
                {
                    Log.Error("ERROR: FriendRequestsPage.SetRequests", dbFault);
                    ViewUtils.ShowPullError(Window.GetWindow(this));
                    NavigationService?.Navigate(new FriendsPage());
                }

                if (requests.Length > 0)
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

            if (!string.IsNullOrEmpty(senderUsername))
            {
                var receiver = ClientSession.GetPlayer();

                if (receiver.PlayerId > 0)
                {
                    try
                    {
                        await FriendsClient
                            .AcceptFriendRequestAsync(senderUsername, receiver.PlayerUsername);
                        RemoveRequestStack(requestsStack, requestControl);
                    }
                    catch (FaultException<DBFault> dbFault)
                    {
                        Log.Error("ERROR: FriendRequestsPage.AcceptButton_Clicks", dbFault);
                        ViewUtils.ShowPushError(Window.GetWindow(this));

                        NavigationService?.Navigate(new FriendsPage());
                    }
                }
            }
            else
            {
                string title = ErrorTitle;
                string message = Properties.Langs.Resources.unexpected_error;
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

                if (receiver.PlayerId > -1)
                {
                    try
                    {
                        await FriendsClient.CancelFriendRequestAsync(senderUsername, receiver.PlayerUsername);
                        RemoveRequestStack(requestsStack, requestControl);
                    }
                    catch (FaultException<DBFault> dbFault)
                    {
                        Log.Error("ERROR: FriendRequestsPage.RejectButton_Click", dbFault);
                        ViewUtils.ShowPushError(Window.GetWindow(this));
                        
                        NavigationService?.Navigate(new FriendsPage());
                    }
                }
            }
            else
            {
                string title = ErrorTitle;
                string message = Properties.Langs.Resources.unexpected_error;
                ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
            }
        }

        public async Task AddRequest(FriendRequest request)
        {
            UserControlFriendRequest requestControl = new UserControlFriendRequest();

            var friend = new ProfileManager.Player();
            try
            {
                friend = await ProfileManager.GetPlayerByIdAsync(request.SenderID, false);
            }
            catch (FaultException<DBFault> dbFault)
            {
                Log.Error("ERROR: FriendRequestsPage.AddRequest", dbFault);
                ViewUtils.ShowPullError(Window.GetWindow(this));

                NavigationService?.Navigate(new FriendsPage());
            }

            string projectDir = ViewUtils.GetProjectDir();
            string avatarPath = System.IO.Path.Combine(projectDir, "avatars", friend.PlayerAvatarPath);
            ImageBrush avatarImage = ViewUtils.GetImageBrush(avatarPath);
            requestControl.SetAvatarImage(requestControl.avatarEllipse, avatarImage);

            requestControl.SetFriendUsername(requestControl.friendUsernametxtBk, friend.PlayerUsername);

            requestControl.acceptBtn.Click += AcceptButton_Click;
            requestControl.rejectBtn.Click += RejectButton_Click;

            requestsStack.Children.Add(requestControl);
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
                searchPlayer = await ProfileManager.GetPlayerByUsernameAsync(friendUsername, false);
            }
            catch (FaultException<DBFault> dbFault)
            {
                Log.Error("ERROR: FriendRequestsPage.SendFriendRequest", dbFault);
                ViewUtils.ShowPullError(Window.GetWindow(this));
                NavigationService?.Navigate(new FriendsPage());
            }

            if (searchPlayer.PlayerId != -1)
            {
                var requestStatus = false;
                try
                {
                    requestStatus = await FriendsClient.SendFriendRequestAsync(
                        ClientSession.Username, searchPlayer.PlayerUsername);
                }
                catch (FaultException<DBFault> dbFault)
                {
                    Log.Error("ERROR: FriendRequestsPage.SendFriendRequest", dbFault);
                    ViewUtils.ShowPushError(Window.GetWindow(this));
                    NavigationService?.Navigate(new FriendsPage());
                }

                if (requestStatus)
                {
                    string title = Properties.Langs.Resources.friend_request_sent_title;
                    string message = Properties.Langs.Resources.friend_request_sent_message + friendUsername;
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