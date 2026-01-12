using Forbbiden.Client.Logic;
using Forbbiden.Client.ProfileManager;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Forbbiden.Client.View
{
    /// <summary>
    /// Interaction logic for FriendProfilePage.xaml
    /// </summary>
    public partial class FriendProfilePage : Page
    {
        public FriendProfilePage()
        {
            InitializeComponent();
        }

        public FriendProfilePage(Player player)
        {
            InitializeComponent();

            txtBkUsername.Text = player.PlayerUsername;
            txtBkEmail.Text = player.PlayerEmail;
            txtBkName.Text = player.PlayerName;
            txtBkAvatarName.Text = player.PlayerUsername;

            var socialMedia = player.SocialMedia;
            if (socialMedia != null)
            {
                foreach (var sm in socialMedia)
                {
                    switch (sm.SocialMediaName)
                    {
                        case "discord":
                            txtBkDiscord.Text = sm.SocialLink;
                            break;
                        case "x":
                            txtBkX.Text = sm.SocialLink;
                            break;
                        case "instagram":
                            txtBkInstagram.Text = sm.SocialLink;
                            break;
                        case "facebook":
                            txtBkFacebook.Text = sm.SocialLink;
                            break;
                        default:
                            MessageBox.Show("Red social desconocida: " + sm.SocialMediaName);
                            break;
                    }
                }
            }

            SetAvatar(player);
        }

        private async void SetAvatar(Player player)
        {
            if (player?.PlayerAvatarBytes != null && player.PlayerAvatarBytes.Length > 0)
            {
                var brush = ViewUtils.GetImageBrushFromBytes(player.PlayerAvatarBytes);
                if (brush != null)
                {
                    imgAvatar.Fill = brush;
                    return;
                }
            }

            var fetched = await AvatarsManager.Instance.GetAvatarBrushAsync(player?.PlayerUsername);
            if (fetched != null)
            {
                imgAvatar.Fill = fetched;
                return;
            }

            if (!string.IsNullOrWhiteSpace(player?.PlayerAvatarName))
            {
                try
                {
                    string projectDir = Directory.GetParent(
                        AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
                    string avatarPath = System.IO.Path.Combine(projectDir, "avatars", player.PlayerAvatarName);

                    if (File.Exists(avatarPath))
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.UriSource = new Uri(avatarPath, UriKind.Absolute);
                        bmp.EndInit();
                        imgAvatar.Fill = new ImageBrush(bmp);
                        return;
                    }
                }
                catch { }
            }
        }

        private void AcceptBtn_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
                NavigationService?.Navigate(new FriendsPage());
        }
    }
}