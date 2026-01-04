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

        private DispatcherTimer timer;
        private GameManagerClient gameClient;
        private GameServiceCallback callback;

        private int matchId;
        private string currentPlayer;

        private readonly HashSet<string> pendingChatEcho = new HashSet<string>();
        private readonly object pendingLock = new object();

        private List<string> previousPlayers = new List<string>();

        private readonly Dictionary<string, bool> readyStates = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private readonly object readyLock = new object();

        private readonly string[] slotUser = new string[4];

        private int matchCapacity = 4;
        private string matchHost = null;

        private DispatcherTimer startCountdownTimer;
        private int countdownValue;

        private Action<Forbbiden.Client.GameManager.PlayerInfo[]> playersUpdatedHandler;
        private Action<string, string> chatMessageHandler;
        private Action gameStartingHandler;
        private Action<string, bool> readyStateHandler;
        private Action matchStartingHandler;

        public LobbyPage(int matchId, string username, GameManagerClient gameClient, GameServiceCallback callback)
        {
            InitializeComponent();

            this.matchId = matchId;
            this.currentPlayer = username;
            this.gameClient = gameClient;
            this.callback = callback;

            playersUpdatedHandler = OnPlayersUpdatedProxy;
            chatMessageHandler = (p, m) => Dispatcher.BeginInvoke(new Action(() => ShowChatMessageFromServer(p, m)));
            gameStartingHandler = () => Dispatcher.BeginInvoke(new Action(() => MessageBox.Show("¡La partida está por comenzar!")));
            readyStateHandler = (user, isReady) => Dispatcher.BeginInvoke(new Action(() => OnReadyStateChanged(user, isReady)));
            matchStartingHandler = () => Dispatcher.BeginInvoke(new Action(() => OnMatchStarting()));

            if (this.callback != null)
            {
                try { this.callback.PlayersUpdated -= playersUpdatedHandler; } catch { }
                try { this.callback.ChatMessageReceived -= chatMessageHandler; } catch { }
                try { this.callback.GameStarting -= gameStartingHandler; } catch { }

                try { this.callback.ReadyStateChanged -= readyStateHandler; } catch { }
                try { this.callback.MatchStarting -= matchStartingHandler; } catch { }

                this.callback.PlayersUpdated += playersUpdatedHandler;
                this.callback.ChatMessageReceived += chatMessageHandler;
                this.callback.GameStarting += gameStartingHandler;

                this.callback.ReadyStateChanged += readyStateHandler;
                this.callback.MatchStarting += matchStartingHandler;
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
                var leavePage = new LeaveMenuPage(matchId, currentPlayer, gameClient, callback);
                NavigationService?.Navigate(leavePage);
            }
        }

        private void TxtChat_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!txtBxChat.Text.StartsWith(currentPlayer + ":"))
                    txtBxChat.Text = $"{currentPlayer}: ";

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
                    match = await Task.Run(() => mClient.GetMatchById(matchId));
                }
                finally
                {
                    try { mClient.Close(); } catch { mClient.Abort(); }
                }

                if (match != null)
                {
                    matchCapacity = match.Capacity > 0 ? match.Capacity : 4;
                    matchHost = match.HostUsername;
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
            var prefix = $"{currentPlayer}: ";
            var msg = raw.Replace(prefix, "").Trim();
            if (string.IsNullOrEmpty(msg)) return;

            AddChatLine($"{currentPlayer}: {msg}");

            var key = $"{currentPlayer}|{msg}";
            lock (pendingLock) pendingChatEcho.Add(key);

            Task.Run(() =>
            {
                try
                {
                    gameClient?.SendChatMessage(matchId.ToString(), currentPlayer, msg);
                }
                catch (Exception ex)
                {
                    Log.Warn("Error enviando mensaje de chat", ex);
                    Dispatcher.BeginInvoke(new Action(() => AddChatLine($"Sistema: Error al enviar mensaje ({ex.Message})")));
                }
            });

            txtBxChat.Text = $"{currentPlayer}: ";
            txtBxChat.CaretIndex = txtBxChat.Text.Length;
        }

        private void ShowChatMessageFromServer(string playerName, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message)) return;

                var key = $"{playerName}|{message}";
                lock (pendingLock)
                {
                    if (pendingChatEcho.Remove(key))
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
            txtBkUser1.Text = currentPlayer;
            var player = ClientSession.GetPlayer();
            var local = ResolveLocalAvatarPath(player?.PlayerAvatarPath);
            SetAvatar(imgAvatar1, local);

            AddChatLine("Sistema: Bienvenido al lobby");
        }

        private void LoadInitialPlayers()
        {
            try
            {
                var serverPlayers = gameClient.GetPlayers(matchId.ToString());
                UpdateSlotsFromPlayerInfos(serverPlayers);
            }
            catch (Exception ex)
            {
                Log.Warn("No se pudo obtener jugadores iniciales", ex);
            }
        }

        private void StartClock()
        {
            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, e) => txtLobbyHour.Text = DateTime.Now.ToString("hh:mm tt");
            timer.Start();
        }

        private void OnPlayersUpdatedProxy(Forbbiden.Client.GameManager.PlayerInfo[] players)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    var current = players?.Select(p => p?.PlayerUsername ?? "").Where(u => !string.IsNullOrEmpty(u)).ToList() ?? new List<string>();

                    var added = current.Except(previousPlayers).ToList();
                    var removed = previousPlayers.Except(current).ToList();

                    foreach (var a in added) AddChatLine($"Sistema: {a} se unió a la partida");
                    foreach (var r in removed)
                    {
                        AddChatLine($"Sistema: {r} salió de la partida");
                        CancelCountdown();
                        lock (readyLock) { readyStates.Remove(r); }
                    }

                    previousPlayers = current;

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
                slotUser[i] = username;
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

                lock (readyLock)
                {
                    if (!readyStates.ContainsKey(username))
                        readyStates[username] = false;
                }
            }

            lock (readyLock)
            {
                foreach (var kv in readyStates.ToList())
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

            slotUser[0] = slotUser[1] = slotUser[2] = slotUser[3] = null;

            SetAvatar(imgAvatar1, null);
            SetAvatar(imgAvatar2, null);
            SetAvatar(imgAvatar3, null);
            SetAvatar(imgAvatar4, null);
        }

        private void OnReadyStateChanged(string username, bool ready)
        {
            try
            {
                lock (readyLock)
                {
                    readyStates[username] = ready;
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
                var slotName = slotUser[i];
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
                bool amHost = !string.IsNullOrEmpty(matchHost) && string.Equals(matchHost, currentPlayer, StringComparison.OrdinalIgnoreCase);

                if (amHost && btnReady.Content?.ToString().Equals("Start", StringComparison.OrdinalIgnoreCase) == true)
                {
                    try
                    {
                        await Task.Run(() => gameClient.StartMatch(matchId.ToString(), currentPlayer));
                    }
                    catch (Exception ex)
                    {
                        Log.Warn("StartMatch RPC failed", ex);
                        AddChatLine("Sistema: No se pudo iniciar la partida (error servidor).");
                    }
                    return;
                }

                bool newState;
                lock (readyLock)
                {
                    readyStates.TryGetValue(currentPlayer, out bool cur);
                    newState = !cur;
                    readyStates[currentPlayer] = newState;
                }

                ApplyReadyVisual(currentPlayer, newState);
                UpdateReadyButtonText();

                try
                {
                    await Task.Run(() => gameClient.SetReady(matchId.ToString(), currentPlayer, newState));
                }
                catch (Exception ex)
                {
                    Log.Warn("SetReady RPC failed", ex);
                    lock (readyLock) { readyStates[currentPlayer] = !newState; }
                    ApplyReadyVisual(currentPlayer, !newState);
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
                bool amHost = !string.IsNullOrEmpty(matchHost) && string.Equals(matchHost, currentPlayer, StringComparison.OrdinalIgnoreCase);
                int currentPlayers = slotUser.Count(s => !string.IsNullOrEmpty(s));
                int readyCount;
                lock (readyLock) { readyCount = readyStates.Count(kv => kv.Value && !string.IsNullOrEmpty(kv.Key)); }
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
                    btnReady.Content = readyStates.TryGetValue(currentPlayer, out bool r) && r ? "Unready" : "Ready";
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
            countdownValue = 3;
            AddChatLine($"Sistema: Comenzando en {countdownValue}...");
            startCountdownTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            startCountdownTimer.Tick += StartCountdownTimer_Tick;
            startCountdownTimer.Start();
        }

        private void StartCountdownTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                countdownValue--;
                if (countdownValue > 0)
                {
                    AddChatLine($"Sistema: {countdownValue}...");
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
                if (startCountdownTimer != null)
                {
                    startCountdownTimer.Stop();
                    startCountdownTimer.Tick -= StartCountdownTimer_Tick;
                    startCountdownTimer = null;
                }
            }
            catch { }
        }

        private void LobbyPage_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (callback != null)
                {
                    try { callback.PlayersUpdated -= playersUpdatedHandler; } catch { }
                    try { callback.ChatMessageReceived -= chatMessageHandler; } catch { }
                    try { callback.GameStarting -= gameStartingHandler; } catch { }
                    try { callback.ReadyStateChanged -= readyStateHandler; } catch { }
                    try { callback.MatchStarting -= matchStartingHandler; } catch { }
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
                        var bytes = profileClient.GetAvatar(avatarFileName);
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