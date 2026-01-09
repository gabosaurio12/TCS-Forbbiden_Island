using Forbbiden.Client.Exceptions;
using Forbbiden.Client.logic;
using Forbbiden.Client.Logic;
using Forbbiden.Client.Logic.Validations;
using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.Repositories;
using Forbbiden.Client.view.info;
using log4net;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
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
        private readonly ProfileRepository ProfileRepo;

        private readonly Player ProfilePlayer;
        private string UploadedAvatarOriginalPath;
        private string AvatarFileName;
        private string NewHashedPassword = null;
        private bool AvatarChanged = false;

        public ProfilePage()
        {
            InitializeComponent();

            ProfileRepo = new ProfileRepository();
        }

        public ProfilePage(Player player)
        {
            InitializeComponent();

            ProfileRepo = new ProfileRepository();

            ProfilePlayer = player;
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

            if (!string.IsNullOrWhiteSpace(player.PlayerAvatarPath))
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
            NewHashedPassword = null;
            NavigationService?.Navigate(new MainPage());
        }

        private static void ResetFieldColors(TextBlock txtBkUsername, TextBlock txtBkName, TextBlock txtBkEmail)
        {
            txtBkUsername.Foreground = Brushes.Black;
            txtBkName.Foreground = Brushes.Black;
            txtBkEmail.Foreground = Brushes.Black;
        }

        private SocialMedia[] GetSocialMediaArray()
        {
            return new[]
            {
                new SocialMedia
                {
                    SocialMediaName = "discord",
                    SocialLink = txtBxDiscord.Text,
                    PlayerId = ProfilePlayer.PlayerId
                },
                new SocialMedia
                {
                    SocialMediaName = "x",
                    SocialLink = txtBxX.Text,
                    PlayerId = ProfilePlayer.PlayerId
                },
                new SocialMedia
                {
                    SocialMediaName = "instagram",
                    SocialLink = txtBxInstagram.Text,
                    PlayerId = ProfilePlayer.PlayerId
                },
                new SocialMedia
                {
                    SocialMediaName = "facebook",
                    SocialLink = txtBxFacebook.Text,
                    PlayerId = ProfilePlayer.PlayerId
                }
            };
        }

        private bool SetPlayer(Player player)
        {
            player.PlayerUsername = txtBxUsername.Text;
            player.PlayerPassword = NewHashedPassword ?? ProfilePlayer.PlayerPassword;
            player.PlayerEmail = txtBxEmail.Text;
            player.PlayerName = txtBxName.Text;
            player.Status = ProfilePlayer.Status;
            player.IsVerified = ProfilePlayer.IsVerified;

            player.SocialMedia = GetSocialMediaArray();

            bool isValid = true;

            if (player.PlayerUsername != ProfilePlayer.PlayerUsername)
            {
                var usernameValidationResults = ValidationUtils.ValidateUsername(player.PlayerUsername);
                if (!usernameValidationResults.IsValid)
                {
                    txtBkUsername.Foreground = Brushes.Red;
                    ErrorsNotificationManager.ShowUsernameValidationErrors(
                        usernameValidationResults.Errors, Window.GetWindow(this));
                    isValid = false;
                }
            }

            if (player.PlayerEmail != ProfilePlayer.PlayerEmail)
            {
                var emailValidationResults = ValidationUtils.ValidateEmail(player.PlayerEmail);
                if (!emailValidationResults.IsValid)
                {
                    txtBkEmail.Foreground = Brushes.Red;
                    ErrorsNotificationManager.ShowEmailValidationErrors(
                        emailValidationResults.Errors, Window.GetWindow(this));
                    isValid = false;
                }
            }

            return isValid;
        }

        private async void BtnSave_ClickAsync(object sender, RoutedEventArgs e)
        {
            ResetFieldColors(txtBkUsername, txtBkName, txtBkEmail);

            Player updatedPlayer = new Player
            {
                PlayerId = ProfilePlayer.PlayerId
            };

            if (!SetPlayer(updatedPlayer))
            {
                return;
            }

            if (AvatarChanged)
            {
                await UploadAvatar(updatedPlayer);
            }
            else
            {
                updatedPlayer.PlayerAvatarPath = ProfilePlayer.PlayerAvatarPath;
            }

            await SaveProfileChanges(updatedPlayer);
        }

        private async Task UploadAvatar(Player updatedPlayer)
        {
            if (string.IsNullOrEmpty(UploadedAvatarOriginalPath) || string.IsNullOrEmpty(AvatarFileName))
            {
                //string message = Properties.Resources.error_invalid_avatar;
                //OpenNotificationError(message);
                return;
            }

            try
            {
                var bytes = GetAvatarBytesResized(UploadedAvatarOriginalPath, 256, 80);
                if (bytes == null || bytes.Length == 0)
                {
                    //string message = Properties.Resources.error_processing_image;
                    //OpenNotificationError(message);
                    return;
                }

                string savedFileName = null;
                try
                {
                    savedFileName = await ProfileRepo.UploadAvatar(
                        ProfilePlayer.PlayerUsername, bytes, AvatarFileName);
                }
                catch (ViewException ex)
                {
                    ErrorsNotificationManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
                }

                if (string.IsNullOrWhiteSpace(savedFileName))
                {
                    //OpenNotificationError(Properties.Resources.error_uploading_avatar);
                    return;
                }

                SaveAvatarCopy(savedFileName, bytes, updatedPlayer);
            }
            catch (Exception ex)
            {
                Log.Error("ProfilePage.UploadAvatar", ex);
                //OpenNotificationError(Properties.Resources.error_processing_avatar);
            }
        }

        private static void SaveAvatarCopy(string savedFileName, byte[] bytes, Player updatedPlayer)
        {
            try
            {
                string exeDir = AppContext.BaseDirectory;
                string projectDir = Directory.GetParent(exeDir).Parent.Parent.FullName;
                string localAvatarsDir = Path.Combine(projectDir, "avatars");
                if (!Directory.Exists(localAvatarsDir))
                {
                    Directory.CreateDirectory(localAvatarsDir);
                }

                string localPath = Path.Combine(localAvatarsDir, savedFileName);
                File.WriteAllBytes(localPath, bytes);
            }
            catch (Exception ex)
            {
                Log.Warn("No se pudo guardar copia local del avatar", ex);
            }
            finally
            {
                updatedPlayer.PlayerAvatarPath = savedFileName;
            }
        }

        private async Task SaveProfileChanges(Player updatedPlayer)
        {
            if (await ProfileRepo.UpdatePlayerProfile(updatedPlayer))
            {
                Player refreshed = null;
                try
                {
                    refreshed = await ProfileRepo
                        .GetPlayerByUsername(updatedPlayer.PlayerUsername, true);
                }
                catch (ViewException ex)
                {
                    ErrorsNotificationManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
                }

                if (refreshed != null && refreshed.PlayerId > 0)
                {
                    ClientSession.SetPlayer(refreshed);
                }

                NavigationService?.Navigate(new MainPage());
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

        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var changePasswordWindow = new ChangePasswordWindow()
            {
                Owner = Window.GetWindow(this)
            };

            if (changePasswordWindow.ShowDialog() == true)
            {
                NewHashedPassword = changePasswordWindow.HashedPassword;
            }
        }

        private static byte[] GetAvatarBytesResized(string filePath, int maxDimension = 256, int jpegQuality = 80)
        {
            try
            {
                return ViewUtils.GetDecodedPixelBitmapImage(filePath, maxDimension, jpegQuality);
            }
            catch (Exception ex)
            {
                Log.Warn("GetAvatarBytesResized failed", ex);
                return new byte[0];
            }
        }

        private static string ResolveLocalAvatarPath(string avatarPathOrFileName)
        {
            try
            {
                if (Path.IsPathRooted(avatarPathOrFileName) && File.Exists(avatarPathOrFileName))
                    return avatarPathOrFileName;

                string projectDir = ViewUtils.GetProjectDir();
                var candidate = Path.Combine(projectDir, "avatars", avatarPathOrFileName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                if (File.Exists(avatarPathOrFileName))
                {
                    return avatarPathOrFileName;
                }

                return null;
            }
            catch { return null; }
        }
    }
}