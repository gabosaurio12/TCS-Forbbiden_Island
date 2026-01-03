using Forbbiden.Client.logic;
using Forbbiden.Client.ProfileManager;
using log4net;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Forbbiden.Client
{
    /// <summary>
    /// ProfilePage.xaml interaction logic
    /// </summary>
    public partial class ProfilePage : Page
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProfilePage));
        private readonly ProfileManagerClient ProfileClient;

        private readonly Player ProfilePlayer;
        private string UploadedAvatarOriginalPath;
        private string AvatarFileName;
        private bool AvatarChanged = false;

        public ProfilePage()
        {
            InitializeComponent();

            ProfileClient = new ProfileManagerClient();
        }

        public ProfilePage(Player player)
        {
            InitializeComponent();

            ProfileClient = new ProfileManagerClient();

            this.ProfilePlayer = player;
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

            if (!string.IsNullOrEmpty(player.PlayerAvatarPath))
            {
                string avatarPath = ResolveLocalAvatarPath(player.PlayerAvatarPath);
                if (!string.IsNullOrEmpty(avatarPath) && File.Exists(avatarPath))
                {
                    try
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.UriSource = new Uri(avatarPath, UriKind.Absolute);
                        bmp.EndInit();
                        imgAvatar.Fill = new ImageBrush(bmp);
                    }
                    catch (Exception ex)
                    {
                        Log.Warn("No se pudo cargar avatar local: " + avatarPath, ex);
                    }
                }
            }
        }

        private void BtnDiscard_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
                NavigationService?.Navigate(new MainPage());
        }

        private static void ResetFieldColors(TextBlock txtBkUsername, TextBlock txtBkName, TextBlock txtBkEmail)
        {
            txtBkUsername.Foreground = Brushes.Black;
            txtBkName.Foreground = Brushes.Black;
            txtBkEmail.Foreground = Brushes.Black;
        }

        private void ValidateUsername(string username, ref bool isValid)
        {
            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("El nombre de usuario no puede estar vacío.");
                txtBkUsername.Foreground = Brushes.Red;
                isValid = false;
            }
            else
            {
                try
                {
                    if (!ProfileClient.IsUsernameAvailable(username))
                    {
                        MessageBox.Show("El nombre de usuario ya existe.");
                        txtBkUsername.Foreground = Brushes.Red;
                        isValid = false;
                    }
                }
                catch (Exception ex)
                {
                    Log.Warn("ValidateUsername error", ex);
                    MessageBox.Show("No se pudo verificar el nombre de usuario.");
                    isValid = false;
                }
            }
        }

        private void ValidateEmail(string email, ref bool isValid)
        {
            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("El correo no puede estar vació.");
                txtBkEmail.Foreground = Brushes.Red;
                isValid = false;
            }
            else
            {
                try
                {
                    if (!ProfileClient.ValidateEmail(email))
                    {
                        MessageBox.Show("El correo electrónico debe contener un @ o ya está registrado.");
                        txtBkEmail.Foreground = Brushes.Red;
                        isValid = false;
                    }
                }
                catch (Exception ex)
                {
                    Log.Warn("ValidateEmail error", ex);
                    MessageBox.Show("No se pudo validar el correo.");
                    isValid = false;
                }
            }
        }

        private void SetPlayer(ref Player player)
        {
            player.PlayerUsername = txtBxUsername.Text;
            player.PlayerEmail = txtBxEmail.Text;
            player.PlayerName = txtBxName.Text;
            player.Status = ProfilePlayer.Status;
            player.IsVerified = ProfilePlayer.IsVerified;

            player.SocialMedia = new[]
            {
                new SocialMedia { SocialMediaName = "discord", SocialLink = txtBxDiscord.Text, PlayerId = this.ProfilePlayer.PlayerId },
                new SocialMedia { SocialMediaName = "x", SocialLink = txtBxX.Text, PlayerId = this.ProfilePlayer.PlayerId },
                new SocialMedia { SocialMediaName = "instagram", SocialLink = txtBxInstagram.Text, PlayerId = this.ProfilePlayer.PlayerId },
                new SocialMedia { SocialMediaName = "facebook", SocialLink = txtBxFacebook.Text, PlayerId = this.ProfilePlayer.PlayerId }
            };

            if (AvatarChanged)
            {
                player.PlayerAvatarPath = AvatarFileName;
                string exeDir = AppContext.BaseDirectory;
                string projectDir = Directory.GetParent(exeDir).Parent.Parent.FullName;

                UploadedAvatarProjectPath = Path.Combine(projectDir, "avatars", AvatarFileName);
                File.Copy(UploadedAvatarOriginalPath, UploadedAvatarProjectPath, true);
            }
            else
            {
                player.PlayerAvatarPath = this.ProfilePlayer.PlayerAvatarPath;
            }            
        }

        private bool IsPlayerValid(Player player)
        {
            bool isValid = true;

            if (player.PlayerUsername != this.ProfilePlayer.PlayerUsername)
            {
                ValidateUsername(player.PlayerUsername, ref isValid);
            }

            if (player.PlayerEmail != this.ProfilePlayer.PlayerEmail)
            {
                ValidateEmail(player.PlayerEmail, ref isValid);
            }

            return isValid;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            ResetFieldColors(txtBkUsername, txtBkName, txtBkEmail);

            Player updatedPlayer = new Player
            {
                PlayerId = ProfilePlayer.PlayerId
            };

            SetPlayer(ref updatedPlayer);

            if (IsPlayerValid(updatedPlayer))
            {
                try
                {
                    if (ProfileClient.UpdatePlayer(updatedPlayer))
                    {
                        NavigationService?.Navigate(new MainPage());
                    }
                    else
                    {
                        MessageBox.Show("Error al actualizar el perfil.");
                    }
                }
                catch (FaultException<DBFault> ex)
                {
                    string classMethod = "ProfilePage.BtnSave_Click";
                    Log.Error(classMethod, ex);
                    ViewUtils.ShowPushError(Window.GetWindow(this));
                }
                
            }
        }

        private void BtnUploadAvatar_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Filter = "Image files (*.png;*.jpg;*.jpeg)| *.png;*.jpg;*.jpeg"
            };
            var result = openFileDialog.ShowDialog();

            if (result == true)
            {
                AvatarFileName = Path.GetFileName(openFileDialog.FileName);
                UploadedAvatarOriginalPath = Path.GetFullPath(openFileDialog.FileName);

                imgAvatar.Fill = ViewUtils.GetImageBrush(UploadedAvatarOriginalPath);

                AvatarChanged = true;
            }
        }

        private byte[] GetAvatarBytesResized(string filePath, int maxDimension = 256, int jpegQuality = 80)
        {
            try
            {
                if (!File.Exists(filePath)) return null;

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(filePath);
                bitmap.DecodePixelWidth = maxDimension;
                bitmap.DecodePixelHeight = maxDimension;
                bitmap.EndInit();
                bitmap.Freeze();

                var encoder = new JpegBitmapEncoder();
                encoder.QualityLevel = jpegQuality;
                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                using (var ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                Log.Warn("GetAvatarBytesResized failed", ex);
                return null;
            }
        }

        private string ResolveLocalAvatarPath(string avatarPathOrFileName)
        {
            try
            {
                if (Path.IsPathRooted(avatarPathOrFileName) && File.Exists(avatarPathOrFileName))
                    return avatarPathOrFileName;

                string projectDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
                var candidate = Path.Combine(projectDir, "avatars", avatarPathOrFileName);
                if (File.Exists(candidate)) return candidate;

                if (File.Exists(avatarPathOrFileName)) return avatarPathOrFileName;

                return null;
            }
            catch { return null; }
        }
    }
}