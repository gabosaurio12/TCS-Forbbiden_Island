using Forbbiden.Client.GameManager; 
using Forbbiden.Client.logic;
using Forbbiden.Client.MatchManager;
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
        private List<MatchItem> allMatches = new List<MatchItem>();

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
                        if (m.Players != null)
                        {
                            var coll = m.Players as System.Collections.ICollection;
                            if (coll != null) playersCount = coll.Count;
                            else playersCount = m.Players.Count(); 
                        }
                    }
                    catch { playersCount = 0; }

                    int capacity = (m.Capacity > 0) ? m.Capacity : 4;

                    return new MatchItem
                    {
                        MatchId = m.MatchId,
                        MatchName = m.MatchName,
                        RoomName = !string.IsNullOrWhiteSpace(m.MatchName) ? m.MatchName : $"Room {m.MatchId}",
                        HostName = m.HostUsername ?? "Unknown",
                        PlayersInfo = $"{playersCount}/{capacity}",
                        CurrentPlayers = playersCount,
                        Capacity = capacity,
                        Difficulty = m.Difficulty ?? "Normal",
                        Visibility = m.Visibility ?? "Public",
                        LockIcon = (m.Visibility ?? "Public").Equals("Private", StringComparison.OrdinalIgnoreCase) ? "/Images/lock.png" : "/Images/unlock.png"
                    };
                }).ToList();

                MatchList.ItemsSource = allMatches;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar las partidas: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (matchClient != null)
                {
                    try { matchClient.Close(); } catch { matchClient.Abort(); }
                }
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filter = (SearchBox.Text ?? "").Trim().ToLower();

            var filtered = allMatches.Where(m =>
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

            if (currentPlayer == null || currentPlayer.PlayerId == -1)
            {
                MessageBox.Show(
                    "Debes iniciar sesión para unirte a una partida.",
                    "Advertencia",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (match.CurrentPlayers >= match.Capacity)
            {
                MessageBox.Show("La partida está llena.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string username = currentPlayer.PlayerUsername;

            MatchManagerClient matchClient = null;
            try
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
                catch { /* no crítico */ }

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
                    MessageBox.Show(
                        "No fue posible unirse a la partida.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    return;
                }

                var lobbyPage = new LobbyPage(
                    match.MatchId,
                    username,
                    gameClient,
                    callback
                );

                NavigationService
                    .GetNavigationService(this)?
                    .Navigate(lobbyPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al unirse a la partida: {ex.Message}",
                    "Error",
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
            }
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