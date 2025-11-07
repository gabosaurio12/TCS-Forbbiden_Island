using Forbbiden.Client.FriendsManager;
using Forbbiden.Client.logic;
using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.view.info;
using Forbbiden.Contracts;
using System;
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

        private CallbacksManager callback;
        private string currentLoginUsername;

        public FriendRequestsPage()
        {
            InitializeComponent();

            callback = new CallbacksManager();
            callback.FriendRequestReceived += OnFriendRequestReceived;
            var client = new FriendsManagerClient();

            SetRequests();
        }

        private void OnFriendRequestReceived(FriendRequest request)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _ = AddRequest(request);
            });
        }

        private void SetRequests()
        {
            var profileManager = new ProfileManagerClient();
            var player = profileManager.GetCurrentLogin();
            if (player.PlayerId != -1)
            {
                currentLoginUsername = player.PlayerUsername;

                var friendsClient = new FriendsManagerClient();
                var requests = friendsClient.getFriendRequests(currentLoginUsername);
                if (requests.Length > 0)
                {
                    foreach(var request in requests)
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

        private string GettxtBkText(Grid stack)
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

        private void RemoveRequestStack(Grid stack)
        {
            requestsStack.Children.Remove(stack);
        }

        private void AcceptButton_Click(Object sender, RoutedEventArgs e)
        {
            var grid = (Grid)((Button)sender).Parent;
            string senderUsername = GettxtBkText(grid);

            if (!senderUsername.Equals(""))
            {
                var profileClient = new ProfileManagerClient();
                var receiver = profileClient.GetCurrentLogin();
                if (receiver.PlayerId != -1)
                {
                    var friendClient = new FriendsManagerClient();
                    var requestStatus = friendClient.AcceptFriendRequest(senderUsername, receiver.PlayerUsername);
                    if (!requestStatus)
                    {
                        OpenNotification(Properties.Langs.Resources.error,
                            Properties.Langs.Resources.push_database_error);
                    }
                    else
                    {
                        RemoveRequestStack(grid);
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
            Button button = new Button();
            var irishGoverFont = new FontFamily(new Uri("pack://application:,,,/"),
                "/Forbbiden.Client;component/Fonts/#Garet Heavy");
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
                    FontFamily = irishGoverFont,
                    Background = green,
                    Foreground = Brushes.White,
                    Cursor = Cursors.Hand
                };
                acceptRequest.Click += AcceptButton_Click;
                button = acceptRequest;
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
                    FontFamily = irishGoverFont,
                    Background = red,
                    Foreground = Brushes.White,
                    Cursor = Cursors.Hand
                };
                button = rejectRequest;
            }

            return button;
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
            var irishGoverFont = new FontFamily(new Uri("pack://application:,,,/"),
                "/Forbbiden.Client;component/Fonts/#Garet Heavy");

            TextBlock friendName = new TextBlock
            {
                Text = username,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(50, 0, 0, 0),
                FontSize = 48,
                FontFamily = irishGoverFont,
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

            var profileClient = new ProfileManagerClient();
            var friend = await profileClient.GetPlayerByIdAsync(request.SenderID);
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
            NavigationService?.GoBack();
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
            var profileClient = new ProfileManagerClient();
            var searchPlayer = await profileClient.GetPlayerByUsernameAsync(friendUsername);
            if (searchPlayer.PlayerId != -1)
            {
                var friendsClient = new FriendsManagerClient();
                var requestStatus = await friendsClient.SendFriendRequestAsync(currentLoginUsername, searchPlayer.PlayerUsername);

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
