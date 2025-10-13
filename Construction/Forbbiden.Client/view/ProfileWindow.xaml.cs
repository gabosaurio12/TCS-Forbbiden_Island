using Forbbiden.Client.ProfileManager;
using log4net;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace Forbbiden.Client
{
    /// <summary>
    /// Interaction logic for ProfileWindow.xaml
    /// </summary>
    public partial class ProfileWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LoginPage));

        private Player _player;
        private string _uploadedAvatarOriginalPath;
        private string _uploadedAvatarProjectPath;
        private bool avatarChanged = false;
        public ProfileWindow()
        {
            InitializeComponent();
        }

        public ProfileWindow(Player player)
        {
            InitializeComponent();
            _player = player;
            txtBxUsername.Text = player.PlayerUsername;
            txtBxEmail.Text = player.PlayerEmail;
            txtBxName.Text = player.PlayerName;
            txtBkAvatarName.Text = player.PlayerUsername;

            var socialMedia = player.SocialMedia;
            if (socialMedia != null)
            {
                foreach(var sm in socialMedia)
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
            Close();
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
                if (!client.IsEmailAvailable(email))
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
                new SocialMedia { SocialMediaName = "discord", SocialLink = txtBxDiscord.Text, PlayerId = _player.PlayerId },
                new SocialMedia { SocialMediaName = "x", SocialLink = txtBxX.Text, PlayerId = _player.PlayerId },
                new SocialMedia { SocialMediaName = "instagram", SocialLink = txtBxInstagram.Text, PlayerId = _player.PlayerId },
                new SocialMedia { SocialMediaName = "facebook", SocialLink = txtBxFacebook.Text, PlayerId = _player.PlayerId }
            };

            if (avatarChanged) player.PlayerAvatarPath = _uploadedAvatarProjectPath;

            bool isValid = true;

            if (player.PlayerUsername != _player.PlayerUsername)
            {
                ValidateUsername(player.PlayerUsername, ref isValid);
            }

            if (player.PlayerEmail != _player.PlayerEmail)
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
                if (avatarChanged) File.Copy(_uploadedAvatarOriginalPath, _uploadedAvatarProjectPath, true);
                
                updatedPlayer.PlayerId = _player.PlayerId;
                
                if (client.UpdatePlayer(updatedPlayer))
                {
                    Close();
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
                _uploadedAvatarOriginalPath = openFileDialog.FileName;

                Console.WriteLine("Uploaded avatar path: " + _uploadedAvatarOriginalPath);

                string projectPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\.."));
                string avatarsPath = Path.Combine(projectPath, "avatars");
                string destinyPath = Path.Combine(avatarsPath, Path.GetFileName(_uploadedAvatarOriginalPath));

                Console.WriteLine("Project path: " + projectPath);
                Console.WriteLine("Avatars path: " + avatarsPath);
                Console.WriteLine("FileName: " + Path.GetFileName(_uploadedAvatarOriginalPath));
                Console.WriteLine("Destiny avatar path: " + destinyPath);

                _uploadedAvatarProjectPath = destinyPath;
                
                imgAvatar.Fill = new ImageBrush(new System.Windows.Media.Imaging.BitmapImage(new Uri(_uploadedAvatarOriginalPath)));
                avatarChanged = true;
            }
        }
    }
}
