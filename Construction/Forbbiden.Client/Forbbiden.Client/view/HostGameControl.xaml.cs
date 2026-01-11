using Forbbiden.Client.GameManager;
using Forbbiden.Client.Logic;
using Forbbiden.Client.MatchManager;
using Forbbiden.Client.View.info;
using Forbbiden.Client.View;
using log4net;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Forbbiden.Client.Model;

namespace Forbbiden.Client
{
    public partial class HostGameControl : UserControl
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HostGameControl));
        private string selectedDifficulty = "Normal";
        private string selectedVisibility = "Public";
        private int selectedCapacity = 4;

        public HostGameControl()
        {
            InitializeComponent();
            this.Loaded += HostGameControl_Loaded;
        }

        private void HostGameControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                selectedDifficulty = "Normal";
                selectedVisibility = "Public";
                selectedCapacity = 4;

                if (PlayerComboBox != null) PlayerComboBox.SelectedIndex = 2; 

                if (NormalButton != null) NormalButton.Background = Brushes.LightGreen;
                if (HardButton != null) HardButton.Background = Brushes.LightGray;
                if (NormalMessage != null) NormalMessage.Visibility = Visibility.Visible;
                if (HardMessage != null) HardMessage.Visibility = Visibility.Collapsed;

                if (PublicToggle != null) PublicToggle.IsChecked = true;
                UpdateVisibilityText(true);
            }
            catch (Exception ex)
            {
                Log.Warn("Error inicializando HostGameControl", ex);
            }
        }

        private void UpdateVisibilityText(bool isPublic)
        {
            if (PublicMessage != null)
            {
                PublicMessage.Visibility = isPublic ? Visibility.Visible : Visibility.Collapsed;
            }
            if (PrivateMessage != null)
            {
                PrivateMessage.Visibility = isPublic ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void PlayerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlayerComboBox?.SelectedItem is ComboBoxItem item
                && int.TryParse(item.Content?.ToString(), out int val))
            {
                selectedCapacity = Math.Max(2, Math.Min(4, val));
            }
        }

        private void NormalButton_Click(object sender, RoutedEventArgs e)
        {
            selectedDifficulty = "Normal";
            if (NormalButton != null) NormalButton.Background = Brushes.LightGreen;
            if (HardButton != null) HardButton.Background = Brushes.LightGray;
            if (NormalMessage != null) NormalMessage.Visibility = Visibility.Visible;
            if (HardMessage != null) HardMessage.Visibility = Visibility.Collapsed;
        }

        private void HardButton_Click(object sender, RoutedEventArgs e)
        {
            selectedDifficulty = "Hard";
            if (HardButton != null) HardButton.Background = Brushes.LightCoral;
            if (NormalButton != null) NormalButton.Background = Brushes.LightGray;
            if (HardMessage != null) HardMessage.Visibility = Visibility.Visible;
            if (NormalMessage != null) NormalMessage.Visibility = Visibility.Collapsed;
        }

        private void PublicToggle_Click(object sender, RoutedEventArgs e)
        {
            bool isPublic = (bool)PublicToggle.IsChecked;
            selectedVisibility = isPublic ? "Public" : "Private";
            UpdateVisibilityText(isPublic);

            PublicToggle.Background = isPublic ? Brushes.LightBlue : Brushes.LightCoral;
        }

        private CreateMatchRequest GetMatchRequest(MatchManagerClient matchClient)
        {
            var currentPlayer = ClientSession.GetPlayer();
            if (currentPlayer == null)
            {
                //ShowWarningNotification(Properties.Resources.warning_login_permission);
                return null;
            }

            string username = currentPlayer.PlayerUsername;

            var roomName = txtRoomName?.Text?.Trim();
            if (!string.IsNullOrEmpty(roomName) && roomName.Length > 20)
            {
                //ShowWarningNotification(Properties.Resources.warning_room_char_limit);
                return null;
            }

            var request = new CreateMatchRequest
            {
                HostUsername = username,
                Difficulty = selectedDifficulty,
                Visibility = selectedVisibility,
                MatchName = string.IsNullOrEmpty(roomName) ? null : roomName,
                Capacity = selectedCapacity
            };

            return request;
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var matchClient = new MatchManagerClient();

            try
            {
                var request = GetMatchRequest(matchClient);
                if (request == null)
                {
                    MessageBox.Show("Debes iniciar sesión antes de crear una partida.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string username = ClientSession.Username;
                var roomName = txtRoomName?.Text?.Trim();

                if (string.IsNullOrWhiteSpace(roomName))
                {
                    var wnd = new NotificationWindow(
                        Properties.Resources.missing_room_name_title,
                        Properties.Resources.missing_room_name_message);
                    wnd.Owner = Window.GetWindow(this);
                    wnd.ShowDialog();
                    return;
                }

                if (roomName.Length > 20)
                {
                    MessageBox.Show("El nombre de la sala debe tener como máximo 20 caracteres.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                /*var request = new CreateMatchRequest
                {
                    HostUsername = username,
                    Difficulty = selectedDifficulty,
                    Visibility = selectedVisibility,
                    MatchName = roomName,
                    Capacity = selectedCapacity
                };*/

                int matchId;
                try
                {
                    matchId = await Task.Run(() => matchClient.CreateMatch(request));
                }
                catch (Exception ex)
                {
                    Log.Error("CreateMatch error", ex);
                    MessageBox.Show("No se pudo crear la partida: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (matchId <= 0)
                {
                    MessageBox.Show("No se pudo crear la partida.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                string avatarFileName = null;
                var currentPlayer = ClientSession.GetPlayer();
                    
                var avatarPath = currentPlayer?.PlayerAvatarPath;
                if (!string.IsNullOrEmpty(avatarPath))
                {
                    avatarFileName = System.IO.Path.GetFileName(avatarPath);
                }

                var callback = new GameServiceCallback();
                var context = new InstanceContext(callback);
                var gameClient = new GameManagerClient(context);

                bool joined = await gameClient.JoinGameAsync(matchId.ToString(), username, null, avatarFileName);

                if (!joined)
                {
                    string title = Properties.Resources.error;
                    //string message = Properties.Resources.error_match_created_not_joined;
                    //ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
                    return;
                }

                var lobbyPage = new LobbyPage(matchId, username, gameClient, callback);
                NavigationService.GetNavigationService(this)?.Navigate(lobbyPage);
            }
            catch (TimeoutException)
            {
                string title = Properties.Resources.error;
                //string message = Properties.Resources.error_server_timeout;
                //ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
            }
            catch (Exception ex)
            {
                Log.Error("HostGameControl.PlayButton_Click", ex);
                string title = Properties.Resources.error;
                string message = Properties.Resources.unexpected_error;
                ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
            }
            finally
            {
                JoinGameControl.CloseMatchClient(matchClient);
            }
        }

        private void PublicToggle_Checked(object sender, RoutedEventArgs e)
        {
        }
    }
}