using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

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
            AddOnlineFriend();
            AddOnlineFriend();
            AddOfflineFriend();
        }

        public void AddOnlineFriend()
        {
            StackPanel friend = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(40,0,40,0),
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
                Margin = new Thickness(20,20,0,20)
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
            onlineStack.Children.Add(friend);
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
    }
}
