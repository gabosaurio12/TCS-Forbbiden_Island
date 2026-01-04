using Forbbiden.Client.GameManager;
using Forbbiden.Client.logic;
using Forbbiden.Client.MatchManager;
using log4net;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Forbbiden.Client.view
{
    public partial class LeaveMenuPage : Page
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LeaveMenuPage));

        private readonly int MatchId;
        private readonly string CurrentPlayer;
        private readonly GameManagerClient GameClient;
        private readonly GameServiceCallback Callback;

        public LeaveMenuPage(int matchId, string currentPlayer, GameManagerClient gameClient, GameServiceCallback callback)
        {
            InitializeComponent();
            MatchId = matchId;
            CurrentPlayer = currentPlayer;
            GameClient = gameClient;
            Callback = callback;
        }

        private void BtnContinue_Click(object sender, RoutedEventArgs e)
        {
            var lobby = new LobbyPage(MatchId, CurrentPlayer, GameClient, Callback);
            NavigationService?.Navigate(lobby);
        }

        private async void BtnLeave_Click(object sender, RoutedEventArgs e)
        {
            MatchManagerClient matchClient = null;

            LeaveGame();

            matchClient = new MatchManagerClient();
            Match match = null;

            try
            {
                match = await Task.Run(() => matchClient.GetMatchById(MatchId));
            }
            catch (FaultException ex)
            {
                Log.Error("LeaveMenuPage.BtnLeave_Click", ex);
                ViewUtils.ShowPullError(Window.GetWindow(this));
            }

            if (match != null)
            {
                var host = match.HostUsername;
                if (!string.IsNullOrEmpty(host) && string.Equals(
                    host, CurrentPlayer, StringComparison.OrdinalIgnoreCase))
                {
                    DeleteMatch(matchClient);
                }
            }

            JoinGameControl.CloseMatchClient(matchClient);

            CloseGameClient(GameClient);

            NavigationService?.Navigate(new MainPage());
        }

        private async void LeaveGame()
        {
            if (GameClient != null)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        GameClient.LeaveGame(MatchId.ToString(), CurrentPlayer);
                    }
                    catch (Exception ex)
                    {
                        Log.Warn("Error calling LeaveGame", ex);
                    }
                });
            }
        }

        private async void DeleteMatch(MatchManagerClient matchClient)
        {
            try
            {
                await Task.Run(() => matchClient.DeleteMatch(MatchId));
            }
            catch (FaultException ex)
            {
                Log.Error("LeaveMenuPage.BtnLeave_Click", ex);
                ViewUtils.ShowPushError(Window.GetWindow(this));
            }
        }

        public static void CloseGameClient(GameManagerClient client)
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

        private void lobbyPlayersUpdatedFallback(Forbbiden.Client.GameManager.PlayerInfo[] players) { }
    }
}