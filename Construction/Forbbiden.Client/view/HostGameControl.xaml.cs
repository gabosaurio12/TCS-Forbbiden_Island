using Forbbiden.Client.MatchManager;
using Forbbiden.Client.ProfileManager;       
using Forbbiden.Client.view;                  
using Forbbiden.Contracts;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace Forbbiden.Client
{
    public partial class HostGameControl : UserControl
    {
        private string selectedDifficulty = "Normal";
        private string selectedVisibility = "Public";

        public HostGameControl()
        {
            InitializeComponent();
            this.Loaded += HostGameControl_Loaded;
        }

        private void HostGameControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Estado inicial de dificultad
            NormalMessage.Visibility = Visibility.Visible;
            HardMessage.Visibility = Visibility.Collapsed;
            NormalButton.Background = Brushes.LightGreen;
            HardButton.Background = Brushes.LightCoral;

            // Estado inicial de visibilidad
            if (PublicToggle.IsChecked == true)
            {
                PublicMessage.Visibility = Visibility.Visible;
                PrivateMessage.Visibility = Visibility.Collapsed;
                selectedVisibility = "Public";
            }
            else
            {
                PublicMessage.Visibility = Visibility.Collapsed;
                PrivateMessage.Visibility = Visibility.Visible;
                selectedVisibility = "Private";
            }
        }

        private void NormalButton_Click(object sender, RoutedEventArgs e)
        {
            selectedDifficulty = "Normal";
            NormalMessage.Visibility = Visibility.Visible;
            HardMessage.Visibility = Visibility.Collapsed;

            NormalButton.Background = Brushes.LightGreen;
            HardButton.Background = Brushes.LightCoral;
        }

        private void HardButton_Click(object sender, RoutedEventArgs e)
        {
            selectedDifficulty = "Hard";
            NormalMessage.Visibility = Visibility.Collapsed;
            HardMessage.Visibility = Visibility.Visible;

            HardButton.Background = Brushes.LightGreen;
            NormalButton.Background = Brushes.LightGray;
        }

        private void PublicToggle_Checked(object sender, RoutedEventArgs e)
        {
            selectedVisibility = "Public";
            PublicMessage.Visibility = Visibility.Visible;
            PrivateMessage.Visibility = Visibility.Collapsed;
        }

        private void PublicToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            selectedVisibility = "Private";
            PublicMessage.Visibility = Visibility.Collapsed;
            PrivateMessage.Visibility = Visibility.Visible;
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var profileClient = new ProfileManagerClient();
                var currentPlayer = await profileClient.GetCurrentLoginAsync();

                if (currentPlayer == null)
                {
                    MessageBox.Show("Debes iniciar sesión antes de crear una partida.", "Advertencia",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string username = currentPlayer.PlayerUsername;

                var matchClient = new MatchManagerClient();

                var request = new CreateMatchRequest
                {
                    HostUsername = username,
                    Difficulty = selectedDifficulty,
                    Visibility = selectedVisibility
                };

                int matchId = await matchClient.CreateMatchAsync(request);

                if (matchId > 0)
                {
                    MessageBox.Show($"Partida creada exitosamente (ID: {matchId})", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    var lobbyPage = new LobbyPage(matchId);
                    NavigationService.GetNavigationService(this)?.Navigate(lobbyPage);
                }
                else
                {
                    MessageBox.Show("No se pudo crear la partida. Intenta nuevamente.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                await matchClient.CloseAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear la partida: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PlayerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //TO DO
        }
    }
}
