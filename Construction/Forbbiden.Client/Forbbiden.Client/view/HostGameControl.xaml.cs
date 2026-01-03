using Forbbiden.Client.logic;
using Forbbiden.Client.MatchManager;
using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.view;
using System;
using System.Net.Http;
using System.ServiceModel;
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
            if (NormalButton == null || HardButton == null ||
                NormalMessage == null || HardMessage == null ||
                PublicToggle == null || PublicMessage == null || PrivateMessage == null)
            {
                return;
            }

            selectedDifficulty = "Normal";
            NormalMessage.Visibility = Visibility.Visible;
            HardMessage.Visibility = Visibility.Collapsed;
            NormalButton.Background = Brushes.LightGreen;
            HardButton.Background = Brushes.LightCoral;

            // Visibilidad por defecto: Pública
            selectedVisibility = "Public";
            PublicToggle.IsChecked = true;
            PublicMessage.Visibility = Visibility.Visible;
            PrivateMessage.Visibility = Visibility.Collapsed;
        }

        private void NormalButton_Click(object sender, RoutedEventArgs e)
        {
            selectedDifficulty = "Normal";
            if (NormalMessage == null || HardMessage == null || NormalButton == null || HardButton == null)
            {
                return;
            }

            NormalMessage.Visibility = Visibility.Visible;
            HardMessage.Visibility = Visibility.Collapsed;
            NormalButton.Background = Brushes.LightGreen;
            HardButton.Background = Brushes.LightCoral;
        }

        private void HardButton_Click(object sender, RoutedEventArgs e)
        {
            selectedDifficulty = "Hard";
            if (NormalMessage == null || HardMessage == null || NormalButton == null || HardButton == null)
            {
                return;
            }

            NormalMessage.Visibility = Visibility.Collapsed;
            HardMessage.Visibility = Visibility.Visible;
            HardButton.Background = Brushes.LightGreen;
            NormalButton.Background = Brushes.LightGray;
        }

        private void PublicToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (PublicMessage == null || PrivateMessage == null)
            {
                return;
            }

            selectedVisibility = "Public";
            PublicMessage.Visibility = Visibility.Visible;
            PrivateMessage.Visibility = Visibility.Collapsed;
        }

        private void PublicToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (PublicMessage == null || PrivateMessage == null)
            {
                return;
            }

            selectedVisibility = "Private";
            PublicMessage.Visibility = Visibility.Collapsed;
            PrivateMessage.Visibility = Visibility.Visible;
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var matchClient = new MatchManagerClient();

            try
            {
                var currentPlayer = ClientSession.GetPlayer();

                if (currentPlayer == null)
                {
                    MessageBox.Show("Debes iniciar sesión antes de crear una partida.", "Advertencia",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string username = currentPlayer.PlayerUsername;

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
            }
            catch (TimeoutException)
            {
                MessageBox.Show("La conexión con el servidor ha excedido el tiempo de espera. Intenta más tarde.",
                    "Tiempo de espera agotado", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (EndpointNotFoundException)
            {
                MessageBox.Show("No se pudo conectar con el servidor. Verifica tu conexión a internet.",
                    "Conexión fallida", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show("Ocurrió un problema de comunicación con el servidor.",
                    "Error de comunicación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (HttpRequestException)
            {
                MessageBox.Show("No se pudo contactar el servicio remoto. Verifica tu red.",
                    "Error de red", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                // Último recurso: error no esperado
                MessageBox.Show($"Ocurrió un error inesperado al crear la partida.\nDetalles: {ex.Message}",
                    "Error inesperado", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                try
                {
                    matchClient.Close();
                }
                catch
                {
                    matchClient.Abort();
                }
            }
        }

        private void PlayerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Guardar cantidad máxima de jugadores 
        }
    }
}
