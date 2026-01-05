using Forbbiden.Client.GameManager;
using Forbbiden.Client.logic;
using Forbbiden.Client.MatchManager;
using Forbbiden.Client.view.info;
using log4net;
using System;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Forbbiden.Client.view
{
    public partial class LeaveMenuPage : Page
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LeaveMenuPage));

        private readonly int matchId;
        private readonly string currentPlayer;
        private readonly GameManagerClient gameClient;
        private readonly GameServiceCallback callback;

        private bool isHost = false;

        public LeaveMenuPage(int matchId, string currentPlayer, GameManagerClient gameClient, GameServiceCallback callback)
        {
            InitializeComponent();
            this.matchId = matchId;
            this.currentPlayer = currentPlayer;
            this.gameClient = gameClient;
            this.callback = callback;

            _ = DetermineHostAndLoadAsync();
        }

        private async Task DetermineHostAndLoadAsync()
        {
            MatchManagerClient matchClient = null;
            try
            {
                matchClient = new MatchManagerClient();
                var match = await Task.Run(() => matchClient.GetMatchById(matchId));
                isHost = match != null &&
                         !string.IsNullOrEmpty(match.HostUsername) &&
                         string.Equals(match.HostUsername, currentPlayer, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                log.Warn("Error determining host", ex);
                isHost = false;
            }
            finally
            {
                if (matchClient != null)
                {
                    try { matchClient.Close(); } catch { matchClient.Abort(); }
                }
            }

            Dispatcher.Invoke(async () =>
            {
                KickSection.Visibility = isHost ? Visibility.Visible : Visibility.Collapsed;
                if (isHost)
                {
                    await RefreshKickListAsync();
                }
            });
        }

        private async Task RefreshKickListAsync()
        {
            try
            {
                var players = await Task.Run(() => gameClient.GetPlayers(matchId.ToString()));
                var names = players?
                    .Select(p => p?.PlayerUsername)
                    .Where(u => !string.IsNullOrEmpty(u) && !string.Equals(u, currentPlayer, StringComparison.OrdinalIgnoreCase))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList() ?? new System.Collections.Generic.List<string>();

                KickCombo.ItemsSource = names;
                KickCombo.SelectedIndex = names.Any() ? 0 : -1;
                KickEmpty.Visibility = names.Any() ? Visibility.Collapsed : Visibility.Visible;
            }
            catch (Exception ex)
            {
                log.Warn("RefreshKickListAsync failed", ex);
                KickCombo.ItemsSource = null;
                KickEmpty.Visibility = Visibility.Visible;
            }
        }

        private void BtnContinue_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new LobbyPage(matchId, currentPlayer, gameClient, callback));
        }

        private async void BtnLeave_Click(object sender, RoutedEventArgs e)
        {
            MatchManagerClient matchClient = null;

            try
            {
                if (gameClient != null)
                {
                    try
                    {
                        await Task.Run(() => gameClient.LeaveGame(matchId.ToString(), currentPlayer));
                    }
                    catch (Exception ex)
                    {
                        log.Warn("Error calling LeaveGame", ex);
                    }
                }

                try
                {
                    matchClient = new MatchManagerClient();
                    var match = await Task.Run(() => matchClient.GetMatchById(matchId));

                    if (match != null &&
                        !string.IsNullOrEmpty(match.HostUsername) &&
                        string.Equals(match.HostUsername, currentPlayer, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            await Task.Run(() => matchClient.DeleteMatch(matchId));
                        }
                        catch (Exception ex)
                        {
                            log.Warn($"DeleteMatch failed for {matchId}", ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Warn("MatchManager delete/leave flow failed", ex);
                }
                finally
                {
                    if (matchClient != null)
                    {
                        try { matchClient.Close(); } catch { matchClient.Abort(); }
                    }
                }

                try
                {
                    if (gameClient != null)
                    {
                        try { gameClient.Close(); } catch { gameClient.Abort(); }
                    }
                }
                catch (Exception ex)
                {
                    log.Warn("Error closing gameClient", ex);
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

        private async void BtnKickPlayer_Click(object sender, RoutedEventArgs e)
        {
            if (!isHost) return;

            var target = KickCombo.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(target))
            {
                ViewUtils.OpenNotificationWindow(Properties.Resources.leave_menu_kick_no_selection_title,
                                                 Properties.Resources.leave_menu_kick_no_selection_msg,
                                                 Window.GetWindow(this));
                return;
            }

            // Verificar que siga en la partida
            var currentPlayers = await Task.Run(() => gameClient.GetPlayers(matchId.ToString()));
            var targetStillThere = currentPlayers?.Any(p =>
                !string.IsNullOrEmpty(p?.PlayerUsername) &&
                string.Equals(p.PlayerUsername, target, StringComparison.OrdinalIgnoreCase)) == true;

            if (!targetStillThere)
            {
                ViewUtils.OpenNotificationWindow(Properties.Resources.leave_menu_kick_not_found_title,
                                                 Properties.Resources.leave_menu_kick_not_found_msg,
                                                 Window.GetWindow(this));
                await RefreshKickListAsync();
                return;
            }

            bool success = false;
            try
            {
                await Task.Run(() => gameClient.KickPlayer(matchId.ToString(), currentPlayer, target));
                success = true;
            }
            catch (FaultException fex)
            {
                log.Warn("KickPlayer fault", fex);
                success = false;
            }
            catch (Exception ex)
            {
                log.Warn("KickPlayer error", ex);
                success = false;
            }

            if (success)
            {
                ViewUtils.OpenNotificationWindow(Properties.Resources.leave_menu_kick_success_title,
                                                 string.Format(Properties.Resources.leave_menu_kick_success_msg, target),
                                                 Window.GetWindow(this));
            }
            else
            {
                ViewUtils.OpenNotificationWindow(Properties.Resources.leave_menu_kick_fail_title,
                                                 Properties.Resources.leave_menu_kick_fail_msg,
                                                 Window.GetWindow(this));
            }

            await RefreshKickListAsync();
        }
    }
}