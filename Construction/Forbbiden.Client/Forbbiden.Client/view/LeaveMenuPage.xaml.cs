using Forbbiden.Client.GameManager;
using Forbbiden.Client.MatchManager;
using Forbbiden.Client.logic;
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
        private static readonly ILog log = LogManager.GetLogger(typeof(LeaveMenuPage));

        private readonly int matchId;
        private readonly string currentPlayer;
        private readonly GameManagerClient gameClient;
        private readonly GameServiceCallback callback;

        public LeaveMenuPage(int matchId, string currentPlayer, GameManagerClient gameClient, GameServiceCallback callback)
        {
            InitializeComponent();
            this.matchId = matchId;
            this.currentPlayer = currentPlayer;
            this.gameClient = gameClient;
            this.callback = callback;
        }

        private void BtnContinue_Click(object sender, RoutedEventArgs e)
        {
            // Volver al lobby con los mismos objetos
            var lobby = new LobbyPage(matchId, currentPlayer, gameClient, callback);
            NavigationService?.Navigate(lobby);
        }

        private async void BtnLeave_Click(object sender, RoutedEventArgs e)
        {
            MatchManagerClient matchClient = null;
            try
            {
                try
                {
                    if (gameClient != null)
                    {
                        await Task.Run(() =>
                        {
                            try { gameClient.LeaveGame(matchId.ToString(), currentPlayer); }
                            catch (Exception ex) { log.Warn("Error calling LeaveGame", ex); }
                        });
                    }
                }
                catch (Exception ex)
                {
                    log.Warn("Error calling LeaveGame (outer)", ex);
                }

                try
                {
                    matchClient = new MatchManagerClient();
                    Forbbiden.Client.MatchManager.Match match = null;

                    try
                    {
                        match = await Task.Run(() => matchClient.GetMatchById(matchId));
                    }
                    catch (Exception ex)
                    {
                        log.Warn($"Error retrieving match {matchId} info", ex);
                    }

                    if (match != null)
                    {
                        var host = match.HostUsername;
                        if (!string.IsNullOrEmpty(host) && string.Equals(host, currentPlayer, StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                bool deleted = false;
                                try
                                {
                                    deleted = await Task.Run(() => matchClient.DeleteMatch(matchId));
                                }
                                catch (Exception ex)
                                {
                                    log.Warn($"DeleteMatch call failed for {matchId}", ex);
                                }

                                if (deleted)
                                {
                                    log.Info($"Match {matchId} deleted by host {currentPlayer}");
                                }
                                else
                                {
                                    log.Warn($"DeleteMatch returned false for {matchId}");
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Warn($"Error while deleting match {matchId}", ex);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Warn("Error handling MatchManager delete flow", ex);
                }
                finally
                {
                    if (matchClient != null)
                    {
                        try { matchClient.Close(); }
                        catch { try { matchClient.Abort(); } catch { } }
                    }
                }

                try
                {
                    if (gameClient != null)
                    {
                        try { gameClient.Close(); }
                        catch
                        {
                            try { gameClient.Abort(); } catch { }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Warn("Error during client cleanup", ex);
                }
            }
            catch (Exception ex)
            {
                log.Warn("Error during leave process", ex);
            }
            finally
            {
                try
                {
                    NavigationService?.Navigate(new MainPage());
                }
                catch (Exception ex)
                {
                    log.Warn("Error navigating to MainPage after leave", ex);
                }
            }
        }

        private void lobbyPlayersUpdatedFallback(Forbbiden.Client.GameManager.PlayerInfo[] players) { }
    }
}