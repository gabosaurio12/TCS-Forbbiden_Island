using Forbbiden.Client.GameManager;
using Forbbiden.Client.logic;
using Forbbiden.Client.MatchManager;
using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.view.info;
using log4net;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

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
            if (PublicMessage != null) PublicMessage.Visibility = isPublic ? Visibility.Visible : Visibility.Collapsed;
            if (PrivateMessage != null) PrivateMessage.Visibility = isPublic ? Visibility.Collapsed : Visibility.Visible;
        }

        private void PlayerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlayerComboBox?.SelectedItem is ComboBoxItem item)
            {
                if (int.TryParse(item.Content?.ToString(), out int val))
                {
                    selectedCapacity = Math.Max(2, Math.Min(4, val));
                }
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
            bool isPublic = PublicToggle.IsChecked == true;
            selectedVisibility = isPublic ? "Public" : "Private";
            UpdateVisibilityText(isPublic);

            PublicToggle.Background = isPublic ? Brushes.LightBlue : Brushes.LightCoral;
        }
        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var matchClient = new MatchManagerClient();

            try
            {
                var currentPlayer = ClientSession.GetPlayer();
                if (currentPlayer == null)
                {
                    MessageBox.Show("Debes iniciar sesión antes de crear una partida.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string username = currentPlayer.PlayerUsername;
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

                var request = new CreateMatchRequest
                {
                    HostUsername = username,
                    Difficulty = selectedDifficulty,
                    Visibility = selectedVisibility,
                    MatchName = roomName,
                    Capacity = selectedCapacity
                };

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

                // Se sigue generando el código en el servidor, pero ya no se muestra aquí.

                string avatarFileName = null;
                try
                {
                    var avatarPath = currentPlayer?.PlayerAvatarPath;
                    if (!string.IsNullOrEmpty(avatarPath))
                        avatarFileName = System.IO.Path.GetFileName(avatarPath);
                }
                catch { /* no crítico */ }

                var callback = new GameServiceCallback();
                var context = new InstanceContext(callback);
                var gameClient = new GameManagerClient(context);

                bool joined = false;
                try
                {
                    joined = await gameClient.JoinGameAsync(matchId.ToString(), username, null, avatarFileName);
                }
                catch (Exception ex)
                {
                    Log.Error("JoinGame error", ex);
                    MessageBox.Show("No se pudo unir al lobby: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if (!joined)
                {
                    MessageBox.Show("La partida se creó, pero no se pudo unir al lobby.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var lobbyPage = new Forbbiden.Client.view.LobbyPage(matchId, username, gameClient, callback);
                NavigationService.GetNavigationService(this)?.Navigate(lobbyPage);
            }
            catch (TimeoutException)
            {
                MessageBox.Show("El servidor tardó demasiado en responder.", "Timeout", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                try { matchClient.Close(); } catch { matchClient.Abort(); }
            }
        }

        private void PublicToggle_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}