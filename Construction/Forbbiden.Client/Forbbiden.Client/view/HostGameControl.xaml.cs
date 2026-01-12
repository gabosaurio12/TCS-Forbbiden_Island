using Forbbiden.Client.Exceptions;
using Forbbiden.Client.Logic;
using Forbbiden.Client.MatchManager;
using Forbbiden.Client.Repositories;
using log4net;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Forbbiden.Client.Model;

namespace Forbbiden.Client.View
{
    public partial class HostGameControl : UserControl, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HostGameControl));

        private readonly MatchRepository matchRepository;
        private GameRepository gameRepository;
        private GameServiceCallback gameCallback;

        private string selectedDifficulty;
        private string selectedVisibility;
        private int selectedCapacity;

        public HostGameControl()
        {
            InitializeComponent();

            matchRepository = new MatchRepository();

            Loaded += HostGameControl_Loaded;
        }

        private void HostGameControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                selectedDifficulty = "Normal";
                selectedVisibility = "Public";
                selectedCapacity = 4;

                if (PlayerComboBox != null)
                {
                    PlayerComboBox.SelectedIndex = 2;
                }

                if (NormalButton != null)
                {
                    NormalButton.Background = Brushes.LightGreen;
                }

                if (HardButton != null)
                {
                    HardButton.Background = Brushes.LightGray;
                }

                if (NormalMessage != null)
                {
                    NormalMessage.Visibility = Visibility.Visible;
                }

                if (HardMessage != null)
                {
                    HardMessage.Visibility = Visibility.Collapsed;
                }

                if (PublicToggle != null)
                {
                    PublicToggle.IsChecked = true;
                }

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
                PublicMessage.Visibility = isPublic
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }

            if (PrivateMessage != null)
            {
                PrivateMessage.Visibility = isPublic
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }

        private void PlayerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlayerComboBox?.SelectedItem is ComboBoxItem item &&
                int.TryParse(item.Content?.ToString(), out int capacity))
            {
                selectedCapacity = Math.Max(2, Math.Min(4, capacity));
            }
        }

        private void NormalButton_Click(object sender, RoutedEventArgs e)
        {
            selectedDifficulty = "Normal";

            if (NormalButton != null)
            {
                NormalButton.Background = Brushes.LightGreen;
            }

            if (HardButton != null)
            {
                HardButton.Background = Brushes.LightGray;
            }

            if (NormalMessage != null)
            {
                NormalMessage.Visibility = Visibility.Visible;
            }

            if (HardMessage != null)
            {
                HardMessage.Visibility = Visibility.Collapsed;
            }
        }

        private void HardButton_Click(object sender, RoutedEventArgs e)
        {
            selectedDifficulty = "Hard";

            if (HardButton != null)
            {
                HardButton.Background = Brushes.LightCoral;
            }

            if (NormalButton != null)
            {
                NormalButton.Background = Brushes.LightGray;
            }

            if (HardMessage != null)
            {
                HardMessage.Visibility = Visibility.Visible;
            }

            if (NormalMessage != null)
            {
                NormalMessage.Visibility = Visibility.Collapsed;
            }
        }

        private void PublicToggle_Click(object sender, RoutedEventArgs e)
        {
            bool isPublic = PublicToggle.IsChecked == true;

            selectedVisibility = isPublic
                ? "Public"
                : "Private";

            UpdateVisibilityText(isPublic);

            PublicToggle.Background = isPublic
                ? Brushes.LightBlue
                : Brushes.LightCoral;
        }

        private CreateMatchRequest BuildMatchRequest()
        {
            var currentPlayer = ClientSession.GetPlayer();

            if (currentPlayer == null || currentPlayer.PlayerId <= 0)
            {
                ViewUtils.OpenNotificationWindow(
                    Properties.Resources.warning_title,
                    Properties.Resources.warning_login_permission,
                    Window.GetWindow(this));

                return null;
            }

            string roomName = txtRoomName?.Text?.Trim();

            if (string.IsNullOrWhiteSpace(roomName))
            {
                ViewUtils.OpenNotificationWindow(
                    Properties.Resources.missing_room_name_title,
                    Properties.Resources.missing_room_name_message,
                    Window.GetWindow(this));
                return null;
            }

            if (roomName.Length > 20)
            {
                ViewUtils.OpenNotificationWindow(
                    Properties.Resources.error,
                    Properties.Resources.warning_room_char_limit,
                    Window.GetWindow(this));
                return null;
            }

            return new CreateMatchRequest
            {
                HostUsername = currentPlayer.PlayerUsername,
                Difficulty = selectedDifficulty,
                Visibility = selectedVisibility,
                MatchName = roomName,
                Capacity = selectedCapacity
            };
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            CreateMatchRequest request = BuildMatchRequest();

            if (request == null)
            {
                return;
            }

            int matchId;

            try
            {
                matchId = await matchRepository.CreateMatch(request);
            }
            catch (ViewException ex)
            {
                ExceptionViewManager.ShowViewExceptionNotification(
                    ex, Window.GetWindow(this));
                return;
            }
            catch (Exception ex)
            {
                Log.Error("CreateMatch error", ex);

                ViewUtils.OpenNotificationWindow(
                    Properties.Resources.error,
                    Properties.Resources.unexpected_error,
                    Window.GetWindow(this));

                return;
            }

            if (matchId <= 0)
            {
                ViewUtils.OpenNotificationWindow(
                    Properties.Resources.error,
                    Properties.Resources.error_match_created_not_joined,
                    Window.GetWindow(this));

                return;
            }

            var currentPlayer = ClientSession.GetPlayer();
            string username = currentPlayer?.PlayerUsername ?? ClientSession.Username;
            string avatarFileName = currentPlayer?.PlayerAvatarName;
            byte[] avatarBytes = currentPlayer?.PlayerAvatarBytes;

            if ((avatarBytes == null || avatarBytes.Length == 0) && !string.IsNullOrWhiteSpace(username))
            {
                avatarBytes = await AvatarsManager.Instance.GetAvatarBytesAsync(username);
            }

            try
            {
                gameCallback = new GameServiceCallback();
                gameRepository = new GameRepository(gameCallback);

                bool joined = await gameRepository.JoinGame(
                    matchId.ToString(),
                    username,
                    avatarBytes,
                    avatarFileName);

                if (!joined)
                {
                    ViewUtils.OpenNotificationWindow(
                        Properties.Resources.error,
                        Properties.Resources.error_match_created_not_joined,
                        Window.GetWindow(this));

                    return;
                }

                var lobbyPage = new LobbyPage(
                    matchId,
                    username,
                    gameRepository.Client,
                    gameCallback);

                NavigationService.GetNavigationService(this)
                    ?.Navigate(lobbyPage);
            }
            catch (ViewException ex)
            {
                ExceptionViewManager.ShowViewExceptionNotification(
                    ex, Window.GetWindow(this));
            }
            catch (TimeoutException)
            {
                ViewUtils.OpenNotificationWindow(
                    Properties.Resources.error,
                    Properties.Resources.error_server_timeout,
                    Window.GetWindow(this));
            }
            catch (Exception ex)
            {
                Log.Error("HostGameControl.PlayButton_Click", ex);

                ViewUtils.OpenNotificationWindow(
                    Properties.Resources.error,
                    Properties.Resources.unexpected_error,
                    Window.GetWindow(this));
            }
        }

        public void Dispose()
        {
            gameRepository?.Dispose();
        }

        private void PublicToggle_Checked(object sender, RoutedEventArgs e)
        {
        }
    }
}