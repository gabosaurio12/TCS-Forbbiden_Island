using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Forbbiden.Client.ProfileManager;

namespace Forbbiden.Client.view
{
    /// <summary>
    /// Interaction logic for FriendsPage.xaml
    /// </summary>
    public partial class FriendsPage : Page
    {
        public FriendsPage()
        {
            InitializeComponent();
            SetFriends();
        }

        private void SetFriends()
        {
            var profileManager = new ProfileManagerClient();
            var player = profileManager.GetCurrentLogin();
            if (player != null)
            {
                foreach (var friend in player.Friends)
                {
                    AddOnlineFriend(friend);
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
            NavigationService?.Navigate(new HostGameControl());
        }

        private void SearchFriend_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (searchtxtBx.Width == 0)
            {
                Storyboard storyBoard = (Storyboard)FindResource("ShowSearchBar");
                storyBoard.Begin();
            }
            else
            {
                Storyboard storyBoard = (Storyboard)FindResource("HideSearchBar");
                storyBoard.Begin();
            }
        }
    }
}
