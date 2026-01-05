using Forbbiden.Client.GameManager;
using Forbbiden.Client.logic;
using Forbbiden.Client.MatchManager;
using log4net;
using System;
using System.Linq;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Forbbiden.Client.view
{
    public partial class LeaveMenuPage : Page
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LeaveMenuPage));

        private readonly int MatchId;
        private readonly string CurrentPlayer;
        private readonly GameManagerClient GameClient;
        private readonly GameServiceCallback Callback;

        private bool isHost = false;

        public LeaveMenuPage(int matchId, string currentPlayer, GameManagerClient gameClient, GameServiceCallback callback)
        {
            InitializeComponent();
            MatchId = matchId;
            CurrentPlayer = currentPlayer;
            GameClient = gameClient;
            Callback = callback;

            _ = DetermineHostAndLoadAsync();
        }

        private async Task DetermineHostAndLoadAsync()
        {
            MatchManagerClient matchClient = null;
            try
            {
                matchClient = new MatchManagerClient();
                var match = await Task.Run(() => matchClient.GetMatchById(MatchId));
                isHost = match != null &&
                         !string.IsNullOrEmpty(match.HostUsername) &&
                         string.Equals(match.HostUsername, CurrentPlayer, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Log.Warn("Error determining host", ex);
                isHost = false;
            }
            finally
            {
                if (matchClient != null)
                {
                    try { matchClient.Close(); } catch { matchClient.Abort(); }
                }
            }

            await Dispatcher.Invoke(async () =>
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
                var players = await Task.Run(() => GameClient.GetPlayers(MatchId.ToString()));
                var names = players?
                    .Select(p => p?.PlayerUsername)
                    .Where(u => !string.IsNullOrEmpty(u) && !string.Equals(u, CurrentPlayer, StringComparison.OrdinalIgnoreCase))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList() ?? new System.Collections.Generic.List<string>();

                KickCombo.ItemsSource = names;
                KickCombo.SelectedIndex = names.Any() ? 0 : -1;
                KickEmpty.Visibility = names.Any() ? Visibility.Collapsed : Visibility.Visible;
            }
            catch (Exception ex)
            {
                Log.Warn("RefreshKickListAsync failed", ex);
                KickCombo.ItemsSource = null;
                KickEmpty.Visibility = Visibility.Visible;
            }
        }

        private void BtnContinue_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new LobbyPage(MatchId, CurrentPlayer, GameClient, Callback));
        }

        private async void BtnLeave_Click(object sender, RoutedEventArgs e)
        {
            MatchManagerClient matchClient = null;

            try
            {
                if (GameClient != null)
                {
                    try
                    {
                        await Task.Run(() => GameClient.LeaveGame(MatchId.ToString(), CurrentPlayer));
                    }
                    catch (Exception ex)
                    {
                        Log.Warn("Error calling LeaveGame", ex);
                    }
                }

                try
                {
                    matchClient = new MatchManagerClient();
                    var match = await Task.Run(() => matchClient.GetMatchById(MatchId));

                    if (match != null &&
                        !string.IsNullOrEmpty(match.HostUsername) &&
                        string.Equals(match.HostUsername, CurrentPlayer, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            await Task.Run(() => matchClient.DeleteMatch(MatchId));
                        }
                        catch (Exception ex)
                        {
                            Log.Warn($"DeleteMatch failed for {MatchId}", ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Warn("MatchManager delete/leave flow failed", ex);
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
                    if (GameClient != null)
                    {
                        try { GameClient.Close(); } catch { GameClient.Abort(); }
                    }
                }
                catch (Exception ex)
                {
                    Log.Warn("Error closing gameClient", ex);
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Error during leave process", ex);
            }
            finally
            {
                try
                {
                    NavigationService?.Navigate(new MainPage());
                }
                catch (Exception ex)
                {
                    Log.Warn("Error navigating to MainPage after leave", ex);
                }
            }
        }

        private async void BtnKickPlayer_Click(object sender, RoutedEventArgs e)
        {
            if (!isHost) return;

            var target = KickCombo.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(target))
            {
                ViewUtils.OpenNotificationWindow(
                    Properties.Resources.leave_menu_kick_no_selection_title,
                    Properties.Resources.leave_menu_kick_no_selection_msg,
                    Window.GetWindow(this));
                return;
            }

            var CurrentPlayers = await Task.Run(() => GameClient.GetPlayers(MatchId.ToString()));
            var targetStillThere = CurrentPlayers?.Any(p =>
                !string.IsNullOrEmpty(p?.PlayerUsername) &&
                string.Equals(p.PlayerUsername, target, StringComparison.OrdinalIgnoreCase)) == true;

            if (!targetStillThere)
            {
                ViewUtils.OpenNotificationWindow(
                    Properties.Resources.leave_menu_kick_not_found_title,
                    Properties.Resources.leave_menu_kick_not_found_msg,
                    Window.GetWindow(this));
                await RefreshKickListAsync();
                return;
            }

            bool success = false;
            try
            {
                await Task.Run(() => GameClient.KickPlayer(MatchId.ToString(), CurrentPlayer, target));
                success = true;
            }
            catch (FaultException fex)
            {
                Log.Warn("KickPlayer fault", fex);
                success = false;
            }
            catch (Exception ex)
            {
                Log.Warn("KickPlayer error", ex);
                success = false;
            }

            if (success)
            {
                ViewUtils.OpenNotificationWindow(
                    Properties.Resources.leave_menu_kick_success_title,
                    string.Format(Properties.Resources.leave_menu_kick_success_msg, target),
                    Window.GetWindow(this));
            }
            else
            {
                ViewUtils.OpenNotificationWindow(
                    Properties.Resources.leave_menu_kick_fail_title,
                    Properties.Resources.leave_menu_kick_fail_msg,
                    Window.GetWindow(this));
            }

            await RefreshKickListAsync();
        }
    }
}