using Forbbiden.Client.GameManager;
using Forbbiden.Client.logic;
using Forbbiden.Client.MatchManager;
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

                AllMatches = matches.Select(MapToMatchItem).ToList();

                MatchList.ItemsSource = AllMatches;
            }
            catch (Exception)
            {
                string title = Properties.Langs.Resources.error;
                string message = Properties.Langs.Resources.loading_matches_error;
                ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
            }
            finally
            {
                CloseClient(matchClient);
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

        private void CloseClient(MatchManagerClient client)
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


        private int GetPlayersCount(MatchManager.PlayerInfo[] players)
        {
            if (players == null)
            {
                return 0;
            }

            if (players is System.Collections.ICollection collection)
            {
                return collection.Count;
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

        private async Task<bool> JoinToAMatch(MatchItem match, Player currentPlayer, GameManagerClient gameClient)
        {
            bool joined = false;
            try
            {
                string avatarFileName = null;
                var avatarPath = currentPlayer.PlayerAvatarPath;
                if (!string.IsNullOrEmpty(avatarPath))
                {
                    avatarFileName = System.IO.Path.GetFileName(avatarPath);
                }

                joined = await gameClient.JoinGameAsync(
                    match.MatchId.ToString(),
                    currentPlayer.PlayerUsername,
                    null,
                    avatarFileName
                );
            }
            catch (Exception ex)
            {
                Log.Error("JoinGameControl.JoinButtonClick", ex);
                string title = Properties.Langs.Resources.error;
                string message = Properties.Langs.Resources.join_match_error;
                ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));
            }

            return joined;
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

        public string VisibilityText => string.Equals(Visibility, "Private", StringComparison.OrdinalIgnoreCase) ? "Privada" : "Pública";
        public Brush VisibilityColor => string.Equals(Visibility, "Private", StringComparison.OrdinalIgnoreCase) ? Brushes.IndianRed : Brushes.SeaGreen;
    }
}