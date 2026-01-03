using Forbbiden.Client.GameManager;
using Forbbiden.Client.logic;
using Forbbiden.Client.MatchManager;
using Forbbiden.Client.ProfileManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
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
            try
            {
                var matchClient = new MatchManagerClient();
                var matches = matchClient.ListMatches();

                allMatches = matches.Select(m => new MatchItem
                {
                    MatchId = m.MatchId,
                    RoomName = $"Room {m.MatchId}",
                    HostName = m.HostUsername ?? "Unknown",
                    PlayersInfo = $"{m.Players?.Length ?? 0}/4",
                    Difficulty = m.Difficulty,
                    LockIcon = m.Visibility == "Private" ? "/Images/lock.png" : "/Images/unlock.png"
                }).ToList();

                MatchList.ItemsSource = allMatches;

                matchClient.Close();
            }
            catch (Exception ex)
            {
                ViewUtils.ShowPullError(Window.GetWindow(this));
            }
        }

        // Evento al escribir en el cuadro de búsqueda
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchPlaceholder != null)
                SearchPlaceholder.Visibility =
                    string.IsNullOrWhiteSpace(SearchBox.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            string filter = SearchBox.Text.ToLower();

            MatchList.ItemsSource = allMatches
                .Where(m => m.RoomName.ToLower().Contains(filter))
                .ToList();
        }

        // Evento del botón de búsqueda
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox_TextChanged(null, null);
        }

        // Evento al hacer clic en el botón de "Unirse"
        private async void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is MatchItem match)
            {
                var currentPlayer = ClientSession.GetPlayer();

                if (currentPlayer.PlayerId == -1)
                {
                    MessageBox.Show("No hay usuario logueado.");
                    return;
                }

                string username = currentPlayer.PlayerUsername;

                // === Configurar callback ===
                GameServiceCallback callback = new GameServiceCallback();
                InstanceContext context = new InstanceContext(callback);

                var gameClient = new GameServiceClient(context);

                bool joined = await gameClient.JoinGameAsync(
                    match.MatchId.ToString(),
                    username
                );

                if (joined)
                {
                    MessageBox.Show($"Unido a la partida {match.RoomName}");

                    // Ir al lobby
                    var window = Window.GetWindow(this);
                    window.Content = new LobbyPage(match.MatchId, username, gameClient, callback);
                }
                else
                {
                    MessageBox.Show("No fue posible unirse a la partida.");
                }
            }
        }


    }

    // Clase auxiliar que se utiliza para mostrar información en la UI
    public class MatchItem
    {
        public int MatchId { get; set; }
        public string RoomName { get; set; }
        public string HostName { get; set; }
        public string PlayersInfo { get; set; }
        public string Difficulty { get; set; }
        public string LockIcon { get; set; }
    }
}
