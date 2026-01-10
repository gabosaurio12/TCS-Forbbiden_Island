using Forbbiden.Client.GameManager;
using Forbbiden.Client.logic;
using Forbbiden.Client.MatchManager;
using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.view.games;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Path = System.IO.Path;

namespace Forbbiden.Client.view
{
    public partial class LobbyPage : Page
    {
        private bool isClosing = false;
        private bool isLoaded = false;
        private int countdownToken = 0;

        private string inviteCode;
        private bool isPrivateMatch = false;
        private bool inviteFetched = false;
        private bool inviteLoading = false;
        private static readonly ILog Log = LogManager.GetLogger(typeof(LobbyPage));
        private bool kickedNotified = false;
        private DispatcherTimer Timer;
        private GameManagerClient GameClient;
        private GameServiceCallback Callback;

        private readonly int MatchId;
        private readonly string CurrentPlayer;

        private readonly HashSet<string> PendingChatEcho = new HashSet<string>();
        private readonly object PendingLock = new object();

        private List<string> PreviousPlayers = new List<string>();

        private readonly Dictionary<string, bool> ReadyStates = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private readonly object ReadyLock = new object();

        private readonly string[] SlotUser = new string[4];

        private string MatchHost = null;

        private DispatcherTimer StartCountdownTimer;
        private int CountdownValue;

        private Action<GameManager.PlayerInfo[]> PlayersUpdatedHandler;
        private Action<string, string> ChatMessageHandler;
        private Action GameStartingHandler;
        private Action<string, bool> ReadyStateHandler;
        private Action MatchStartingHandler;

        private bool hostLeftNotified = false;

        private void LobbyPage_Loaded(object sender, RoutedEventArgs e)
        {
            isLoaded = true;
        }

        private void LobbyPage_Unloaded(object sender, RoutedEventArgs e)
        {
            isLoaded = false;
            isClosing = true;
            CancelCountdown();
            UnsubscribeCallbacks();
        }

        public LobbyPage(int matchId, string username, GameManagerClient gameClient, GameServiceCallback callback)
        {
            InitializeComponent();
            MatchNotificationsSingleton.Instance.Subscribe(ClientSession.Username);
            MatchId = matchId;
            CurrentPlayer = username;
            GameClient = gameClient;
            Callback = callback;

            PlayersUpdatedHandler = OnPlayersUpdatedProxy;
            ChatMessageHandler = (p, m) => Dispatcher.BeginInvoke(new Action(() => ShowChatMessageFromServer(p, m)));
            GameStartingHandler = () => Dispatcher.BeginInvoke(new Action(() => AddChatLine($"{Properties.Resources.system_prefix}: {Properties.Resources.countdown_starting}")));
            ReadyStateHandler = (user, isReady) => Dispatcher.BeginInvoke(new Action(() => OnReadyStateChanged(user, isReady)));
            MatchStartingHandler = () => Dispatcher.BeginInvoke(new Action(() => OnMatchStarting()));

            if (Callback != null)
            {
                UnsubscribeCallbacks();
                SubscribeCallbacks();
            }

            this.Loaded += LobbyPage_Loaded;
            this.Unloaded += LobbyPage_Unloaded;

            InitializePlayerUI();
            _ = LoadMatchInfoAsync();
            LoadInitialPlayers();
            StartClock();

            this.Loaded += (s, e) =>
            {
                this.Focusable = true;
                this.Focus();
                this.PreviewKeyDown += LobbyPage_PreviewKeyDown;
                txtChatPrefix.Text = $"{CurrentPlayer}:";
                txtChatMessage.Focus();
            };
        }

        private void ShowGameStartingNotification()
        {
            /*string title = Properties.Resources.game_starting_title;
            string message = Properties.Resources.game_starting_message;
            ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(this));*/
        }

        private void SubscribeCallbacks()
        {
            Callback.PlayersUpdated += PlayersUpdatedHandler;
            Callback.ChatMessageReceived += ChatMessageHandler;
            Callback.GameStarting += GameStartingHandler;
            Callback.ReadyStateChanged += ReadyStateHandler;
            Callback.MatchStarting += MatchStartingHandler;
        }

        private void UnsubscribeCallbacks()
        {
            Callback.PlayersUpdated -= PlayersUpdatedHandler;
            Callback.ChatMessageReceived -= ChatMessageHandler;
            Callback.GameStarting -= GameStartingHandler;
            Callback.ReadyStateChanged -= ReadyStateHandler;
            Callback.MatchStarting -= MatchStartingHandler;
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

        private async Task EnsureInviteVisibleAsync(int playerCount)
        {
            if (!isPrivateMatch)
            {
                InvitePanel.Visibility = Visibility.Collapsed;
                return;
            }

            bool amHost = !string.IsNullOrEmpty(MatchHost) &&
                          string.Equals(MatchHost, CurrentPlayer, StringComparison.OrdinalIgnoreCase);

            if (!amHost && playerCount < 2)
            {
                InvitePanel.Visibility = Visibility.Collapsed;
                return;
            }

            if (inviteFetched || inviteLoading)
            {
                InvitePanel.Visibility = Visibility.Visible;
                return;
            }

            inviteLoading = true;
            try
            {
                await LoadInviteCodeAsync();
            }
            finally
            {
                inviteLoading = false;
            }
        }

        private async Task LoadMatchInfoAsync()
        {
            try
            {
                var matchClient = new MatchManagerClient();
                MatchManager.Match match = null;
                try
                {
                    match = await Task.Run(() => matchClient.GetMatchById(MatchId));
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

                if (match != null)
                {
                    isPrivateMatch = string.Equals(match.Visibility, "Private", StringComparison.OrdinalIgnoreCase);

                    int initialCount = match.Players?.Count() ?? 0;
                    _ = EnsureInviteVisibleAsync(initialCount);
                }
            }
            catch (Exception ex)
            {
                Log.Error("LobbyPage.LoadMatchInfoAsync", ex);
            }

            UpdateReadyButtonText();
        }

        private async Task LoadInviteCodeAsync()
        {
            try
            {
                var mClient = new MatchManagerClient();
                string code = null;
                try
                {
                    code = await Task.Run(() => mClient.GetInviteCode(MatchId));
                }
                finally
                {
                    try { mClient.Close(); } catch { mClient.Abort(); }
                }

                inviteCode = code;
                InvitePanel.Visibility = Visibility.Visible;
                txtInviteCode.Text = !string.IsNullOrWhiteSpace(inviteCode)
                    ? inviteCode
                    : Properties.Resources.invite_code_not_available;
                inviteFetched = true;
            }
            catch (Exception ex)
            {
                Log.Warn("LoadInviteCodeAsync failed", ex);
                InvitePanel.Visibility = Visibility.Visible;
                txtInviteCode.Text = Properties.Resources.invite_code_not_available;
            }
        }

        private void TxtChatMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            e.Handled = true;

            var msg = (txtChatMessage.Text ?? "").Trim();
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
                    if (isLoaded)
                    {
                        Dispatcher.BeginInvoke(new Action(() => AddChatLine($"{Properties.Resources.system_prefix}: {Properties.Resources.chat_send_error}")));
                    }
                }
            });

            txtChatMessage.Text = "";
            txtChatMessage.Focus();
        }

        private void ShowChatMessageFromServer(string playerName, string message)
        {
            if (isClosing) return;
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

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (isClosing || !isLoaded) return;

                    if (string.IsNullOrEmpty(playerName) ||
                        string.Equals(playerName, "System", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(playerName, "Sistema", StringComparison.OrdinalIgnoreCase))
                    {
                        AddChatLine($"{Properties.Resources.system_prefix}: {message}");
                    }
                    else
                    {
                        AddChatLine($"{playerName}: {message}");
                    }
                }));
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
                if (lstChatHistory == null)
                {
                    return;
                }
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

            AddChatLine($"{Properties.Resources.system_prefix}: {Properties.Resources.lobby_welcome}");
        }

        private void LoadInitialPlayers()
        {
            try
            {
                var serverPlayers = GameClient.GetPlayers(MatchId.ToString());

                PreviousPlayers = serverPlayers?
                    .Select(p => p?.PlayerUsername)
                    .Where(u => !string.IsNullOrEmpty(u))
                    .ToList()
                    ?? new List<string>();

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

        private Window SafeGetWindow()
        {
            return isLoaded ? Window.GetWindow(this) : null;
        }

        private void CleanupAndReturnToMain()
        {
            isClosing = true;
            CancelCountdown();
            UnsubscribeCallbacks();

            try
            {
                if (GameClient != null)
                {
                    try { GameClient.Close(); }
                    catch { GameClient.Abort(); }
                }
            }
            catch { }

            NavigationService?.Navigate(new MainPage());
        }

        private void OnPlayersUpdatedProxy(GameManager.PlayerInfo[] players)
        {
            if (isClosing) return;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (isClosing || !isLoaded) return;

                try
                {
                    var current = players?.Select(p => p?.PlayerUsername ?? "").Where(u => !string.IsNullOrEmpty(u)).ToList() ?? new List<string>();

                    var added = current.Except(PreviousPlayers).ToList();
                    var removed = PreviousPlayers.Except(current).ToList();

                    foreach (var a in added) AddChatLine($"{Properties.Resources.system_prefix}: {string.Format(Properties.Resources.player_joined, a)}");
                    foreach (var r in removed)
                    {
                        AddChatLine($"{Properties.Resources.system_prefix}: {string.Format(Properties.Resources.player_left, r)}");
                        CancelCountdown();
                        lock (ReadyLock) { ReadyStates.Remove(r); }
                    }

                    PreviousPlayers = current;

                    bool hostPresent = string.IsNullOrEmpty(MatchHost) || current.Any(u => string.Equals(u, MatchHost, StringComparison.OrdinalIgnoreCase));
                    if (!hostPresent && !hostLeftNotified)
                    {
                        hostLeftNotified = true;
                        var wnd = SafeGetWindow();
                        if (wnd != null)
                            ViewUtils.OpenNotificationWindow(Properties.Resources.host_left_title, Properties.Resources.host_left_message, wnd);
                        CleanupAndReturnToMain();
                        return;
                    }

                    if (!kickedNotified && !current.Any(u => string.Equals(u, CurrentPlayer, StringComparison.OrdinalIgnoreCase)))
                    {
                        kickedNotified = true;
                        CancelCountdown();
                        var wnd = SafeGetWindow();
                        if (wnd != null)
                            ViewUtils.OpenNotificationWindow(Properties.Resources.kicked_title, Properties.Resources.kicked_message, wnd);
                        CleanupAndReturnToMain();
                        return;
                    }

                    var currentCount = current.Count;
                    _ = EnsureInviteVisibleAsync(currentCount);

                    UpdateSlotsFromPlayerInfos(players);

                    UpdateReadyButtonText();
                }
                catch (Exception ex)
                { 
                    Log.Warn("Error OnPlayersUpdatedProxy", ex);
                }
            }));
        }

        private void UpdateSlotsFromPlayerInfos(GameManager.PlayerInfo[] list)
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
                    {
                        ReadyStates[username] = false;
                    }
                }
            }

            lock (ReadyLock)
            {
                foreach (var kv in ReadyStates.ToList())
                {
                    if (!string.IsNullOrEmpty(kv.Key) && kv.Value)
                    {
                        ApplyReadyVisual(kv.Key, true);
                    }
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
            if (isClosing) return;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (isClosing || !isLoaded) return;

                try
                {
                    lock (ReadyLock)
                    {
                        if (!ReadyStates.ContainsKey(username))
                            return;
                        ReadyStates[username] = ready;
                    }

                    ApplyReadyVisual(username, ready);

                    AddChatLine(ready
                        ? $"{Properties.Resources.system_prefix}: {string.Format(Properties.Resources.player_ready, username)}"
                        : $"{Properties.Resources.system_prefix}: {string.Format(Properties.Resources.player_unready, username)}");

                    UpdateReadyButtonText();
                }
                catch (Exception ex) { Log.Warn("OnReadyStateChanged failed", ex); }
            }));
        }

        private void ApplyReadyVisual(string username, bool ready)
        {
            for (int i = 0; i < 4; i++)
            {
                var slotName = SlotUser[i];
                if (!string.IsNullOrEmpty(slotName)
                    && string.Equals(slotName, username, StringComparison.OrdinalIgnoreCase))
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
                case 1: 
                    return imgAvatar1;
                case 2:
                    return imgAvatar2;
                case 3:
                    return imgAvatar3;
                case 4:
                    return imgAvatar4;
            }
            return null;
        }

        private async void BtnReady_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool amHost = !string.IsNullOrEmpty(MatchHost) && string.Equals(
                    MatchHost, CurrentPlayer, 
                    StringComparison.OrdinalIgnoreCase);

                if (amHost && btnReady.Content?.ToString().Equals(Properties.Resources.ready_button_start, StringComparison.OrdinalIgnoreCase) == true)
                {
                    try
                    {
                        await Task.Run(() => GameClient.StartMatch(MatchId.ToString(), CurrentPlayer));
                    }
                    catch (Exception ex)
                    {
                        Log.Warn("StartMatch RPC failed", ex);
                        AddChatLine($"{Properties.Resources.system_prefix}: {Properties.Resources.start_failed}");
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
                    lock (ReadyLock) {
                        ReadyStates[CurrentPlayer] = !newState;
                    }
                    ApplyReadyVisual(CurrentPlayer, !newState);
                    UpdateReadyButtonText();
                    AddChatLine($"{Properties.Resources.system_prefix}: {Properties.Resources.ready_failed}");
                }
            }
            catch (Exception ex)
            {
                Log.Warn("LobbyPage.BtnReady_Click", ex);
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
                        btnReady.Content = Properties.Resources.ready_button_start;
                    else
                        btnReady.Content = Properties.Resources.ready_button_ready;
                }
                else
                {
                    btnReady.Content = ReadyStates.TryGetValue(CurrentPlayer, out bool r) && r
                        ? Properties.Resources.ready_button_unready
                        : Properties.Resources.ready_button_ready;
                }
            }
            catch { }
        }

        private void OnMatchStarting()
        {
            if (isClosing) return;

            try
            {
                AddChatLine($"{Properties.Resources.system_prefix}: {Properties.Resources.countdown_starting}");
                StartCountdown();
            }
            catch (Exception ex) { Log.Warn("OnMatchStarting failed", ex); }
        }

        private void StartCountdown()
        {
            CancelCountdown();
            countdownToken++;

            int localToken = countdownToken;
            CountdownValue = 3;

            AddChatLine($"{Properties.Resources.system_prefix}: {string.Format(Properties.Resources.countdown_number, CountdownValue)}");

            StartCountdownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            StartCountdownTimer.Tick += (_, __) =>
            {
                if (isClosing || localToken != countdownToken) return;
                StartCountdownTimer_Tick();
            };

            StartCountdownTimer.Start();
        }

        private async void OpenBoardAsync(int matchId)
        {
            if (isClosing) return;
            isClosing = true;

            CancelCountdown();
            UnsubscribeCallbacks();

            try
            {
                var mClient = new MatchManagerClient();
                MatchManager.Match match = null;
                try
                {
                    match = await Task.Run(() => mClient.GetMatchById(matchId));
                }
                finally
                {
                    try { mClient.Close(); } catch { mClient.Abort(); }
                }

                if (match != null && isLoaded)
                {
                    
                    NavigationService?.Navigate(new BoardPage(match));
                }
            }
            catch (Exception ex)
            {
                Log.Warn("No se pudo navegar a BoardPage", ex);
            }
        }

        private void StartCountdownTimer_Tick()
        {
            CountdownValue--;

            if (CountdownValue > 0)
            {
                AddChatLine($"{Properties.Resources.system_prefix}: {string.Format(Properties.Resources.countdown_number, CountdownValue)}");
            }
            else
            {
                CancelCountdown();
                AddChatLine($"{Properties.Resources.system_prefix}: {Properties.Resources.countdown_go}");
                OpenBoardAsync(MatchId);
            }
        }

        private void CancelCountdown()
        {
            if (StartCountdownTimer != null)
            {
                if (StartCountdownTimer != null)
                {
                    StartCountdownTimer.Stop();
                    StartCountdownTimer = null;
                }
            }
        }

        private void StartAvatarLoad(Ellipse avatar, GameManager.PlayerInfo p)
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

        private static void SetAvatarFromBytesSync(Ellipse avatar, byte[] bytes)
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

        private static string GetLocalPath(string avatarFileName)
        {
            string projectDir = ViewUtils.GetProjectDir();
            string localAvatarsDir = Path.Combine(projectDir, "avatars");
            Directory.CreateDirectory(localAvatarsDir);

            return Path.Combine(localAvatarsDir, avatarFileName);
        }

        private static async Task<string> EnsureAvatarLocalAsync(string avatarFileName)
        {
            if (string.IsNullOrEmpty(avatarFileName))
            {
                return null;
            }

            string localPath = "";
            ProfileManagerClient profileClient = null;
            try
            {
                localPath = GetLocalPath(avatarFileName);

                if (File.Exists(localPath))
                {
                    profileClient = new ProfileManagerClient();
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
            }
            catch (Exception ex)
            {
                Log.Error("LobbyPage.EnsureAvatarLocalAsync", ex);
            }

            try
            {
                profileClient = new ProfileManagerClient();
                var bytes = await profileClient.GetAvatarAsync(avatarFileName);

                if (bytes?.Length > 0)
                {
                    File.WriteAllBytes(localPath, bytes);
                    return localPath;
                }

                profileClient.Close();
            }
            catch (Exception ex)
            {
                Log.Error("LobbyPage.EnsureAvatarLocalAsync", ex);
            }
            finally
            {
                if (profileClient != null && profileClient.State != CommunicationState.Closed)
                {
                    try 
                    {
                        profileClient.Close();
                    }
                    catch
                    { 
                        profileClient.Abort();
                    }
                }
            }

            return null;
        }

        private static void SetAvatar(Ellipse avatar, string avatarFile)
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
                Log.Warn("SetAvatar failed", ex);
            }
        }

        private static string ResolveLocalAvatarPath(string avatarPathOrFileName)
        {
            if (string.IsNullOrEmpty(avatarPathOrFileName))
            {
                return null;
            }

            if (Path.IsPathRooted(avatarPathOrFileName) && File.Exists(avatarPathOrFileName))
            {
                return avatarPathOrFileName;
            }

            string projectDir = Directory.GetParent(
                AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
            var candidate = Path.Combine(projectDir, "avatars", avatarPathOrFileName);
            if (File.Exists(candidate))
            { 
                return candidate;
            }

            return null;
        }
    }
}