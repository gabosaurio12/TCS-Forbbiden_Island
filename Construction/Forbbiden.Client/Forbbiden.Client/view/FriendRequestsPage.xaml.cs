using Forbbiden.Client.FriendsManager;
using Forbbiden.Client.logic;
using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.view.info;
using log4net;
using System;
using System.Numerics;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Forbbiden.Client.view
{
    /// <summary>
    /// Interaction logic for FriendRequestsPage.xaml
    /// </summary>
    public partial class FriendRequestsPage : Page
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FriendRequestsPage));
        private readonly CallbacksManager callback;
        private string currentLoginUsername;
        private readonly FontFamily IrishGrover;
        private readonly ProfileManagerClient profileManager = new ProfileManagerClient();
        private readonly FriendsManagerClient friendsClient = new FriendsManagerClient();

        public FriendRequestsPage()
        {
            InitializeComponent();

            IrishGrover = (FontFamily)Application.Current.Resources["IrishGrover"];

            callback = new CallbacksManager();
            callback.FriendRequestReceived += OnFriendRequestReceived;

            _ = SetRequests();
        }

        private void HandlePullDBFault(FaultException<DBFault> dbFault)
        {
            log.Error(dbFault.Detail);
            string title = Properties.Langs.Resources.error;
            string message = Properties.Langs.Resources.pull_database_error;
            var notificationWindow = new NotificationWindow(title, message)
            {
                Owner = Window.GetWindow(this)
            };
            notificationWindow.ShowDialog();
        }

        private void HandlePushDBFault(FaultException<DBFault> dbFault)
        {
            log.Error(dbFault.Detail);
            string title = Properties.Langs.Resources.error;
            string message = Properties.Langs.Resources.push_database_error;
            var notificationWindow = new NotificationWindow(title, message)
            {
                Owner = Window.GetWindow(this)
            };
            notificationWindow.ShowDialog();
        }

        private async Task<ProfileManager.Player> GetCurrentLogin()
        {
            var player = new ProfileManager.Player();
            try
            {
                player = await profileManager.GetCurrentLoginAsync();
            }
            catch (FaultException<DBFault> dbFault)
            {
                HandlePullDBFault(dbFault);
                NavigationService?.Navigate(new FriendsPage());
            }

            return player;
        }

        private void OnFriendRequestReceived(FriendRequest request)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _ = AddRequest(request);
            });
        }

        private async Task SetRequests()
        {
            var player = await GetCurrentLogin();

            if (player.PlayerId != -1)
            {
                currentLoginUsername = player.PlayerUsername;

                var requests = new FriendRequest[] { };
                try
                {
                    requests = await friendsClient.GetFriendRequestsAsync(currentLoginUsername);
                }
                catch (FaultException<DBFault> dbFault)
                {
                    HandlePullDBFault(dbFault);
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

        private void OpenNotification(string title, string message)
        {
            var notificationWindow = new NotificationWindow(title, message)
            {
                Owner = Window.GetWindow(this)
            };
            notificationWindow.ShowDialog();
        }

        private static string GettxtBkText(Grid stack)
        {
            string text = "";
            foreach (var child in stack.Children)
            {
                if (child is TextBlock textBlock)
                {
                    text = textBlock.Text;
                    break;
                }
            }

            return text;
        }

        private static void RemoveRequestStack(StackPanel stackRemoving, Grid gridToRemove)
        {
            stackRemoving.Children.Remove(gridToRemove);
        }

        private async void AcceptButton_Click(Object sender, RoutedEventArgs e)
        {
            var grid = (Grid)((Button)sender).Parent;
            string senderUsername = GettxtBkText(grid);

            if (!string.IsNullOrEmpty(senderUsername))
            {
                var receiver = await GetCurrentLogin();

                if (receiver.PlayerId != -1)
                {
                    var requestStatus = false;
                    try
                    {
                        requestStatus = await friendsClient
                            .AcceptFriendRequestAsync(senderUsername, receiver.PlayerUsername);
                    }
                    catch (FaultException<DBFault> dbFault)
                    {
                        HandlePushDBFault(dbFault);
                        NavigationService?.Navigate(new FriendsPage());
                    }

                    if (!requestStatus)
                    {
                        OpenNotification(Properties.Langs.Resources.error,
                            Properties.Langs.Resources.push_database_error);
                    }
                    else
                    {
                        RemoveRequestStack(requestsStack, grid);
                    }
                }
                else
                {
                    OpenNotification(Properties.Langs.Resources.error,
                        Properties.Langs.Resources.pull_database_error);
                }
            }
            else
            {
                OpenNotification(Properties.Langs.Resources.error,
                    Properties.Langs.Resources.unexpected_error);
            }
        }

        private async void RejectButton_Click(Object sender, RoutedEventArgs e)
        {
            var grid = (Grid)((Button)sender).Parent;
            string senderUsername = GettxtBkText(grid);

            if (!string.IsNullOrEmpty(senderUsername))
            {
                var receiver = await GetCurrentLogin();

                if (receiver.PlayerId != -1)
                {
                    var requestStatus = false;
                    try
                    {
                        requestStatus = await friendsClient
                            .CancelFriendRequestAsync(senderUsername, receiver.PlayerUsername);
                    }
                    catch (FaultException<DBFault> dbFault)
                    {
                        HandlePushDBFault(dbFault);
                        NavigationService?.Navigate(new FriendsPage());
                    }

                    if (!requestStatus)
                    {
                        OpenNotification(Properties.Langs.Resources.error,
                            Properties.Langs.Resources.push_database_error);
                    }
                    else
                    {
                        RemoveRequestStack(requestsStack, grid);
                    }
                }
                else
                {
                    OpenNotification(Properties.Langs.Resources.error,
                        Properties.Langs.Resources.pull_database_error);
                }
            }
            else
            {
                OpenNotification(Properties.Langs.Resources.error,
                    Properties.Langs.Resources.unexpected_error);
            }
        }

        private Button CreateGridButton(string action)
        {
            var green = (Brush)new BrushConverter().ConvertFromString("#43C414");
            var red = (Brush)new BrushConverter().ConvertFromString("#C41414");

            if (action == "accept")
            {
                string accept = "+";
                Button acceptRequest = new Button
                {
                    Content = accept,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(20, 0, 10, 0),
                    Width = 60,
                    Height = 60,
                    FontSize = 54,
                    FontFamily = IrishGrover,
                    Background = green,
                    Foreground = Brushes.White,
                    Cursor = Cursors.Hand
                };
                acceptRequest.Click += AcceptButton_Click;
                return acceptRequest;
            }
            else
            {
                string reject = "-";
                Button rejectRequest = new Button
                {
                    Content = reject,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(20, 0, 10, 0),
                    Width = 60,
                    Height = 60,
                    FontSize = 54,
                    FontFamily = IrishGrover,
                    Background = red,
                    Foreground = Brushes.White,
                    Cursor = Cursors.Hand
                };
                rejectRequest.Click += RejectButton_Click;
                return rejectRequest;
            }
        }

        private Ellipse CreateGridEllipse(ImageBrush avatarImg)
        {
            Ellipse avatar = new Ellipse
            {
                Width = 100,
                Height = 100,
                VerticalAlignment = VerticalAlignment.Center,
                Fill = avatarImg,
                Margin = new Thickness(20, 20, 0, 20)
            };

            return avatar;
        }

        private TextBlock CreateGridTextBlock(string username)
        {
            TextBlock friendName = new TextBlock
            {
                Text = username,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(50, 0, 0, 0),
                FontSize = 48,
                FontFamily = IrishGrover,
            };
            Grid.SetColumn(friendName, 1);

            return friendName;
        }

        public async Task AddRequest(FriendRequest request)
        {
            Grid requestGrid = new Grid
            {
                Margin = new Thickness(40, 0, 40, 0),
                Width = 800,
                Background = Brushes.LightGray
            };

            requestGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            requestGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            requestGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            requestGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var friend = new ProfileManager.Player();
            try
            {
                friend = await profileManager.GetPlayerByIdAsync(request.SenderID, false);
            }
            catch (FaultException<DBFault> dbFault)
            {
                HandlePullDBFault(dbFault);
                NavigationService?.Navigate(new FriendsPage());
            }

            ImageBrush avatarImg = new ImageBrush(new BitmapImage(new Uri(friend.PlayerAvatarPath)));
            var avatar = CreateGridEllipse(avatarImg);
            Grid.SetColumn(avatar, 0);

            var friendName = CreateGridTextBlock(friend.PlayerUsername);
            Grid.SetColumn(friendName, 1);

            var acceptRequest = CreateGridButton("accept");
            var rejectRequest = CreateGridButton("reject");
            Grid.SetColumn(acceptRequest, 2);
            Grid.SetColumn(rejectRequest, 3);

            requestGrid.Children.Add(avatar);
            requestGrid.Children.Add(friendName);
            requestGrid.Children.Add(acceptRequest);
            requestGrid.Children.Add(rejectRequest);
            requestsStack.Children.Add(requestGrid);
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
                searchPlayer = await profileManager.GetPlayerByUsernameAsync(friendUsername, false);
            }
            catch (FaultException<DBFault> dbFault)
            {
                HandlePullDBFault(dbFault);
                NavigationService?.Navigate(new FriendsPage());
            }

            if (searchPlayer.PlayerId != -1)
            {
                var requestStatus = false;
                try
                {
                    requestStatus = await friendsClient.SendFriendRequestAsync(currentLoginUsername, searchPlayer.PlayerUsername);
                }
                catch (FaultException<DBFault> dbFault)
                {
                    HandlePushDBFault(dbFault);
                    NavigationService?.Navigate(new FriendsPage());
                }

                if (requestStatus)
                {
                    string title = Properties.Langs.Resources.friend_request_sent_title;
                    string message = Properties.Langs.Resources.friend_request_sent_message + friendUsername;
                    var notificationWindow = new NotificationWindow(title, message)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    notificationWindow.ShowDialog();
                }
                else
                {
                    string title = Properties.Langs.Resources.error;
                    string message = Properties.Langs.Resources.friend_request_not_sent;
                    var notificationWindow = new NotificationWindow(title, message)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    notificationWindow.ShowDialog();
                }
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string friendUsername = searchtxtBx.Text.Trim();
            await SendFriendRequest(friendUsername);
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