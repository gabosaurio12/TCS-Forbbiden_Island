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

        // External services passed from LobbyPage
        private readonly int matchId;
        private readonly string currentPlayer;
        private readonly GameManagerClient gameClient;
        private readonly GameServiceCallback callback;

        private DispatcherTimer countdownTimer;
        private DispatcherTimer preCountdownTimer;

        private int remainingSeconds = 15;
        private int preCountdown = 3;

        // players and mapping
        private readonly List<string> playersOrder = new List<string>(); // usernames in positions 0..n-1
        private readonly Dictionary<string, int> playerSlot = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase); // username -> slot index (1..4)
        private readonly Dictionary<string, int> scores = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // key generation & sync
        private readonly List<char> possibleKeys;
        private char currentKey;
        private bool amHost = false;

        // audio
        private AudioManager audioManager;

        // lock for thread-safety
        private readonly object sync = new object();

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

            // subscribe to callbacks
            if (this.callback != null)
            {
                // ensure we don't double-subscribe
                try { this.callback.PlayersUpdated -= OnPlayersUpdatedProxy; } catch { }
                try { this.callback.ChatMessageReceived -= OnChatMessageReceived; } catch { }

                this.callback.PlayersUpdated += OnPlayersUpdatedProxy;
                this.callback.ChatMessageReceived += OnChatMessageReceived;
            }

            Loaded += RiuvPage_Loaded;
            Unloaded += RiuvPage_Unloaded;

            // keyboard
            Focusable = true;
            Focus();
            KeyDown += RiuvPage_KeyDown;
        }

        private void RiuvPage_Loaded(object sender, RoutedEventArgs e)
        {
            // load players snapshot and determine host
            RefreshPlayersFromServer();

            // Determine if we are host (matchHost from MatchManager is available through MatchManager.GetMatchById,
            // but Lobby already had it — we do a best-effort: if first player in list equals currentPlayer -> host)
            if (playersOrder.Count > 0 && string.Equals(playersOrder[0], currentPlayer, StringComparison.OrdinalIgnoreCase))
                amHost = true;
            else
                amHost = false;

            StartPreCountdown();
            Focus();
        }

        private void RiuvPage_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // unsubscribes
                if (callback != null)
                {
                    try { this.callback.PlayersUpdated -= OnPlayersUpdatedProxy; } catch { }
                    try { this.callback.ChatMessageReceived -= OnChatMessageReceived; } catch { }
                }
            }
            catch { }

            audioManager?.Dispose();
            audioManager = null;

            countdownTimer?.Stop();
            preCountdownTimer?.Stop();

            KeyDown -= RiuvPage_KeyDown;
        }

        // -------------------- Players --------------------

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

        // Called when server sends players update via callback
        private void OnPlayersUpdatedProxy(PlayerInfo[] players)
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

        private void UpdateSlotsFromPlayerInfos(PlayerInfo[] list)
        {
            // Build ordered players present
            var ordered = (list ?? new PlayerInfo[0]).OrderBy(p => p?.Position ?? 0).ToArray();
            playersOrder.Clear();
            playerSlot.Clear();
            scores.Clear();

            for (int i = 0; i < ordered.Length && i < 4; i++)
            {
                var p = ordered[i];
                var username = p?.PlayerUsername ?? "";
                playersOrder.Add(username);
                playerSlot[username] = i + 1; // slot 1..4
                scores[username] = 0;
            }

            // hide unused slots and populate used ones
            ApplyPlayerToSlot(1, playersOrder.ElementAtOrDefault(0));
            ApplyPlayerToSlot(2, playersOrder.ElementAtOrDefault(1));
            ApplyPlayerToSlot(3, playersOrder.ElementAtOrDefault(2));
            ApplyPlayerToSlot(4, playersOrder.ElementAtOrDefault(3));
        }

        private void ApplyPlayerToSlot(int slot, string username)
        {
            // show/hide slot content based on username null or not
            switch (slot)
            {
                case 1:
                    if (string.IsNullOrEmpty(username))
                    {
                        txtName1.Visibility = Visibility.Collapsed;
                        imgAvatar1.Visibility = Visibility.Collapsed;
                        borderKey1.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        txtName1.Visibility = Visibility.Visible;
                        imgAvatar1.Visibility = Visibility.Visible;
                        borderKey1.Visibility = Visibility.Visible;
                        txtName1.Text = $"{username} (0)";
                        LoadAvatarForSlot(imgAvatar1, username);
                    }
                    break;
                case 2:
                    if (string.IsNullOrEmpty(username))
                    {
                        txtName2.Visibility = Visibility.Collapsed;
                        imgAvatar2.Visibility = Visibility.Collapsed;
                        borderKey2.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        txtName2.Visibility = Visibility.Visible;
                        imgAvatar2.Visibility = Visibility.Visible;
                        borderKey2.Visibility = Visibility.Visible;
                        txtName2.Text = $"{username} (0)";
                        LoadAvatarForSlot(imgAvatar2, username);
                    }
                    break;
                case 3:
                    if (string.IsNullOrEmpty(username))
                    {
                        txtName3.Visibility = Visibility.Collapsed;
                        imgAvatar3.Visibility = Visibility.Collapsed;
                        borderKey3.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        txtName3.Visibility = Visibility.Visible;
                        imgAvatar3.Visibility = Visibility.Visible;
                        borderKey3.Visibility = Visibility.Visible;
                        txtName3.Text = $"{username} (0)";
                        LoadAvatarForSlot(imgAvatar3, username);
                    }
                    break;
                case 4:
                    if (string.IsNullOrEmpty(username))
                    {
                        txtName4.Visibility = Visibility.Collapsed;
                        imgAvatar4.Visibility = Visibility.Collapsed;
                        borderKey4.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        txtName4.Visibility = Visibility.Visible;
                        imgAvatar4.Visibility = Visibility.Visible;
                        borderKey4.Visibility = Visibility.Visible;
                        txtName4.Text = $"{username} (0)";
                        LoadAvatarForSlot(imgAvatar4, username);
                    }
                    break;
            }
        }

        private void LoadAvatarForSlot(Ellipse avatar, string username)
        {
            // Try to get avatar locally from ProfileManager; if not found, use default
            Task.Run(() =>
            {
                try
                {
                    var profileClient = new ProfileManagerClient();
                    var p = profileClient.GetPlayerByUsername(username, includeFriends: false);
                    string avatarFile = p?.PlayerAvatarPath;
                    try { profileClient.Close(); } catch { profileClient.Abort(); }

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        SetAvatar(avatar, avatarFile);
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

        // -------------------- Countdown and game start --------------------

        private void StartPreCountdown()
        {
            txtTimer.Text = preCountdown.ToString();
            audioManager.PlayEffect("sounds/gameCountdown.mp3");

            preCountdownTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            preCountdownTimer.Tick += async (s, e) =>
            {
                preCountdown--;

                if (preCountdown > 0)
                {
                    txtTimer.Text = preCountdown.ToString();
                }
                else if (preCountdown == 0)
                {
                    txtTimer.Text = "YA";
                }
                else
                {
                    preCountdownTimer.Stop();
                    txtTimer.Text = remainingSeconds.ToString("D2");
                    audioManager.PlayBackground("sounds/riuvGameMusic.mp3", loop: true);

                    // After pre-countdown, if host -> generate and broadcast first key
                    StartMainGame();
                }
            };
            preCountdownTimer.Start();
        }

        private void StartMainGame()
        {
            StartCountdown();

            // Host will generate and broadcast the first key
            if (amHost)
            {
                GenerateAndBroadcastKey();
            }
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
                countdownTimer.Stop();
                KeyDown -= RiuvPage_KeyDown;
                audioManager.StopAll();
                ShowGameEnd();
            }
        }

        private void ShowGameEnd()
        {
            // simple scoreboard message
            var orderedScores = scores.OrderByDescending(kv => kv.Value).ToList();
            string msg = "Resultado final:\n" + string.Join("\n", orderedScores.Select(kv => $"{kv.Key}: {kv.Value}"));
            MessageBox.Show(msg, "Resultados");
        }

        // -------------------- Key generation & messaging --------------------

        private void GenerateAndBroadcastKey()
        {
            lock (sync)
            {
                currentKey = possibleKeys[MatchLogic.Rand.Next(possibleKeys.Count)];
            }

            // broadcast via chat command GAME_KEY|<char>
            var cmd = $"GAME_KEY|{currentKey}";
            try
            {
                Task.Run(() => gameClient.SendChatMessage(matchId.ToString(), currentPlayer, cmd));
            }
            catch (Exception ex)
            {
                log.Warn("GenerateAndBroadcastKey failed to send", ex);
            }

            // Also set own display immediately
            SetDisplayedKeyForAll(currentKey);
        }

        private void SetDisplayedKeyForAll(char key)
        {
            // Update the primary displayed key(s). We'll set the first visible slot txtKeyX to show key.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                txtKey1.Text = key.ToString();
                txtKey2.Text = key.ToString();
                txtKey3.Text = key.ToString();
                txtKey4.Text = key.ToString();

                // reset colors
                txtKey1.Foreground = Brushes.Black;
                txtKey2.Foreground = Brushes.Black;
                txtKey3.Foreground = Brushes.Black;
                txtKey4.Foreground = Brushes.Black;
            }));
        }

        // -------------------- Input handling --------------------

        private async void RiuvPage_KeyDown(object sender, KeyEventArgs e)
        {
            if (remainingSeconds <= 0) return;

            string pressedKey = e.Key.ToString().ToUpper();
            if (pressedKey.Length == 2 && pressedKey.StartsWith("D"))
                pressedKey = pressedKey[1].ToString();

            char pressedChar = pressedKey.Length > 0 ? pressedKey[0] : '\0';

            // send press to host for validation
            var cmd = $"GAME_PRESS|{currentPlayer}|{pressedChar}";
            try
            {
                await Task.Run(() => gameClient.SendChatMessage(matchId.ToString(), currentPlayer, cmd));
            }
            catch (Exception ex)
            {
                log.Warn("Failed to send GAME_PRESS", ex);
            }
        }

        // -------------------- Chat commands processing --------------------

        // This method is subscribed to callback.ChatMessageReceived
        private void OnChatMessageReceived(string playerName, string message)
        {
            // messages format: GAME_KEY|<char>  or GAME_PRESS|username|char  or GAME_RESULT|username|OK/ERR  or GAME_SCORE|username|n
            if (string.IsNullOrEmpty(message)) return;

            if (message.StartsWith("GAME_KEY|"))
            {
                var parts = message.Split('|');
                if (parts.Length >= 2 && parts[1].Length > 0)
                {
                    char key = parts[1][0];
                    currentKey = key;
                    SetDisplayedKeyForAll(key);
                }
                return;
            }

            if (message.StartsWith("GAME_PRESS|"))
            {
                var parts = message.Split('|');
                if (parts.Length >= 3)
                {
                    var who = parts[1];
                    var keyStr = parts[2];
                    char pressed = keyStr.Length > 0 ? keyStr[0] : '\0';

                    // If I am host, validate and broadcast result
                    if (amHost)
                    {
                        bool ok;
                        lock (sync) { ok = pressed == currentKey; }

                        // If OK increment host's authoritative score and broadcast GAME_RESULT and GAME_SCORE
                        if (ok)
                        {
                            // update score
                            lock (sync)
                            {
                                if (!scores.ContainsKey(who)) scores[who] = 0;
                                scores[who]++;
                            }

                            // broadcast result and score
                            var res = $"GAME_RESULT|{who}|OK";
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

                            // generate next key after a short delay to let clients show feedback
                            Task.Delay(250).ContinueWith(t => GenerateAndBroadcastKey());
                        }
                        else
                        {
                            var res = $"GAME_RESULT|{who}|ERR";
                            try { Task.Run(() => gameClient.SendChatMessage(matchId.ToString(), currentPlayer, res)); }
                            catch (Exception ex) { log.Warn("Failed to broadcast GAME_RESULT ERR", ex); }

                            // still generate new key to keep flow
                            Task.Delay(250).ContinueWith(t => GenerateAndBroadcastKey());
                        }
                    }

                    return;
                }
            }

            if (message.StartsWith("GAME_RESULT|"))
            {
                var parts = message.Split('|');
                if (parts.Length >= 3)
                {
                    var who = parts[1];
                    var result = parts[2];
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // flash OK (green) or ERR (red) on that player's key border
                        if (playerSlot.TryGetValue(who, out int slot))
                        {
                            Border border = GetBorderBySlot(slot);
                            if (border != null)
                                FlashBorder(border, result == "OK");
                        }
                    }));
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
                        Dispatcher.BeginInvoke(new Action(() => UpdateScoreInUi(who, sc)));
                    }
                }
                return;
            }

            // fallback: normal chat -> show in chat area
            Dispatcher.BeginInvoke(new Action(() => AddChatLine(string.IsNullOrEmpty(playerName) ? $"Sistema: {message}" : $"{playerName}: {message}")));
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
            try
            {
                var original = border.Background;
                border.Background = ok ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.IndianRed);
                await Task.Delay(350);
                border.Background = original;
            }
            catch { }
        }

        private void UpdateScoreInUi(string username, int sc)
        {
            if (playerSlot.TryGetValue(username, out int slot))
            {
                switch (slot)
                {
                    case 1: txtName1.Text = $"{username} ({sc})"; break;
                    case 2: txtName2.Text = $"{username} ({sc})"; break;
                    case 3: txtName3.Text = $"{username} ({sc})"; break;
                    case 4: txtName4.Text = $"{username} ({sc})"; break;
                }
            }
        }

        // -------------------- Helper UI / Avatar --------------------

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
                log.Warn("SetAvatar failed", ex);
            }
        }

        private void AddChatLine(string text)
        {
            // reuse a chat from Lobby? If you don't have one here, you can show messagebox or ignore.
            // For now, show in console and log.
            log.Info(text);
        }
    }
}