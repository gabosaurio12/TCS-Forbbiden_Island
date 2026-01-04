using Forbbiden.Client.logic;
using Forbbiden.Client.GameManager; 
using Forbbiden.Client.MatchManager;
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
using Forbbiden.Client.view.games;

namespace Forbbiden.Client.view
{
    public partial class LobbyPage : Page
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LobbyPage));

        private DispatcherTimer Timer;
        private readonly GameManagerClient GameClient;
        private readonly GameServiceCallback Callback;

        private readonly int MatchId;
        private readonly string CurrentPlayer;

        private readonly HashSet<string> PendingChatEcho = new HashSet<string>();
        private readonly object PendingLock = new object();

        private List<string> PreviousPlayers = new List<string>();

        private readonly Dictionary<string, bool> ReadyStates = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private readonly object ReadyLock = new object();

        private readonly string[] SlotUser = new string[4];

        private int MatchCapacity = 4;
        private string MatchHost = null;

        private DispatcherTimer StartCountdownTimer;
        private int CountdownValue;

        private Action<GameManager.PlayerInfo[]> PlayersUpdatedHandler;
        private Action<string, string> ChatMessageHandler;
        private Action GameStartingHandler;
        private Action<string, bool> ReadyStateHandler;
        private Action MatchStartingHandler;

        public LobbyPage(int matchId, string username, GameManagerClient gameClient, GameServiceCallback callback)
        {
            InitializeComponent();

            this.MatchId = matchId;
            this.CurrentPlayer = username;
            this.GameClient = gameClient;
            this.Callback = callback;

            PlayersUpdatedHandler = OnPlayersUpdatedProxy;
            ChatMessageHandler = (p, m) => Dispatcher.BeginInvoke(new Action(() => ShowChatMessageFromServer(p, m)));
            GameStartingHandler = () => Dispatcher.BeginInvoke(new Action(() => MessageBox.Show("¡La partida está por comenzar!")));
            ReadyStateHandler = (user, isReady) => Dispatcher.BeginInvoke(new Action(() => OnReadyStateChanged(user, isReady)));
            MatchStartingHandler = () => Dispatcher.BeginInvoke(new Action(() => OnMatchStarting()));

            if (this.Callback != null)
            {
                try { this.Callback.PlayersUpdated -= PlayersUpdatedHandler; } catch { }
                try { this.Callback.ChatMessageReceived -= ChatMessageHandler; } catch { }
                try { this.Callback.GameStarting -= GameStartingHandler; } catch { }

                try { this.Callback.ReadyStateChanged -= ReadyStateHandler; } catch { }
                try { this.Callback.MatchStarting -= MatchStartingHandler; } catch { }

                this.Callback.PlayersUpdated += PlayersUpdatedHandler;
                this.Callback.ChatMessageReceived += ChatMessageHandler;
                this.Callback.GameStarting += GameStartingHandler;

                this.Callback.ReadyStateChanged += ReadyStateHandler;
                this.Callback.MatchStarting += MatchStartingHandler;
            }

            InitializePlayerUI();
            _ = LoadMatchInfoAsync();
            LoadInitialPlayers();
            StartClock();

            txtBxChat.GotFocus += TxtChat_GotFocus;
            txtBxChat.KeyDown += TxtChat_KeyDown;

            this.Unloaded += LobbyPage_Unloaded;

            this.Loaded += (s, e) =>
            {
                this.Focusable = true;
                this.Focus();
                this.PreviewKeyDown += LobbyPage_PreviewKeyDown;
            };
        }

        private void LobbyPage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                var leavePage = new LeaveMenuPage(MatchId, CurrentPlayer, GameClient, Callback);
                NavigationService?.Navigate(leavePage);
            }
        }

        private void TxtChat_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!txtBxChat.Text.StartsWith(CurrentPlayer + ":"))
                    txtBxChat.Text = $"{CurrentPlayer}: ";

                txtBxChat.CaretIndex = txtBxChat.Text.Length;
            }
            catch { /* no crítico */ }
        }

        private async Task LoadMatchInfoAsync()
        {
            try
            {
                var mClient = new MatchManagerClient();
                Forbbiden.Client.MatchManager.Match match = null;
                try
                {
                    match = await Task.Run(() => mClient.GetMatchById(MatchId));
                }
                finally
                {
                    try { mClient.Close(); } catch { mClient.Abort(); }
                }

                if (match != null)
                {
                    MatchCapacity = match.Capacity > 0 ? match.Capacity : 4;
                    MatchHost = match.HostUsername;
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Could not load match info", ex);
            }

            UpdateReadyButtonText();
        }

        private void TxtChat_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            e.Handled = true;

            var raw = txtBxChat.Text ?? "";
            var prefix = $"{CurrentPlayer}: ";
            var msg = raw.Replace(prefix, "").Trim();
            if (string.IsNullOrEmpty(msg)) return;

            AddChatLine($"{CurrentPlayer}: {msg}");

            var key = $"{CurrentPlayer}|{msg}";
            lock (PendingLock) PendingChatEcho.Add(key);

            Task.Run(() =>
            {
                try
                {
                    GameClient?.SendChatMessage(MatchId.ToString(), CurrentPlayer, msg);
                }
                catch (Exception ex)
                {
                    Log.Warn("Error enviando mensaje de chat", ex);
                    Dispatcher.BeginInvoke(new Action(() => AddChatLine($"Sistema: Error al enviar mensaje ({ex.Message})")));
                }
            });

            txtBxChat.Text = $"{CurrentPlayer}: ";
            txtBxChat.CaretIndex = txtBxChat.Text.Length;
        }

        private void ShowChatMessageFromServer(string playerName, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message)) return;

                var key = $"{playerName}|{message}";
                lock (PendingLock)
                {
                    if (PendingChatEcho.Remove(key))
                    {
                        return;
                    }
                }

                if (string.Equals(playerName, "System", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(playerName, "Sistema", StringComparison.OrdinalIgnoreCase) ||
                    string.IsNullOrEmpty(playerName))
                {
                    AddChatLine($"Sistema: {message}");
                }
                else
                {
                    AddChatLine($"{playerName}: {message}");
                }
            }
            catch (Exception ex)
            {
                Log.Warn("ShowChatMessageFromServer failed", ex);
            }
        }

        private void AddChatLine(string text)
        {
            try
            {
                if (lstChatHistory == null) return;
                lstChatHistory.Items.Add(text);
                if (lstChatHistory.Items.Count > 0)
                {
                    lstChatHistory.ScrollIntoView(lstChatHistory.Items[lstChatHistory.Items.Count - 1]);
                }
            }
            catch (Exception ex)
            {
                Log.Warn("AddChatLine failed", ex);
            }
        }

        private void InitializePlayerUI()
        {
            txtBkUser1.Text = CurrentPlayer;
            var player = ClientSession.GetPlayer();
            var local = ResolveLocalAvatarPath(player?.PlayerAvatarPath);
            SetAvatar(imgAvatar1, local);

            AddChatLine("Sistema: Bienvenido al lobby");
        }

        private void LoadInitialPlayers()
        {
            try
            {
                var serverPlayers = GameClient.GetPlayers(MatchId.ToString());
                UpdateSlotsFromPlayerInfos(serverPlayers);
            }
            catch (Exception ex)
            {
                Log.Warn("No se pudo obtener jugadores iniciales", ex);
            }
        }

        private void StartClock()
        {
            Timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            Timer.Tick += (s, e) => txtLobbyHour.Text = DateTime.Now.ToString("hh:mm tt");
            Timer.Start();
        }

        private void OnPlayersUpdatedProxy(Forbbiden.Client.GameManager.PlayerInfo[] players)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    var current = players?.Select(p => p?.PlayerUsername ?? "").Where(u => !string.IsNullOrEmpty(u)).ToList() ?? new List<string>();

                    var added = current.Except(PreviousPlayers).ToList();
                    var removed = PreviousPlayers.Except(current).ToList();

                    foreach (var a in added) AddChatLine($"Sistema: {a} se unió a la partida");
                    foreach (var r in removed)
                    {
                        AddChatLine($"Sistema: {r} salió de la partida");
                        CancelCountdown();
                        lock (ReadyLock) { ReadyStates.Remove(r); }
                    }

                    PreviousPlayers = current;

                    UpdateSlotsFromPlayerInfos(players);

                    UpdateReadyButtonText();
                }
                catch (Exception ex) { Log.Warn("Error OnPlayersUpdatedProxy", ex); }
            }));
        }

        private void UpdateSlotsFromPlayerInfos(Forbbiden.Client.GameManager.PlayerInfo[] list)
        {
            ClearSlots();

            if (list == null) return;

            var ordered = list.OrderBy(p => p?.Position ?? 0).ToArray();

            for (int i = 0; i < ordered.Length && i < 4; i++)
            {
                var p = ordered[i];
                var username = p?.PlayerUsername ?? "";
                SlotUser[i] = username;
                switch (i)
                {
                    case 0:
                        txtBkUser1.Text = username;
                        StartAvatarLoad(imgAvatar1, p);
                        break;
                    case 1:
                        txtBkUser2.Text = username;
                        StartAvatarLoad(imgAvatar2, p);
                        break;
                    case 2:
                        txtBkUser3.Text = username;
                        StartAvatarLoad(imgAvatar3, p);
                        break;
                    case 3:
                        txtBkUser4.Text = username;
                        StartAvatarLoad(imgAvatar4, p);
                        break;
                }

                lock (ReadyLock)
                {
                    if (!ReadyStates.ContainsKey(username))
                        ReadyStates[username] = false;
                }
            }

            lock (ReadyLock)
            {
                foreach (var kv in ReadyStates.ToList())
                {
                    if (!string.IsNullOrEmpty(kv.Key) && kv.Value)
                        ApplyReadyVisual(kv.Key, true);
                }
            }

            UpdateReadyButtonText();
        }

        private void ClearSlots()
        {
            txtBkUser1.Text = "";
            txtBkUser2.Text = "";
            txtBkUser3.Text = "";
            txtBkUser4.Text = "";

            SlotUser[0] = SlotUser[1] = SlotUser[2] = SlotUser[3] = null;

            SetAvatar(imgAvatar1, null);
            SetAvatar(imgAvatar2, null);
            SetAvatar(imgAvatar3, null);
            SetAvatar(imgAvatar4, null);
        }

        private void OnReadyStateChanged(string username, bool ready)
        {
            try
            {
                lock (ReadyLock)
                {
                    ReadyStates[username] = ready;
                }

                ApplyReadyVisual(username, ready);

                AddChatLine(ready ? $"Sistema: {username} está listo" : $"Sistema: {username} ya no está listo");

                UpdateReadyButtonText();
            }
            catch (Exception ex) {  Log.Warn("OnReadyStateChanged failed", ex); }
        }

        private void ApplyReadyVisual(string username, bool ready)
        {
            for (int i = 0; i < 4; i++)
            {
                var slotName = SlotUser[i];
                if (!string.IsNullOrEmpty(slotName) && string.Equals(slotName, username, StringComparison.OrdinalIgnoreCase))
                {
                    Ellipse avatarEllipse = GetAvatarEllipseByIndex(i + 1);
                    if (avatarEllipse != null)
                    {
                        if (ready)
                        {
                            avatarEllipse.Stroke = new SolidColorBrush(Colors.LimeGreen);
                            avatarEllipse.StrokeThickness = 6;
                        }
                        else
                        {
                            avatarEllipse.Stroke = null;
                            avatarEllipse.StrokeThickness = 0;
                        }
                    }
                }
            }
        }

        private Ellipse GetAvatarEllipseByIndex(int slot)
        {
            switch (slot)
            {
                case 1: return imgAvatar1;
                case 2: return imgAvatar2;
                case 3: return imgAvatar3;
                case 4: return imgAvatar4;
            }
            return null;
        }

        private async void BtnReady_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool amHost = !string.IsNullOrEmpty(MatchHost) && string.Equals(MatchHost, CurrentPlayer, StringComparison.OrdinalIgnoreCase);

                if (amHost && btnReady.Content?.ToString().Equals("Start", StringComparison.OrdinalIgnoreCase) == true)
                {
                    try
                    {
                        await Task.Run(() => GameClient.StartMatch(MatchId.ToString(), CurrentPlayer));
                    }
                    catch (Exception ex)
                    {
                        Log.Warn("StartMatch RPC failed", ex);
                        AddChatLine("Sistema: No se pudo iniciar la partida (error servidor).");
                    }
                    return;
                }

                bool newState;
                lock (ReadyLock)
                {
                    ReadyStates.TryGetValue(CurrentPlayer, out bool cur);
                    newState = !cur;
                    ReadyStates[CurrentPlayer] = newState;
                }

                ApplyReadyVisual(CurrentPlayer, newState);
                UpdateReadyButtonText();

                try
                {
                    await Task.Run(() => GameClient.SetReady(MatchId.ToString(), CurrentPlayer, newState));
                }
                catch (Exception ex)
                {
                    Log.Warn("SetReady RPC failed", ex);
                    lock (ReadyLock) { ReadyStates[CurrentPlayer] = !newState; }
                    ApplyReadyVisual(CurrentPlayer, !newState);
                    UpdateReadyButtonText();
                    AddChatLine("Sistema: No se pudo cambiar el estado Ready (error servidor).");
                }
            }
            catch (Exception ex)
            {
                Log.Warn("BtnReady_Click failed", ex);
            }
        }

        private void UpdateReadyButtonText()
        {
            try
            {
                bool amHost = !string.IsNullOrEmpty(MatchHost) && string.Equals(MatchHost, CurrentPlayer, StringComparison.OrdinalIgnoreCase);
                int currentPlayers = SlotUser.Count(s => !string.IsNullOrEmpty(s));
                int readyCount;
                lock (ReadyLock) { readyCount = ReadyStates.Count(kv => kv.Value && !string.IsNullOrEmpty(kv.Key)); }
                bool allPresentReady = (currentPlayers > 0 && readyCount == currentPlayers);

                if (amHost)
                {
                    if (currentPlayers >= 2 && allPresentReady)
                        btnReady.Content = "Start";
                    else
                        btnReady.Content = "Ready";
                }
                else
                {
                    btnReady.Content = ReadyStates.TryGetValue(CurrentPlayer, out bool r) && r ? "Unready" : "Ready";
                }
            }
            catch { /* ignore */ }
        }

        private void OnMatchStarting()
        {
            try
            {
                AddChatLine("Sistema: Iniciando cuenta regresiva...");
                StartCountdown();
            }
            catch (Exception ex) { Log.Warn("OnMatchStarting failed", ex); }
        }

        private void StartCountdown()
        {
            CancelCountdown(); 
            CountdownValue = 3;
            AddChatLine($"Sistema: Comenzando en {CountdownValue}...");
            StartCountdownTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            StartCountdownTimer.Tick += StartCountdownTimer_Tick;
            StartCountdownTimer.Start();
        }

        private void StartCountdownTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                CountdownValue--;
                if (CountdownValue > 0)
                {
                    AddChatLine($"Sistema: {CountdownValue}...");
                }
                else
                {
                    CancelCountdown();
                    AddChatLine("Sistema: ¡Comenzando partida!");
                    try
                    {
                        NavigationService?.Navigate(new RiuvPage());
                    }
                    catch (Exception ex)
                    {
                        Log.Warn("Could not navigate to game page", ex);
                    }
                }
            }
            catch (Exception ex) { Log.Warn("StartCountdownTimer_Tick failed", ex); }
        }

        private void CancelCountdown()
        {
            try
            {
                if (StartCountdownTimer != null)
                {
                    StartCountdownTimer.Stop();
                    StartCountdownTimer.Tick -= StartCountdownTimer_Tick;
                    StartCountdownTimer = null;
                }
            }
            catch { }
        }

        private void LobbyPage_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Callback != null)
                {
                    try { Callback.PlayersUpdated -= PlayersUpdatedHandler; } catch { }
                    try { Callback.ChatMessageReceived -= ChatMessageHandler; } catch { }
                    try { Callback.GameStarting -= GameStartingHandler; } catch { }
                    try { Callback.ReadyStateChanged -= ReadyStateHandler; } catch { }
                    try { Callback.MatchStarting -= MatchStartingHandler; } catch { }
                }
            }
            catch { }
        }

        private void StartAvatarLoad(Ellipse avatar, Forbbiden.Client.GameManager.PlayerInfo p)
        {
            try
            {
                if (p?.AvatarBytes != null && p.AvatarBytes.Length > 0)
                {
                    SetAvatarFromBytesSync(avatar, p.AvatarBytes);
                    return;
                }

                if (!string.IsNullOrEmpty(p?.AvatarFileName))
                {
                    string local = ResolveLocalAvatarPath(p.AvatarFileName);
                    if (!string.IsNullOrEmpty(local) && File.Exists(local))
                    {
                        SetAvatar(avatar, local);
                        return;
                    }

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var downloadedPath = await EnsureAvatarLocalAsync(p.AvatarFileName);
                            if (!string.IsNullOrEmpty(downloadedPath))
                            {
                                _ = Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    SetAvatar(avatar, downloadedPath);
                                }));
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Warn("Error descargando avatar " + p?.AvatarFileName, ex);
                        }
                    });

                    SetAvatar(avatar, null);
                    return;
                }

                SetAvatar(avatar, null);
            }
            catch (Exception ex)
            {
                            Log.Warn("StartAvatarLoad failed", ex);
                SetAvatar(avatar, null);
            }
        }

        private void SetAvatarFromBytesSync(Ellipse avatar, byte[] bytes)
        {
            try
            {
                using (var ms = new MemoryStream(bytes))
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = ms;
                    bmp.EndInit();
                    bmp.Freeze();
                    avatar.Fill = new ImageBrush(bmp);
                }
            }
            catch (Exception ex)
            {
                Log.Warn("SetAvatarFromBytesSync failed", ex);
                SetAvatar(avatar, null);
            }
        }

        private async Task<string> EnsureAvatarLocalAsync(string avatarFileName)
        {
            if (string.IsNullOrEmpty(avatarFileName)) return null;

            try
            {
                string projectDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
                string localAvatarsDir = Path.Combine(projectDir, "avatars");
                if (!Directory.Exists(localAvatarsDir)) Directory.CreateDirectory(localAvatarsDir);

                string localPath = Path.Combine(localAvatarsDir, avatarFileName);
                if (File.Exists(localPath)) return localPath;

                try
                {
                    var profileClient = new ProfileManagerClient();
                    try
                    {
                        var bytes = await profileClient.GetAvatarAsync(avatarFileName);
                        if (bytes != null && bytes.Length > 0)
                        {
                            File.WriteAllBytes(localPath, bytes);
                            return localPath;
                        }
                    }
                    finally
                    {
                        try { profileClient.Close(); } catch { profileClient.Abort(); }
                    }
                }
                catch (Exception ex)
                {
                                    Log.Warn("Error descargando avatar desde ProfileManager", ex);
                }
            }
            catch (Exception ex)
            {
                Log.Warn("EnsureAvatarLocalAsync failed", ex);
            }

            return null;
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
                    Log .Warn("SetAvatar failed", ex);
            }
        }

        private string ResolveLocalAvatarPath(string avatarPathOrFileName)
        {
            try
            {
                if (string.IsNullOrEmpty(avatarPathOrFileName)) return null;

                if (Path.IsPathRooted(avatarPathOrFileName) && File.Exists(avatarPathOrFileName))
                    return avatarPathOrFileName;

                string projectDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
                var candidate = Path.Combine(projectDir, "avatars", avatarPathOrFileName);
                if (File.Exists(candidate)) return candidate;

                return null;
            }
            catch { return null; }
        }
    }
}