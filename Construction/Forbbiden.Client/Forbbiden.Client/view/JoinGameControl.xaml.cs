using Forbbiden.Client.Exceptions;
using Forbbiden.Client.logic;
using Forbbiden.Client.Logic;
using Forbbiden.Client.Repositories;
using Forbbiden.Client.view.info;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace Forbbiden.Client.view
{
    public partial class JoinGameControl : UserControl
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(JoinGameControl));

        private readonly MatchRepository matchRepository;
        private GameRepository gameRepository;
        private GameServiceCallback gameCallback;

        private List<MatchItem> allMatches;

        public JoinGameControl()
        {
            InitializeComponent();

            matchRepository = new MatchRepository();
            allMatches = new List<MatchItem>();

            Loaded += JoinGameControl_Loaded;
        }

        private async void JoinGameControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadMatches();
        }

        private async Task LoadMatches()
        {
            try
            {
                var matches = await matchRepository.ListMatches();

                allMatches = matches.Select(match =>
                {
                    int playersCount;

                    try
                    {
                        playersCount = match.Players?.Count() ?? 0;
                    }
                    catch
                    {
                        playersCount = 0;
                    }

                    int capacity = match.Capacity > 0
                        ? match.Capacity
                        : 4;

                    string visibilityKey = match.Visibility
                        ?? Properties.Resources.visibility_public_key;

                    bool isPrivate = visibilityKey.Equals(
                        Properties.Resources.visibility_private_key,
                        StringComparison.OrdinalIgnoreCase);

                    return new MatchItem
                    {
                        MatchId = match.MatchId,
                        MatchName = match.MatchName,
                        RoomName = !string.IsNullOrWhiteSpace(match.MatchName)
                            ? match.MatchName
                            : string.Format(
                                Properties.Resources.room_default,
                                match.MatchId),
                        HostName = string.IsNullOrEmpty(match.HostUsername)
                            ? Properties.Resources.host_unknown
                            : match.HostUsername,
                        PlayersInfo = $"{playersCount}/{capacity}",
                        CurrentPlayers = playersCount,
                        Capacity = capacity,
                        Difficulty = match.Difficulty
                            ?? Properties.Resources.difficulty_normal,
                        Visibility = visibilityKey,
                        LockIcon = isPrivate
                            ? "/Images/lock.png"
                            : "/Images/unlock.png",
                        VisibilityText = isPrivate
                            ? Properties.Resources.visibility_private
                            : Properties.Resources.visibility_public,
                        VisibilityColor = isPrivate
                            ? Brushes.IndianRed
                            : Brushes.SeaGreen
                    };
                }).ToList();

                MatchList.ItemsSource = allMatches;
            }
            catch (ViewException ex)
            {
                ErrorsNotificationManager.ShowViewExceptionNotification(
                    ex, Window.GetWindow(this));
            }
            catch (Exception ex)
            {
                Log.Error("JoinGameControl.LoadMatches", ex);

                ViewUtils.OpenNotificationWindow(
                    Properties.Resources.error,
                    Properties.Resources.loading_matches_error,
                    Window.GetWindow(this));
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filter = (SearchBox.Text ?? string.Empty)
                .Trim()
                .ToLowerInvariant();

            var filteredMatches = allMatches.Where(match =>
                (!string.IsNullOrEmpty(match.RoomName) &&
                 match.RoomName.ToLowerInvariant().Contains(filter)) ||
                (!string.IsNullOrEmpty(match.HostName) &&
                 match.HostName.ToLowerInvariant().Contains(filter)) ||
                match.MatchId.ToString().Contains(filter) ||
                (!string.IsNullOrEmpty(match.Difficulty) &&
                 match.Difficulty.ToLowerInvariant().Contains(filter))
            ).ToList();

            MatchList.ItemsSource = filteredMatches;
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadMatches();
        }

        private async void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button &&
                button.DataContext is MatchItem match))
            {
                return;
            }

            var currentPlayer = ClientSession.GetPlayer();

            if (currentPlayer == null || currentPlayer.PlayerId == -1)
            {
                ViewUtils.OpenNotificationWindow(
                    Properties.Resources.error,
                    Properties.Resources.unexpected_error,
                    Window.GetWindow(this));

                return;
            }

            if (match.CurrentPlayers >= match.Capacity)
            {
                var window = new NotificationWindow(
                    Properties.Resources.join_full_title,
                    Properties.Resources.join_full_message)
                {
                    Owner = Window.GetWindow(this)
                };

                window.ShowDialog();
                return;
            }

            bool isPrivate = match.Visibility.Equals(
                Properties.Resources.visibility_private_key,
                StringComparison.OrdinalIgnoreCase);

            if (isPrivate)
            {
                var inviteWindow = new InviteCodeWindow
                {
                    Owner = Window.GetWindow(this)
                };

                if (inviteWindow.ShowDialog() != true)
                {
                    return;
                }

                bool isValidInvite;

                try
                {
                    isValidInvite = await matchRepository.ValidateInvite(
                        match.MatchId,
                        inviteWindow.Code);
                }
                catch (ViewException ex)
                {
                    ErrorsNotificationManager.ShowViewExceptionNotification(
                        ex, Window.GetWindow(this));
                    return;
                }
                catch (Exception ex)
                {
                    Log.Error(
                        "JoinGameControl.JoinButton_Click ValidateInvite",
                        ex);
                    return;
                }

                if (!isValidInvite)
                {
                    ViewUtils.OpenNotificationWindow(
                        Properties.Resources.invite_invalid_title,
                        Properties.Resources.invite_invalid_message,
                        Window.GetWindow(this));

                    return;
                }
            }

            string username = currentPlayer.PlayerUsername;
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
                    match.MatchId.ToString(),
                    username,
                    avatarBytes,
                    avatarFileName);

                if (!joined)
                {
                    ViewUtils.OpenNotificationWindow(
                        Properties.Resources.join_banned_title,
                        Properties.Resources.join_banned_message,
                        Window.GetWindow(this));

                    return;
                }

                var lobbyPage = new LobbyPage(
                    match.MatchId,
                    username,
                    gameRepository.Client,
                    gameCallback);

                NavigationService
                    .GetNavigationService(this)
                    ?.Navigate(lobbyPage);
            }
            catch (ViewException ex)
            {
                ErrorsNotificationManager.ShowViewExceptionNotification(
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
                Log.Error("JoinGameControl.JoinButton_Click", ex);

                ViewUtils.OpenNotificationWindow(
                    Properties.Resources.error,
                    Properties.Resources.unexpected_error,
                    Window.GetWindow(this));
            }
        }

        public class MatchItem
        {
            public int MatchId { get; set; }
            public string MatchName { get; set; }
            public string RoomName { get; set; }
            public string HostName { get; set; }
            public string PlayersInfo { get; set; }
            public int CurrentPlayers { get; set; }
            public int Capacity { get; set; }
            public string Difficulty { get; set; }
            public string Visibility { get; set; }
            public string LockIcon { get; set; }
            public string VisibilityText { get; set; }
            public Brush VisibilityColor { get; set; }
        }
    }
}