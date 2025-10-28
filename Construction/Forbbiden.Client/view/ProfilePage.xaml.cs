using Forbbiden.Client.ProfileManager;
using log4net;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Forbbiden.Client
{
    /// <summary>
    /// ProfilePage.xaml interaction logic
    /// </summary>
    public partial class ProfilePage : Page
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ProfilePage));

        private Player player;
        private string uploadedAvatarOriginalPath;
        private string uploadedAvatarProjectPath;
        private bool avatarChanged = false;

        public ProfilePage()
        {
            InitializeComponent();
        }

        public ProfilePage(Player player)
        {
            InitializeComponent();
            this.player = player;
            txtBxUsername.Text = player.PlayerUsername;
            txtBxEmail.Text = player.PlayerEmail;
            txtBxName.Text = player.PlayerName;
            txtBkAvatarName.Text = player.PlayerUsername;

            var socialMedia = player.SocialMedia;
            if (socialMedia != null)
            {
                foreach (var sm in socialMedia)
                {
                    switch (sm.SocialMediaName)
                    {
                        case "discord":
                            txtBxDiscord.Text = sm.SocialLink;
                            break;
                        case "x":
                            txtBxX.Text = sm.SocialLink;
                            break;
                        case "instagram":
                            txtBxInstagram.Text = sm.SocialLink;
                            break;
                        case "facebook":
                            txtBxFacebook.Text = sm.SocialLink;
                            break;
                        default:
                            MessageBox.Show("Red social desconocida: " + sm.SocialMediaName);
                            break;
                    }
                }
            }

            if (player.PlayerAvatarPath != null)
            {
                ImageBrush avatar = new ImageBrush(new System.Windows.Media.Imaging.BitmapImage(new Uri(player.PlayerAvatarPath)));
                imgAvatar.Fill = avatar;
            }
        }

        private void BtnDiscard_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        private void ResetFieldColors()
        {
            txtBkUsername.Foreground = Brushes.Black;
            txtBkName.Foreground = Brushes.Black;
            txtBkEmail.Foreground = Brushes.Black;
        }

        private void ValidateUsername(string username, ref bool isValid)
        {
            var client = new ProfileManagerClient();

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("El nombre de usuario no puede estar vacío.");
                txtBkUsername.Foreground = Brushes.Red;
                isValid = false;
            }
            else
            {
                if (!client.IsUsernameAvailable(username))
                {
                    MessageBox.Show("El nombre de usuario ya existe.");
                    txtBkUsername.Foreground = Brushes.Red;
                    isValid = false;
                }
            }
        }

        private void ValidateEmail(string email, ref bool isValid)
        {
            var client = new ProfileManagerClient();

            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("El correo no puede estar vació.");
                txtBkEmail.Foreground = Brushes.Red;
                isValid = false;
            }
            else
            {
                if (!client.ValidateEmail(email))
                {
                    MessageBox.Show("El correo electrónico debe contener un @ o ya está registrado.");
                    txtBkEmail.Foreground = Brushes.Red;
                    isValid = false;
                }
            }
        }

        private bool SetPlayer(ref Player player)
        {
            player.PlayerUsername = txtBxUsername.Text;
            player.PlayerEmail = txtBxEmail.Text;
            player.PlayerName = txtBxName.Text;

            player.SocialMedia = new SocialMedia[]
            {
                new SocialMedia { SocialMediaName = "discord", SocialLink = txtBxDiscord.Text, PlayerId = this.player.PlayerId },
                new SocialMedia { SocialMediaName = "x", SocialLink = txtBxX.Text, PlayerId = this.player.PlayerId },
                new SocialMedia { SocialMediaName = "instagram", SocialLink = txtBxInstagram.Text, PlayerId = this.player.PlayerId },
                new SocialMedia { SocialMediaName = "facebook", SocialLink = txtBxFacebook.Text, PlayerId = this.player.PlayerId }
            };

            if (avatarChanged) player.PlayerAvatarPath = uploadedAvatarProjectPath;

            bool isValid = true;

            if (player.PlayerUsername != this.player.PlayerUsername)
            {
                ValidateUsername(player.PlayerUsername, ref isValid);
            }

            if (player.PlayerEmail != this.player.PlayerEmail)
            {
                ValidateEmail(player.PlayerEmail, ref isValid);
            }

            return isValid;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            ResetFieldColors();
            var client = new ProfileManagerClient();

            Player updatedPlayer = new Player();

            if (SetPlayer(ref updatedPlayer))
            {
                if (avatarChanged) File.Copy(uploadedAvatarOriginalPath, uploadedAvatarProjectPath, true);

                updatedPlayer.PlayerId = player.PlayerId;

                if (client.UpdatePlayer(updatedPlayer))
                {
                    if (NavigationService != null && NavigationService.CanGoBack)
                        NavigationService.GoBack();
                }
                else
                {
                    MessageBox.Show("Error al actualizar el perfil.");
                }
            }
        }

        private void BtnUploadAvatar_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Filter = "Image files (*.png;*.jpg;*.jpeg)| *.png;*.jpg;*.jpeg|All files (*.*)|*.*"
            };
            var result = openFileDialog.ShowDialog();

            if (result == true)
            {
                uploadedAvatarOriginalPath = openFileDialog.FileName;

                string projectPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\.."));
                string avatarsPath = Path.Combine(projectPath, "avatars");
                string destinyPath = Path.Combine(avatarsPath, Path.GetFileName(uploadedAvatarOriginalPath));

                uploadedAvatarProjectPath = destinyPath;

                imgAvatar.Fill = new ImageBrush(new System.Windows.Media.Imaging.BitmapImage(new Uri(uploadedAvatarOriginalPath)));
                avatarChanged = true;
            }
        }
    }
}
