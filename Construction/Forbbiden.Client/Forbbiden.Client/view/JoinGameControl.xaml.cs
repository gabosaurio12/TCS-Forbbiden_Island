using Forbbiden.Client.GameManager;
using Forbbiden.Client.logic;
using Forbbiden.Client.MatchManager;
using Forbbiden.Client.view.info;
using Forbbiden.Client.ProfileManager;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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

        private List<MatchItem> AllMatches = new List<MatchItem>();

        public JoinGameControl()
        {
            InitializeComponent();
            Loaded += JoinGameControl_Loaded;
        }

        private void JoinGameControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadMatches();
        }

        private void LoadMatches()
        {
            MatchManagerClient matchClient = null;
            try
            {
                matchClient = new MatchManagerClient();
                var matches = matchClient.ListMatches();

                allMatches = matches.Select(m =>
                {
                    int playersCount = 0;
                    try
                    {
                        if (m.Players is System.Collections.ICollection coll) playersCount = coll.Count;
                        else if (m.Players != null) playersCount = m.Players.Count();
                    }
                    catch { playersCount = 0; }

                    int capacity = (m.Capacity > 0) ? m.Capacity : 4;

                    return new MatchItem
                    {
                        MatchId = m.MatchId,
                        MatchName = m.MatchName,
                        RoomName = !string.IsNullOrWhiteSpace(m.MatchName)
                            ? m.MatchName
                            : string.Format(Properties.Resources.room_default, m.MatchId),
                        HostName = string.IsNullOrEmpty(m.HostUsername) ? Properties.Resources.host_unknown : m.HostUsername,
                        PlayersInfo = $"{playersCount}/{capacity}",
                        CurrentPlayers = playersCount,
                        Capacity = capacity,
                        Difficulty = m.Difficulty ?? Properties.Resources.difficulty_normal,
                        Visibility = m.Visibility ?? Properties.Resources.visibility_public_key,
                        LockIcon = (m.Visibility ?? Properties.Resources.visibility_public_key)
                            .Equals(Properties.Resources.visibility_private_key, StringComparison.OrdinalIgnoreCase)
                            ? "/Images/lock.png" : "/Images/unlock.png",
                        VisibilityText = (m.Visibility ?? Properties.Resources.visibility_public_key)
                            .Equals(Properties.Resources.visibility_private_key, StringComparison.OrdinalIgnoreCase)
                            ? Properties.Resources.visibility_private
                            : Properties.Resources.visibility_public,
                        VisibilityColor = (m.Visibility ?? Properties.Resources.visibility_public_key)
                            .Equals(Properties.Resources.visibility_private_key, StringComparison.OrdinalIgnoreCase)
                            ? Brushes.IndianRed : Brushes.SeaGreen
                    };
                }).ToList();

                MatchList.ItemsSource = allMatches;
            }
            catch (Exception ex)
            {
                string title = Properties.Langs.Resources.error;
                string message = Properties.Langs.Resources.loading_matches_error;
                ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
            }
            finally
            {
                CloseMatchClient(matchClient);
            }
        }

        private MatchItem MapToMatchItem(Match match)
        {
            int playersCount = GetPlayersCount(match.Players);
            int capacity = match.Capacity > 0 ? match.Capacity : 4;
            string visibility = match.Visibility ?? "Public";

            return new MatchItem
            {
                MatchId = match.MatchId,
                MatchName = match.MatchName,
                RoomName = !string.IsNullOrWhiteSpace(match.MatchName)
                    ? match.MatchName
                    : $"Room {match.MatchId}",

                HostName = match.HostUsername ?? "Unknown",
                PlayersInfo = $"{playersCount}/{capacity}",
                CurrentPlayers = playersCount,
                Capacity = capacity,
                Difficulty = match.Difficulty ?? "Normal",
                Visibility = visibility,
                LockIcon = visibility.Equals("Private", StringComparison.OrdinalIgnoreCase)
                    ? "/Images/lock.png"
                    : "/Images/unlock.png"
            };
        }

        public static void CloseMatchClient(MatchManagerClient client)
        {
            if (client == null)
            {
                return;
            }

            try
            {
                if (client.State == CommunicationState.Faulted)
                {
                    client.Abort();
                }
                else
                {
                    client.Close();
                }
            }
            catch
            {
                client.Abort();
            }
        }


        private static int GetPlayersCount(MatchManager.PlayerInfo[] players)
        {
            if (players == null)
            {
                return 0;
            }

            return players.Count();
        }


        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filter = (SearchBox.Text ?? "").Trim().ToLower();

            var filtered = AllMatches.Where(m =>
                (!string.IsNullOrEmpty(m.RoomName) && m.RoomName.ToLower().Contains(filter)) ||
                (!string.IsNullOrEmpty(m.HostName) && m.HostName.ToLower().Contains(filter)) ||
                m.MatchId.ToString().Contains(filter) ||
                (!string.IsNullOrEmpty(m.Difficulty) && m.Difficulty.ToLower().Contains(filter))
            ).ToList();

            MatchList.ItemsSource = filtered;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadMatches();
        }


        private async void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn && btn.DataContext is MatchItem match))
                return;

            var currentPlayer = ClientSession.GetPlayer();

            if (currentPlayer.PlayerId == -1)
            {
                string title = Properties.Langs.Resources.error;
                string message = Properties.Langs.Resources.unexpected_error;
                ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
                return;
            }

            if (match.CurrentPlayers >= match.Capacity)
            {
                var wnd = new NotificationWindow(
                    Properties.Resources.join_full_title,
                    Properties.Resources.join_full_message);
                wnd.Owner = Window.GetWindow(this);
                wnd.ShowDialog();
                return;
            }

            // Validación de código de invitación si es privada
            bool isPrivate = string.Equals(match.Visibility, Properties.Resources.visibility_private_key, StringComparison.OrdinalIgnoreCase);
            string inviteCode = null;
            if (isPrivate)
            {
                var codeWindow = new InviteCodeWindow();
                if (codeWindow.ShowDialog() == true)
                {
                    inviteCode = codeWindow.Code;
                }
                else
                {
                    return; // canceló
                }

                var mClient = new MatchManagerClient();
                bool ok = false;
                try
                {
                    ok = await mClient.ValidateInviteAsync(match.MatchId, inviteCode);
                }
                catch (Exception ex)
                {
                    //falta
                }
                finally
                {
                    try { mClient.Close(); } catch { mClient.Abort(); }
                }

                if (!ok)
                {
                    ViewUtils.OpenNotificationWindow(
                        Properties.Resources.invite_invalid_title,
                        Properties.Resources.invite_invalid_message,
                        Window.GetWindow(this));
                    return;
                }
            }
                string title = Properties.Langs.Resources.advice_title;
                string message = Properties.Langs.Resources.match_is_full_advice_message;
                ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
                return;
            }

            var callback = new GameServiceCallback();
            var context = new InstanceContext(callback);
            var gameClient = new GameManagerClient(context);

            string username = currentPlayer.PlayerUsername;

            bool joined = await JoinToAMatch(match, currentPlayer, gameClient);

            if (!joined)
            {
                string avatarFileName = null;
                try
                {
                    var avatarPath = currentPlayer?.PlayerAvatarPath;
                    if (!string.IsNullOrEmpty(avatarPath))
                    {
                        avatarFileName = System.IO.Path.GetFileName(avatarPath);
                    }
                }
                catch { }

                var callback = new GameServiceCallback();
                var context = new InstanceContext(callback);

                var gameClient = new GameManagerClient(context);

                bool joined = await gameClient.JoinGameAsync(
                    match.MatchId.ToString(),
                    username,
                    null,
                    avatarFileName
                );

                if (!joined)
                {
                    var wnd = new NotificationWindow(
                        Properties.Resources.join_banned_title,
                        Properties.Resources.join_banned_message);
                    wnd.Owner = Window.GetWindow(this);
                    wnd.ShowDialog();
                    return;
                }

                var lobbyPage = new LobbyPage(
                    match.MatchId,
                    username,
                    gameClient,
                    callback
                );

                NavigationService.GetNavigationService(this)?.Navigate(lobbyPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(Properties.Resources.join_error, ex.Message),
                    Properties.Resources.error_title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                if (matchClient != null)
                {
                    try { matchClient.Close(); } catch { matchClient.Abort(); }
                }
                string title = Properties.Langs.Resources.error;
                string message = Properties.Langs.Resources.join_match_error;
                ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
                return;
            }

            var lobbyPage = new LobbyPage(
                match.MatchId,
                username,
                gameClient,
                callback
            );

            NavigationService.GetNavigationService(this)?.Navigate(lobbyPage);
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