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
        private ProfileManagerClient ProfileClient;

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
                        var bmp = ViewUtils.GetBitmapImage(player.PlayerAvatarPath);
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

        private bool SetPlayer(ref Player player)
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

            if (!SetPlayer(ref updatedPlayer))
                return;

            if (AvatarChanged)
            {
                if (string.IsNullOrEmpty(UploadedAvatarOriginalPath) || string.IsNullOrEmpty(AvatarFileName))
                {
                    MessageBox.Show("Avatar no válido."); // Fix ViewUtils.OpenNotification(title, message)
                    return;
                }

                try
                {

                    var bytes = GetAvatarBytesResized(UploadedAvatarOriginalPath, 256, 80);
                    if (bytes == null || bytes.Length == 0)
                    {
                        MessageBox.Show("No se pudo procesar la imagen."); // Fix ViewUtils.OpenNotification()
                        return;
                    }

                    string savedFileName = null;
                    try
                    {
                        savedFileName = ProfileClient.UploadAvatar(ProfilePlayer.PlayerUsername, bytes, AvatarFileName);
                    }
                    catch (FaultException fex)
                    {
                        Log.Error("UploadAvatar Fault", fex);
                        MessageBox.Show("Error al subir avatar: " + fex.Message); // Fix ViewUtils.OpenNotification()
                        return;
                    }
                    catch (Exception ex)
                    {
                        Log.Error("UploadAvatar error", ex);
                        MessageBox.Show("Error al subir avatar: " + ex.Message); // Fix ViewUtils.OpenNotification()
                        return;
                    }

                    if (string.IsNullOrEmpty(savedFileName))
                    {
                        MessageBox.Show("El servidor no devolvió un nombre para el avatar."); // Fix ViewUtils.OpenNotification()
                        return;
                    }

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
                catch (Exception ex)
                {
                    Log.Error("Error preparando o subiendo avatar", ex);
                    MessageBox.Show("Error al procesar el avatar: " + ex.Message); // Fix ViewUtils.OpenNotification()
                    return;
                }
            }
            else
            {
                updatedPlayer.PlayerAvatarPath = this.ProfilePlayer.PlayerAvatarPath;
            }

            try
            {
                if (ProfileClient.UpdatePlayer(updatedPlayer))
                {
                    try
                    {
                        var refreshed = ProfileClient.GetPlayerByUsername(updatedPlayer.PlayerUsername, true);
                        if (refreshed != null && refreshed.PlayerId > 0)
                        {
                            ClientSession.SetPlayer(refreshed); 
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warn("No se pudo refrescar la sesión después de UpdatePlayer", ex);
                    }

                    NavigationService?.Navigate(new MainPage());
                }
            }
            catch (FaultException fex)
            {
                Log.Error("UpdatePlayer Fault", fex);
                MessageBox.Show("Error al actualizar perfil: " + fex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("UpdatePlayer error", ex);
                MessageBox.Show("Error al actualizar perfil: " + ex.Message);
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
                return ViewUtils.GetDecodedPixelBitmapImage(filePath, maxDimension, jpegQuality);
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