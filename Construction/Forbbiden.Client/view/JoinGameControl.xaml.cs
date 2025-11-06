using Forbbiden.Client.MatchManager;
using Forbbiden.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

        // Se ejecuta al cargar el control
        private void JoinGameControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadMatches();
        }

        // Carga las partidas disponibles desde el servidor
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
                    PlayersInfo = $"{m.Players?.Count ?? 0}/4",
                    Difficulty = m.Difficulty,
                    LockIcon = m.Visibility == "Private" ? "/Images/lock.png" : "/Images/unlock.png"
                }).ToList();

                MatchList.ItemsSource = allMatches;
                matchClient.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar las partidas: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Evento al escribir en el cuadro de búsqueda
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchPlaceholder != null)
                SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(SearchBox.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            string filter = SearchBox.Text.ToLower();
            MatchList.ItemsSource = allMatches
                .Where(m => m.RoomName.ToLower().Contains(filter))
                .ToList();
        }

        // Evento del botón de búsqueda (simplemente refresca el filtro)
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox_TextChanged(null, null);
        }

        // Evento al hacer clic en el botón de "Unirse"
        private void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is MatchItem match)
            {
                MessageBox.Show($"Intentando unirse a la partida {match.RoomName}...");
                // Aquí se llamará a JoinMatch más adelante
            }
        }
    }

    // Clase auxiliar para mostrar la información de las partidas
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
