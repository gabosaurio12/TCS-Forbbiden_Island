using Forbbiden.Client.logic;
using Forbbiden.Client.GameManager;
using Forbbiden.Client.ProfileManager;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Path = System.IO.Path;

namespace Forbbiden.Client.view.games
{
    public partial class RiuvPage : Page
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RiuvPage));

        private readonly int matchId;
        private readonly string currentPlayer;
        private readonly GameManagerClient gameClient;
        private readonly GameServiceCallback callback;

        private DispatcherTimer countdownTimer;
        private DispatcherTimer preCountdownTimer;

        private int remainingSeconds = 20;
        private int preCountdown = 3;

        private readonly List<string> playersOrder = new List<string>();
        private readonly Dictionary<string, int> playerSlot = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> scores = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<int, char> currentKeyBySlot = new Dictionary<int, char>();

        private readonly List<char> possibleKeys;

        private bool amHost = false;

        private AudioManager audioManager;
        private MediaPlayer countdownEffectPlayer;
        private bool countdownEffectEnded = false;

        private readonly object sync = new object();

        private Action<Forbbiden.Client.GameManager.PlayerInfo[]> playersUpdatedHandler;
        private Action<string, string> chatMessageHandler;

        private readonly Dictionary<string, int> finalScoresReceived = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private bool localFinished = false;
        private bool gameActive = false;
        private bool keysGenerated = false;

        public RiuvPage(int matchId, string username, GameManagerClient gameClient, GameServiceCallback callback)
        {
            InitializeComponent();

            this.matchId = matchId;
            this.currentPlayer = username;
            this.gameClient = gameClient;
            this.callback = callback;

            audioManager = new AudioManager();

            possibleKeys = new List<char>();
            possibleKeys.AddRange(Enumerable.Range('A', 26).Select(c => (char)c));
            possibleKeys.AddRange(Enumerable.Range(0, 10).Select(n => n.ToString()[0]));

            playersUpdatedHandler = OnPlayersUpdatedProxy;
            chatMessageHandler = (p, m) => Dispatcher.BeginInvoke(new Action(() => OnChatMessageReceived(p, m)));

            if (this.callback != null)
            {
                try { this.callback.PlayersUpdated -= playersUpdatedHandler; } catch { }
                try { this.callback.ChatMessageReceived -= chatMessageHandler; } catch { }

                this.callback.PlayersUpdated += playersUpdatedHandler;
                this.callback.ChatMessageReceived += chatMessageHandler;
            }

            Loaded += RiuvPage_Loaded;
            Unloaded += RiuvPage_Unloaded;

            Focusable = true;
            Focus();
            KeyDown += RiuvPage_KeyDown;
        }

        private void RiuvPage_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshPlayersFromServer();

            if (playersOrder.Count > 0 && string.Equals(playersOrder[0], currentPlayer, StringComparison.OrdinalIgnoreCase))
                amHost = true;
            else
                amHost = false;

            localFinished = false;
            finalScoresReceived.Clear();
            keysGenerated = false;
            gameActive = false;
            remainingSeconds = 20;
            preCountdown = 3;
            countdownEffectEnded = false;

            countdownEffectPlayer = new MediaPlayer();
            try
            {
                countdownEffectPlayer.Open(new Uri("sounds/gameCountdown.mp3", UriKind.Relative));
                countdownEffectPlayer.Volume = 1.0;
            }
            catch { }

            countdownEffectPlayer.MediaEnded -= CountdownEffectPlayer_MediaEnded;
            countdownEffectPlayer.MediaEnded += CountdownEffectPlayer_MediaEnded;

            StartPreCountdown();
            Focus();
        }

        private void RiuvPage_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (callback != null)
                {
                    try { this.callback.PlayersUpdated -= playersUpdatedHandler; } catch { }
                    try { this.callback.ChatMessageReceived -= chatMessageHandler; } catch { }
                }
            }
            catch { }

            try { countdownEffectPlayer?.Stop(); } catch { }
            try { countdownEffectPlayer?.Close(); } catch { }
            countdownEffectPlayer = null;

            audioManager?.Dispose();
            audioManager = null;

            try { countdownTimer?.Stop(); } catch { }
            try { preCountdownTimer?.Stop(); } catch { }

            KeyDown -= RiuvPage_KeyDown;
        }

        private void CountdownEffectPlayer_MediaEnded(object sender, EventArgs e)
        {
            countdownEffectEnded = true;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                StartMainGameAfterKeys();
            }));
        }

        private void RefreshPlayersFromServer()
        {
            try
            {
                var serverPlayers = gameClient.GetPlayers(matchId.ToString());
                UpdateSlotsFromPlayerInfos(serverPlayers);
            }
            catch (Exception ex)
            {
                log.Warn("RefreshPlayersFromServer failed", ex);
            }
        }

        private void OnPlayersUpdatedProxy(Forbbiden.Client.GameManager.PlayerInfo[] players)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    UpdateSlotsFromPlayerInfos(players);
                }
                catch (Exception ex) { log.Warn("OnPlayersUpdatedProxy failed", ex); }
            }));
        }

        private void UpdateSlotsFromPlayerInfos(Forbbiden.Client.GameManager.PlayerInfo[] list)
        {
            var ordered = (list ?? new Forbbiden.Client.GameManager.PlayerInfo[0]).OrderBy(p => p?.Position ?? 0).ToArray();
            playersOrder.Clear();
            playerSlot.Clear();

            lock (sync)
            {
                scores.Clear();
                currentKeyBySlot.Clear();
                finalScoresReceived.Clear();
            }

            for (int i = 0; i < ordered.Length && i < 4; i++)
            {
                var p = ordered[i];
                var username = p?.PlayerUsername ?? "";
                playersOrder.Add(username);
                playerSlot[username] = i + 1;
                scores[username] = 0;
            }

            ApplyPlayerToSlot(1, playersOrder.ElementAtOrDefault(0));
            ApplyPlayerToSlot(2, playersOrder.ElementAtOrDefault(1));
            ApplyPlayerToSlot(3, playersOrder.ElementAtOrDefault(2));
            ApplyPlayerToSlot(4, playersOrder.ElementAtOrDefault(3));
        }

        private void ApplyPlayerToSlot(int slot, string username)
        {
            FrameworkElement container = null;
            switch (slot)
            {
                case 1: container = borderKey1.Parent as FrameworkElement; break;
                case 2: container = borderKey2.Parent as FrameworkElement; break;
                case 3: container = borderKey3.Parent as FrameworkElement; break;
                case 4: container = borderKey4.Parent as FrameworkElement; break;
            }

            if (string.IsNullOrEmpty(username))
            {
                if (container != null) container.Visibility = Visibility.Collapsed;
                switch (slot)
                {
                    case 1:
                        txtName1.Text = "";
                        txtKey1.Text = "";
                        break;
                    case 2:
                        txtName2.Text = "";
                        txtKey2.Text = "";
                        break;
                    case 3:
                        txtName3.Text = "";
                        txtKey3.Text = "";
                        break;
                    case 4:
                        txtName4.Text = "";
                        txtKey4.Text = "";
                        break;
                }

                lock (sync)
                {
                    currentKeyBySlot.Remove(slot);
                }
            }
            else
            {
                if (container != null) container.Visibility = Visibility.Visible;

                switch (slot)
                {
                    case 1:
                        txtName1.Visibility = Visibility.Visible;
                        imgAvatar1.Visibility = Visibility.Visible;
                        borderKey1.Visibility = Visibility.Visible;
                        txtName1.Text = username;
                        LoadAvatarForSlot(imgAvatar1, username);
                        break;
                    case 2:
                        txtName2.Visibility = Visibility.Visible;
                        imgAvatar2.Visibility = Visibility.Visible;
                        borderKey2.Visibility = Visibility.Visible;
                        txtName2.Text = username;
                        LoadAvatarForSlot(imgAvatar2, username);
                        break;
                    case 3:
                        txtName3.Visibility = Visibility.Visible;
                        imgAvatar3.Visibility = Visibility.Visible;
                        borderKey3.Visibility = Visibility.Visible;
                        txtName3.Text = username;
                        LoadAvatarForSlot(imgAvatar3, username);
                        break;
                    case 4:
                        txtName4.Visibility = Visibility.Visible;
                        imgAvatar4.Visibility = Visibility.Visible;
                        borderKey4.Visibility = Visibility.Visible;
                        txtName4.Text = username;
                        LoadAvatarForSlot(imgAvatar4, username);
                        break;
                }
            }
        }

        private void LoadAvatarForSlot(Ellipse avatar, string username)
        {
            Task.Run(() =>
            {
                try
                {
                    var profileClient = new ProfileManagerClient();
                    var p = profileClient.GetPlayerByUsername(username, includeFriends: false);
                    //string avatarFile = p?.PlayerAvatarPath;
                    try { profileClient.Close(); } catch { profileClient.Abort(); }

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        //SetAvatar(avatar, avatarFile);
                    }));
                }
                catch
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        SetAvatar(avatar, null);
                    }));
                }
            });
        }

        private void StartPreCountdown()
        {
            txtTimer.Text = preCountdown.ToString();

            try
            {
                if (countdownEffectPlayer != null)
                {
                    countdownEffectPlayer.Position = TimeSpan.Zero;
                    countdownEffectPlayer.Play();
                }
            }
            catch { }

            if (amHost && !keysGenerated)
            {
                GenerateAndBroadcastKeysForAllSlots();
                keysGenerated = true;
            }

            preCountdownTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            preCountdownTimer.Tick += (s, e) =>
            {
                preCountdown--;
                if (preCountdown > 0)
                {
                    txtTimer.Text = preCountdown.ToString();
                }
                else
                {
                    preCountdownTimer.Stop();
                    txtTimer.Text = "YA";
                }
            };
            preCountdownTimer.Start();
        }

        private void StartMainGameAfterKeys()
        {
            if (!countdownEffectEnded) return;
            if (gameActive) return;

            remainingSeconds = 20;
            try { audioManager.PlayBackground("sounds/riuvGameMusic.mp3", loop: true); } catch { }
            gameActive = true;
            StartCountdown();
        }

        private void StartCountdown()
        {
            countdownTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            countdownTimer.Tick += CountdownTimer_Tick;
            countdownTimer.Start();
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            remainingSeconds--;
            txtTimer.Text = remainingSeconds.ToString("D2");

            if (remainingSeconds <= 0)
            {
                try { countdownTimer?.Stop(); } catch { }
                KeyDown -= RiuvPage_KeyDown;
                try { audioManager.StopAll(); } catch { }
                LocalFinish();
            }
        }

        private void GenerateAndBroadcastKeysForAllSlots()
        {
            var activeSlots = playerSlot.Values.OrderBy(i => i).ToList();
            var chosen = new HashSet<char>();

            var keysForSlots = new Dictionary<int, char>();

            lock (sync)
            {
                foreach (var slot in activeSlots)
                {
                    char k;
                    int attempts = 0;
                    do
                    {
                        k = possibleKeys[MatchLogic.Rand.Next(possibleKeys.Count)];
                        attempts++;
                    } while (chosen.Contains(k) && attempts < 100);
                    chosen.Add(k);
                    currentKeyBySlot[slot] = k;
                    keysForSlots[slot] = k;
                }
            }

            var parts = keysForSlots.Select(kv => $"{kv.Key}:{kv.Value}");
            var cmd = "GAME_KEYS|" + string.Join("|", parts);

            try
            {
                Task.Run(() => gameClient.SendChatMessage(matchId.ToString(), currentPlayer, cmd));
            }
            catch (Exception ex)
            {
                log.Warn("GenerateAndBroadcastKeysForAllSlots failed to send", ex);
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                ApplyKeysToUi(keysForSlots);
                if (!gameActive && countdownEffectEnded)
                {
                    StartMainGameAfterKeys();
                }
            }));
        }

        private void ApplyKeysToUi(Dictionary<int, char> keysForSlots)
        {
            foreach (var kv in keysForSlots)
            {
                SetKeyForSlot(kv.Key, kv.Value);
            }
            keysGenerated = true;
        }

        private void SetKeyForSlot(int slot, char key)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                switch (slot)
                {
                    case 1: txtKey1.Text = key.ToString(); txtKey1.Foreground = Brushes.Black; break;
                    case 2: txtKey2.Text = key.ToString(); txtKey2.Foreground = Brushes.Black; break;
                    case 3: txtKey3.Text = key.ToString(); txtKey3.Foreground = Brushes.Black; break;
                    case 4: txtKey4.Text = key.ToString(); txtKey4.Foreground = Brushes.Black; break;
                }
            }));
        }

        private async void RiuvPage_KeyDown(object sender, KeyEventArgs e)
        {
            if (!gameActive) return;

            if (remainingSeconds <= 0) return;

            string pressedKey = e.Key.ToString().ToUpper();
            if (pressedKey.Length == 2 && pressedKey.StartsWith("D"))
                pressedKey = pressedKey[1].ToString();

            char pressedChar = pressedKey.Length > 0 ? pressedKey[0] : '\0';

            if (!playerSlot.TryGetValue(currentPlayer, out int slot)) return;

            var cmd = $"GAME_PRESS|{currentPlayer}|{slot}|{pressedChar}";
            try
            {
                await Task.Run(() => gameClient.SendChatMessage(matchId.ToString(), currentPlayer, cmd));
            }
            catch (Exception ex)
            {
                log.Warn("Failed to send GAME_PRESS", ex);
            }
        }

        private void OnChatMessageReceived(string playerName, string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            if (message.StartsWith("GAME_KEYS|"))
            {
                var parts = message.Split('|').Skip(1);
                var map = new Dictionary<int, char>();
                foreach (var p in parts)
                {
                    var kv = p.Split(':');
                    if (kv.Length == 2 && int.TryParse(kv[0], out int slot) && kv[1].Length > 0)
                    {
                        map[slot] = kv[1][0];
                        lock (sync) { currentKeyBySlot[slot] = kv[1][0]; }
                    }
                }

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ApplyKeysToUi(map);
                    StartMainGameAfterKeys();
                }));
                return;
            }

            if (message.StartsWith("GAME_KEY|"))
            {
                var p = message.Substring("GAME_KEY|".Length);
                var kv = p.Split(':');
                if (kv.Length == 2 && int.TryParse(kv[0], out int slot) && kv[1].Length > 0)
                {
                    char key = kv[1][0];
                    lock (sync) { currentKeyBySlot[slot] = key; }
                    SetKeyForSlot(slot, key);
                }
                return;
            }

            if (message.StartsWith("GAME_PRESS|"))
            {
                var parts = message.Split('|');
                if (parts.Length >= 4)
                {
                    var who = parts[1];
                    if (!int.TryParse(parts[2], out int slot)) return;
                    var keyStr = parts[3];
                    char pressed = keyStr.Length > 0 ? keyStr[0] : '\0';

                    if (amHost)
                    {
                        bool ok = false;
                        lock (sync)
                        {
                            if (currentKeyBySlot.TryGetValue(slot, out char expected))
                                ok = (pressed == expected);
                        }

                        if (ok)
                        {
                            lock (sync)
                            {
                                if (!scores.ContainsKey(who)) scores[who] = 0;
                                scores[who]++;
                            }

                            var res = $"GAME_RESULT|{who}|{slot}|OK";
                            var scoreMsg = $"GAME_SCORE|{who}|{scores[who]}";
                            try
                            {
                                Task.Run(() =>
                                {
                                    gameClient.SendChatMessage(matchId.ToString(), currentPlayer, res);
                                    gameClient.SendChatMessage(matchId.ToString(), currentPlayer, scoreMsg);
                                });
                            }
                            catch (Exception ex) { log.Warn("Failed to broadcast GAME_RESULT/score", ex); }

                            Task.Delay(350).ContinueWith(t => GenerateAndBroadcastKeyForSlot(slot));
                        }
                        else
                        {
                            var res = $"GAME_RESULT|{who}|{slot}|ERR";
                            try { Task.Run(() => gameClient.SendChatMessage(matchId.ToString(), currentPlayer, res)); }
                            catch (Exception ex) { log.Warn("Failed to broadcast GAME_RESULT ERR", ex); }

                            Task.Delay(350).ContinueWith(t => GenerateAndBroadcastKeyForSlot(slot));
                        }
                    }

                    return;
                }
            }

            if (message.StartsWith("GAME_RESULT|"))
            {
                var parts = message.Split('|');
                if (parts.Length >= 4)
                {
                    if (int.TryParse(parts[2], out int slot))
                    {
                        var result = parts[3];
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            var border = GetBorderBySlot(slot);
                            FlashBorder(border, result == "OK");
                        }));
                    }
                }
                return;
            }

            if (message.StartsWith("GAME_SCORE|"))
            {
                var parts = message.Split('|');
                if (parts.Length >= 3)
                {
                    var who = parts[1];
                    if (int.TryParse(parts[2], out int sc))
                    {
                        lock (sync) { scores[who] = sc; }
                    }
                }
                return;
            }

            if (message.StartsWith("GAME_FINAL|"))
            {
                var parts = message.Split('|');
                if (parts.Length >= 3)
                {
                    var who = parts[1];
                    if (int.TryParse(parts[2], out int sc))
                    {
                        lock (sync) { finalScoresReceived[who] = sc; }

                        if (localFinished && finalScoresReceived.Count == playersOrder.Count)
                        {
                            if (amHost)
                                HostAggregateAndBroadcastFinals();
                        }
                    }
                }
                return;
            }

            if (message.StartsWith("GAME_FINALS|"))
            {
                var parts = message.Split('|').Skip(1);
                var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var p in parts)
                {
                    var kv = p.Split(':');
                    if (kv.Length == 2 && int.TryParse(kv[1], out int sc))
                        map[kv[0]] = sc;
                }

                lock (sync)
                {
                    foreach (var kv in map) scores[kv.Key] = kv.Value;
                }

                Dispatcher.BeginInvoke(new Action(() => ShowFinalsAndWinner(map)));
                return;
            }

            if (message.StartsWith("GAME_WINNER|"))
            {
                var parts = message.Split('|');
                if (parts.Length >= 2)
                {
                    var payload = parts[1];
                    var kv = payload.Split(':');
                    if (kv.Length == 2 && int.TryParse(kv[1], out int sc))
                    {
                        var winner = kv[0];
                        Dispatcher.BeginInvoke(new Action(() => ViewUtils.OpenNotificationWindow("Ganador", $"{winner} ganó con {sc} pts", Window.GetWindow(this))));
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() => ViewUtils.OpenNotificationWindow("Ganador", $"{payload} ganó", Window.GetWindow(this))));
                    }
                }
                return;
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                AddChatLine(string.IsNullOrEmpty(playerName) ? $"Sistema: {message}" : $"{playerName}: {message}");
            }));
        }

        private void HostAggregateAndBroadcastFinals()
        {
            Dictionary<string, int> finalMap;
            lock (sync) finalMap = new Dictionary<string, int>(finalScoresReceived, StringComparer.OrdinalIgnoreCase);

            var parts = finalMap.Select(kv => $"{kv.Key}:{kv.Value}");
            var cmd = "GAME_FINALS|" + string.Join("|", parts);

            try
            {
                Task.Run(() => gameClient.SendChatMessage(matchId.ToString(), currentPlayer, cmd));
            }
            catch (Exception ex)
            {
                log.Warn("HostAggregateAndBroadcastFinals failed to send GAME_FINALS", ex);
            }

            var winner = DetermineWinner(finalMap);
            int winnerScore = finalMap.ContainsKey(winner) ? finalMap[winner] : 0;
            try
            {
                Task.Run(() => gameClient.SendChatMessage(matchId.ToString(), currentPlayer, $"GAME_WINNER|{winner}:{winnerScore}"));
            }
            catch (Exception ex)
            {
                log.Warn("HostAggregateAndBroadcastFinals failed to send GAME_WINNER", ex);
            }

            Dispatcher.BeginInvoke(new Action(() => ShowFinalsAndWinner(finalMap)));
        }

        private string DetermineWinner(Dictionary<string, int> finalMap)
        {
            if (finalMap == null || finalMap.Count == 0) return string.Empty;
            int max = finalMap.Max(kv => kv.Value);
            var winners = finalMap.Where(kv => kv.Value == max).Select(kv => kv.Key).ToList();
            foreach (var p in playersOrder)
            {
                if (winners.Contains(p)) return p;
            }
            return winners.First();
        }

        private void ShowFinalsAndWinner(Dictionary<string, int> finalMap)
        {
            var ordered = finalMap.OrderByDescending(kv => kv.Value).ToList();
            if (ordered.Count == 0)
            {
                ViewUtils.OpenNotificationWindow("Resultados", "No hay jugadores.", Window.GetWindow(this));
                return;
            }

            int max = ordered.First().Value;
            var winners = ordered.Where(kv => kv.Value == max).Select(kv => kv.Key).ToList();
            string winner;
            if (winners.Count == 1) winner = winners[0];
            else
            {
                winner = string.Join(", ", winners);
            }

            ViewUtils.OpenNotificationWindow("Ganador", $"{winner} ganó con {max} pts", Window.GetWindow(this));
        }

        private void LocalFinish()
        {
            try
            {
                countdownTimer?.Stop();
                KeyDown -= RiuvPage_KeyDown;
                try { audioManager.StopAll(); } catch { }
            }
            catch { }

            localFinished = true;
            gameActive = false;

            int myScore = 0;
            lock (sync)
            {
                scores.TryGetValue(currentPlayer, out myScore);
            }

            try
            {
                Task.Run(() => gameClient.SendChatMessage(matchId.ToString(), currentPlayer, $"GAME_FINAL|{currentPlayer}|{myScore}"));
            }
            catch (Exception ex)
            {
                log.Warn("LocalFinish failed to send GAME_FINAL", ex);
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                txtTimer.Text = "Esperando jugadores...";
            }));

            lock (sync)
            {
                finalScoresReceived[currentPlayer] = myScore;
            }

            if (amHost)
            {
                if (finalScoresReceived.Count == playersOrder.Count)
                    HostAggregateAndBroadcastFinals();
            }
        }

        private void GenerateAndBroadcastKeyForSlot(int slot)
        {
            char newKey;
            int attempts = 0;
            lock (sync)
            {
                var used = new HashSet<char>(currentKeyBySlot.Values);
                do
                {
                    newKey = possibleKeys[MatchLogic.Rand.Next(possibleKeys.Count)];
                    attempts++;
                } while (used.Contains(newKey) && attempts < 100);

                currentKeyBySlot[slot] = newKey;
            }

            var cmd = $"GAME_KEY|{slot}:{newKey}";
            try { Task.Run(() => gameClient.SendChatMessage(matchId.ToString(), currentPlayer, cmd)); }
            catch (Exception ex) { log.Warn("Failed to broadcast GAME_KEY", ex); }

            SetKeyForSlot(slot, newKey);
        }

        private Border GetBorderBySlot(int slot)
        {
            switch (slot)
            {
                case 1: return borderKey1;
                case 2: return borderKey2;
                case 3: return borderKey3;
                case 4: return borderKey4;
            }
            return null;
        }

        private async void FlashBorder(Border border, bool ok)
        {
            if (border == null) return;
            try
            {
                var flashBrush = ok ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.IndianRed);
                var previousBackground = border.Background;
                var keyText = GetTextBlockForBorder(border);
                var previousForeground = keyText?.Foreground;

                border.Background = flashBrush;
                if (keyText != null) keyText.Foreground = Brushes.White;

                await Task.Delay(100);

                if (border.Background == flashBrush) border.Background = previousBackground;
                if (keyText != null && keyText.Foreground == Brushes.White) keyText.Foreground = previousForeground ?? Brushes.Black;
            }
            catch (Exception ex)
            {
                log.Warn("FlashBorder failed", ex);
            }
        }

        private TextBlock GetTextBlockForBorder(Border border)
        {
            if (border == null) return null;
            if (border == borderKey1) return txtKey1;
            if (border == borderKey2) return txtKey2;
            if (border == borderKey3) return txtKey3;
            if (border == borderKey4) return txtKey4;
            return null;
        }

        private void UpdateScoreInUi(string username, int sc)
        {
            if (playerSlot.TryGetValue(username, out int slot))
            {
                switch (slot)
                {
                    case 1: txtName1.Text = username; break;
                    case 2: txtName2.Text = username; break;
                    case 3: txtName3.Text = username; break;
                    case 4: txtName4.Text = username; break;
                }
            }
        }

        private void SetAvatar(Ellipse avatar, string avatarFile)
        {
            try
            {
                string baseDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;

                string path = null;
                if (!string.IsNullOrEmpty(avatarFile))
                {
                    if (Path.IsPathRooted(avatarFile) && File.Exists(avatarFile))
                        path = avatarFile;
                    else
                    {
                        string candidate = Path.Combine(baseDir, "avatars", Path.GetFileName(avatarFile));
                        if (File.Exists(candidate)) path = candidate;
                    }
                }

                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    path = Path.Combine(baseDir, "Images", "defaultAvatar.png");

                avatar.Fill = new ImageBrush(new BitmapImage(new Uri(path)));
            }
            catch (Exception ex)
            {
                log.Error("RiuvPage.SetAvatar", ex);
            }
        }

        private void AddChatLine(string text)
        {
            log.Info(text);
        }
    }
}