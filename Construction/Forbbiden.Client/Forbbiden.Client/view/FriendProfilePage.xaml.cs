using Forbbiden.Client.ProfileManager;
using log4net;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Forbbiden.Client.view
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

            if (player.PlayerAvatarPath != null)
            {
                string projectDir = Directory.GetParent(
                    AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
                string avatarPath = System.IO.Path.Combine(projectDir, "avatars", player.PlayerAvatarPath);

                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = new Uri(avatarPath, UriKind.Absolute);
                bmp.EndInit();
                imgAvatar.Fill = new ImageBrush(bmp);
            }
        }

        private void AcceptBtn_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
                NavigationService?.Navigate(new FriendsPage());
        }
    }
}
