using Forbbiden.Client.Exceptions;
using Forbbiden.Client.Model;
using Forbbiden.Client.Logic.Validations;
using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.Repositories;
using Forbbiden.Client.View.info;
using log4net;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Forbbiden.Client.Logic;

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

            LoadAvatar(player);
        }

        private async void LoadAvatar(Player player)
        {
            try
            {
                if (player?.PlayerAvatarBytes != null && player.PlayerAvatarBytes.Length > 0)
                {
                    var brush = GetImageBrushFromBytes(player.PlayerAvatarBytes);
                    if (brush != null)
                    {
                        imgAvatar.Fill = brush;
                        return;
                    }
                }

                var fetched = await AvatarsManager.Instance.GetAvatarBrushAsync(player.PlayerUsername);
                imgAvatar.Fill = fetched ?? AvatarsManager.Instance.GetAvatarBrushAsync("").Result;
            }
            catch (Exception ex)
            {
                Log.Warn("No se pudo cargar avatar", ex);
            }
        }

        private static ImageBrush GetImageBrushFromBytes(byte[] bytes)
        {
            try
            {
                using (var ms = new MemoryStream(bytes))
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = ms;
                    bmp.EndInit();
                    bmp.Freeze();
                    return new ImageBrush(bmp);
                }
            }
            catch { return null; }
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
                    ExceptionViewManager.ShowUsernameValidationErrors(
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
                    ExceptionViewManager.ShowEmailValidationErrors(
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
                updatedPlayer.PlayerAvatarBytes = ProfilePlayer.PlayerAvatarBytes;
                updatedPlayer.PlayerAvatarName = ProfilePlayer.PlayerAvatarName;
            }

            await SaveProfileChanges(updatedPlayer);
        }

        private async Task UploadAvatar(Player updatedPlayer)
        {
            if (string.IsNullOrEmpty(UploadedAvatarOriginalPath) || string.IsNullOrEmpty(AvatarFileName))
            {
                return;
            }

            try
            {
                var bytes = GetAvatarBytesResized(UploadedAvatarOriginalPath, 256, 80);
                if (bytes == null || bytes.Length == 0)
                {
                    return;
                }

                bool saved = false;
                try
                {
                    saved = await ProfileRepo.UploadAvatar(
                        ProfilePlayer.PlayerUsername, bytes, AvatarFileName);
                }
                catch (ViewException ex)
                {
                    ExceptionViewManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
                }

                if (!saved)
                {
                    return;
                }

                updatedPlayer.PlayerAvatarBytes = bytes;
                updatedPlayer.PlayerAvatarName = AvatarFileName;
                AvatarsManager.Instance.UpdateCache(ProfilePlayer.PlayerUsername, bytes);
            }
            catch (Exception ex)
            {
                Log.Error("ProfilePage.UploadAvatar", ex);
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
                    ExceptionViewManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
                }

                if (refreshed != null && refreshed.PlayerId > 0)
                {
                    AvatarsManager.Instance.UpdateCache(refreshed.PlayerUsername, refreshed.PlayerAvatarBytes);
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

    }
}