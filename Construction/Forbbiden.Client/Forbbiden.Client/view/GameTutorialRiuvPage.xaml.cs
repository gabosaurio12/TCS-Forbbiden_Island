using Forbbiden.Client.GameManager;
using Forbbiden.Client.Logic;
using Forbbiden.Client.MatchManager;
using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.View.Games;
using log4net;
using System;
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

namespace Forbbiden.Client.View
{
    public partial class GameTutorialRiuvPage : Page
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GameTutorialRiuvPage));

        private readonly int matchId;
        private readonly string currentPlayer;
        private readonly GameManagerClient gameClient;
        private readonly GameServiceCallback callback;

        private readonly object readyLock = new object();
        private readonly System.Collections.Generic.Dictionary<string, bool> readyStates = new System.Collections.Generic.Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        private DispatcherTimer startCountdownTimer;
        private int countdownValue;

        private DispatcherTimer hostStartGraceTimer;
        private readonly TimeSpan hostStartGrace = TimeSpan.FromMilliseconds(1500);
        private bool isStarting = false;

        private bool awaitingServerStart = false;
        private DispatcherTimer serverStartFallbackTimer;
        private readonly TimeSpan serverStartFallback = TimeSpan.FromSeconds(5);

        public Action OnTutorialStarted { get; set; }

        private string matchHost;
        private readonly string[] slotUser = new string[4];

        public GameTutorialRiuvPage(int matchId,
                                   string username,
                                   GameManagerClient gameClient,
                                   GameServiceCallback callback)
        {
            InitializeComponent();

            this.matchId = matchId;
            this.currentPlayer = username;
            this.gameClient = gameClient;
            this.callback = callback;

            if (this.callback != null)
            {
                try { this.callback.PlayersUpdated -= OnPlayersUpdatedProxy; } catch { }
                try { this.callback.ReadyStateChanged -= OnReadyStateChangedProxy; } catch { }
                try { this.callback.MatchStarting -= OnMatchStartingProxy; } catch { }

                this.callback.PlayersUpdated += OnPlayersUpdatedProxy;
                this.callback.ReadyStateChanged += OnReadyStateChangedProxy;
                this.callback.MatchStarting += OnMatchStartingProxy;
            }

            this.Loaded += GamTutorialRiuvPage_Loaded;
            this.Unloaded += GamTutorialRiuvPage_Unloaded;

            InitializePlayerUI();
        }

        private void GamTutorialRiuvPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focusable = true;
            this.Focus();
            this.PreviewKeyDown += GamTutorialRiuvPage_PreviewKeyDown;

            try
            {
                mediaTutorial.MediaEnded -= MediaTutorial_MediaEnded;
                mediaTutorial.MediaEnded += MediaTutorial_MediaEnded;

                mediaTutorial.MediaFailed -= MediaTutorial_MediaFailed;
                mediaTutorial.MediaFailed += MediaTutorial_MediaFailed;

                mediaTutorial.MediaOpened -= MediaTutorial_MediaOpened;
                mediaTutorial.MediaOpened += MediaTutorial_MediaOpened;
            }
            catch (Exception ex)
            {
                Log.Warn("No se pudo suscribir a eventos de mediaTutorial", ex);
            }

            try
            {
                SetVideoSource("Videos/Meme.mp4");
            }
            catch (Exception ex)
            {
                Log.Warn("Fallo al intentar cargar video por defecto", ex);
            }

            _ = Task.Run(() => LoadPlayersFromServer());
        }

        private void GamTutorialRiuvPage_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.PreviewKeyDown -= GamTutorialRiuvPage_PreviewKeyDown;

                try { mediaTutorial.MediaEnded -= MediaTutorial_MediaEnded; } catch { }
                try { mediaTutorial.MediaFailed -= MediaTutorial_MediaFailed; } catch { }
                try { mediaTutorial.MediaOpened -= MediaTutorial_MediaOpened; } catch { }

                try { mediaTutorial.Stop(); } catch { }

                if (callback != null)
                {
                    try { this.callback.PlayersUpdated -= OnPlayersUpdatedProxy; } catch { }
                    try { this.callback.ReadyStateChanged -= OnReadyStateChangedProxy; } catch { }
                    try { this.callback.MatchStarting -= OnMatchStartingProxy; } catch { }
                }
            }
            catch { }

            CancelHostGrace();
            CancelServerStartFallback();
            CancelCountdown();
        }

        private void GamTutorialRiuvPage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Enter)
            {
                e.Handled = true;
                TrySetReadyLocal();
            }
        }

        private void TrySetReadyLocal()
        {
            lock (readyLock)
            {
                if (readyStates.TryGetValue(currentPlayer, out bool cur) && cur)
                    return;
                readyStates[currentPlayer] = true;
            }

            ApplyReadyVisual(currentPlayer, true);

            if (gameClient != null)
            {
                Task.Run(() =>
                {
                    try { gameClient.SetReady(matchId.ToString(), currentPlayer, true); }
                    catch (Exception ex) { Log.Warn("SetReady RPC failed", ex); }
                });
            }

            UpdateReadyVisual();
            MaybeScheduleHostStart();
        }

        private void UpdateReadyVisual()
        {
            try
            {
                int currentPlayers = slotUser.Count(s => !string.IsNullOrEmpty(s));
                int readyCount;
                lock (readyLock) { readyCount = readyStates.Count(kv => kv.Value && !string.IsNullOrEmpty(kv.Key)); }
                bool allPresentReady = (currentPlayers > 0 && readyCount == currentPlayers);

                if (!string.IsNullOrEmpty(txtKeyStart?.Text))
                    txtKeyStart.Foreground = allPresentReady ? new SolidColorBrush(Colors.LimeGreen) : new SolidColorBrush(Colors.Black);
            }
            catch { }
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
                            avatarEllipse.StrokeThickness = 4;
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
                case 1: return slotAvatar1;
                case 2: return slotAvatar2;
                case 3: return slotAvatar3;
                case 4: return slotAvatar4;
            }
            return null;
        }

        public void SetVideoSource(string pathOrUri)
        {
            try
            {
                if (string.IsNullOrEmpty(pathOrUri)) return;

                if (Uri.IsWellFormedUriString(pathOrUri, UriKind.Absolute))
                {
                    mediaTutorial.Source = new Uri(pathOrUri, UriKind.Absolute);
                    mediaTutorial.LoadedBehavior = MediaState.Manual;
                    mediaTutorial.IsMuted = false;
                    mediaTutorial.Volume = 1.0;
                    mediaTutorial.Play();
                    return;
                }

                if (pathOrUri.StartsWith("pack://siteoforigin:", StringComparison.OrdinalIgnoreCase) ||
                    pathOrUri.StartsWith("pack://application:", StringComparison.OrdinalIgnoreCase))
                {
                    mediaTutorial.Source = new Uri(pathOrUri, UriKind.Absolute);
                    mediaTutorial.LoadedBehavior = MediaState.Manual;
                    mediaTutorial.IsMuted = false;
                    mediaTutorial.Volume = 1.0;
                    mediaTutorial.Play();
                    return;
                }

                string baseDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
                string candidate = Path.Combine(baseDir, pathOrUri.TrimStart('/', '\\'));
                if (File.Exists(candidate))
                {
                    mediaTutorial.Source = new Uri(candidate);
                    mediaTutorial.LoadedBehavior = MediaState.Manual;
                    mediaTutorial.IsMuted = false;
                    mediaTutorial.Volume = 1.0;
                    mediaTutorial.Play();
                    return;
                }

                if (File.Exists(pathOrUri))
                {
                    mediaTutorial.Source = new Uri(Path.GetFullPath(pathOrUri));
                    mediaTutorial.LoadedBehavior = MediaState.Manual;
                    mediaTutorial.IsMuted = false;
                    mediaTutorial.Volume = 1.0;
                    mediaTutorial.Play();
                    return;
                }

                string siteOriginCandidate = $"pack://siteoforigin:,,,/{pathOrUri.TrimStart('/', '\\')}";
                try
                {
                    mediaTutorial.Source = new Uri(siteOriginCandidate, UriKind.Absolute);
                    mediaTutorial.LoadedBehavior = MediaState.Manual;
                    mediaTutorial.IsMuted = false;
                    mediaTutorial.Volume = 1.0;
                    mediaTutorial.Play();
                    return;
                }
                catch { }

                try
                {
                    mediaTutorial.Source = new Uri(pathOrUri, UriKind.RelativeOrAbsolute);
                    mediaTutorial.LoadedBehavior = MediaState.Manual;
                    mediaTutorial.IsMuted = false;
                    mediaTutorial.Volume = 1.0;
                    mediaTutorial.Play();
                    return;
                }
                catch (Exception ex)
                {
                    Log.Warn("No se pudo crear URI para video: " + pathOrUri, ex);
                }
            }
            catch (Exception ex)
            {
                Log.Warn("SetVideoSource failed", ex);
            }
        }

        private void MediaTutorial_MediaOpened(object sender, RoutedEventArgs e)
        {
            try
            {
                mediaTutorial.IsMuted = false;
                mediaTutorial.Volume = 1.0;
                mediaTutorial.Play();
            }
            catch (Exception ex) { Log.Warn("MediaOpened handler failed", ex); }
        }

        private void MediaTutorial_MediaEnded(object sender, RoutedEventArgs e)
        {
            try
            {
                mediaTutorial.Position = TimeSpan.Zero;
                mediaTutorial.Play();
            }
            catch (Exception ex)
            {
                Log.Warn("MediaEnded handler failed", ex);
            }
        }

        private void MediaTutorial_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            try { Log.Warn($"mediaTutorial MediaFailed. URI={mediaTutorial?.Source} Error={e.ErrorException?.Message}", e.ErrorException); }
            catch (Exception ex) { Log.Warn("MediaFailed handler failed", ex); }
        }

        private void InitializePlayerUI()
        {
            txtSlot1.Text = txtSlot2.Text = txtSlot3.Text = txtSlot4.Text = "";

            SetAvatarEllipse(slotAvatar1, null);
            SetAvatarEllipse(slotAvatar2, null);
            SetAvatarEllipse(slotAvatar3, null);
            SetAvatarEllipse(slotAvatar4, null);
        }

        private void SetAvatarEllipse(Ellipse avatar, string avatarFile)
        {
            try
            {
                if (string.IsNullOrEmpty(avatarFile))
                {
                    avatar.Fill = new SolidColorBrush(Colors.LightGray);
                    return;
                }

                string path = null;
                if (Path.IsPathRooted(avatarFile) && File.Exists(avatarFile))
                    path = avatarFile;
                else
                {
                    string baseDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
                    string candidate = Path.Combine(baseDir, "avatars", Path.GetFileName(avatarFile));
                    if (File.Exists(candidate)) path = candidate;
                }

                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    var bmp = new BitmapImage(new Uri(path));
                    avatar.Fill = new ImageBrush(bmp);
                }
                else
                {
                    avatar.Fill = new SolidColorBrush(Colors.LightGray);
                }
            }
            catch (Exception ex)
            {
                Log.Warn("SetAvatarEllipse failed", ex);
                avatar.Fill = new SolidColorBrush(Colors.LightGray);
            }
        }

        private void LoadPlayersFromServer()
        {
            try
            {
                if (gameClient == null) return;
                var serverPlayers = gameClient.GetPlayers(matchId.ToString());
                Dispatcher.BeginInvoke(new Action(() => UpdateSlotsFromPlayerInfos(serverPlayers)));
            }
            catch (Exception ex)
            {
                Log.Warn("LoadPlayersFromServer failed", ex);
            }
        }

        private void OnPlayersUpdatedProxy(Forbbiden.Client.GameManager.PlayerInfo[] players)
        {
            Dispatcher.BeginInvoke(new Action(() => UpdateSlotsFromPlayerInfos(players)));
        }

        private void UpdateSlotsFromPlayerInfos(Forbbiden.Client.GameManager.PlayerInfo[] list)
        {
            try
            {
                txtSlot1.Text = txtSlot2.Text = txtSlot3.Text = txtSlot4.Text = "";
                slotUser[0] = slotUser[1] = slotUser[2] = slotUser[3] = null;

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
                            txtSlot1.Text = username;
                            StartAvatarLoad(slotAvatar1, p);
                            break;
                        case 1:
                            txtSlot2.Text = username;
                            StartAvatarLoad(slotAvatar2, p);
                            break;
                        case 2:
                            txtSlot3.Text = username;
                            StartAvatarLoad(slotAvatar3, p);
                            break;
                        case 3:
                            txtSlot4.Text = username;
                            StartAvatarLoad(slotAvatar4, p);
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

                _ = Task.Run(async () =>
                {
                    try
                    {
                        var mClient = new MatchManagerClient();
                        Forbbiden.Client.MatchManager.Match match = null;
                        try
                        {
                            match = await Task.Run(() => mClient.GetMatchById(matchId));
                            if (match != null) matchHost = match.HostUsername;
                        }
                        finally
                        {
                            try { mClient.Close(); } catch { mClient.Abort(); }
                        }
                    }
                    catch { }
                    await Dispatcher.BeginInvoke(new Action(() => UpdateReadyVisual()));
                });

                if (startCountdownTimer != null)
                {
                    int currentPlayers = slotUser.Count(s => !string.IsNullOrEmpty(s));
                    if (currentPlayers < 2)
                    {
                        CancelCountdown();
                        awaitingServerStart = false;
                        Log.Info("Countdown cancelado: jugadores desconectados durante el inicio.");
                    }
                }

                MaybeScheduleHostStart();
            }
            catch (Exception ex) { Log.Warn("UpdateSlotsFromPlayerInfos failed", ex); }
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

        private void StartAvatarLoad(Ellipse avatar, Forbbiden.Client.GameManager.PlayerInfo p)
        {
            try
            {
                if (p?.AvatarBytes != null && p.AvatarBytes.Length > 0)
                {
                    using (var ms = new MemoryStream(p.AvatarBytes))
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.StreamSource = ms;
                        bmp.EndInit();
                        bmp.Freeze();
                        avatar.Fill = new ImageBrush(bmp);
                    }
                    return;
                }

                if (!string.IsNullOrEmpty(p?.AvatarFileName))
                {
                    string local = ResolveLocalAvatarPath(p.AvatarFileName);
                    if (!string.IsNullOrEmpty(local) && File.Exists(local))
                    {
                        SetAvatarEllipse(avatar, local);
                        return;
                    }

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var downloadedPath = await EnsureAvatarLocalAsync(p.AvatarFileName);
                            if (!string.IsNullOrEmpty(downloadedPath))
                            {
                                await Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    SetAvatarEllipse(avatar, downloadedPath);
                                }));
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Warn("Error downloading avatar " + p?.AvatarFileName, ex);
                        }
                    });

                    SetAvatarEllipse(avatar, null);
                    return;
                }

                SetAvatarEllipse(avatar, null);
            }
            catch (Exception ex)
            {
                Log.Warn("StartAvatarLoad failed", ex);
                SetAvatarEllipse(avatar, null);
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
                    Log.Warn("Error downloading avatar from ProfileManager", ex);
                }
            }
            catch (Exception ex)
            {
                Log.Warn("EnsureAvatarLocalAsync failed", ex);
            }

            return null;
        }

        private void OnReadyStateChangedProxy(string username, bool isReady)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    lock (readyLock) { readyStates[username] = isReady; }
                    ApplyReadyVisual(username, isReady);
                    UpdateReadyVisual();
                    MaybeScheduleHostStart();
                }
                catch (Exception ex) { Log.Warn("OnReadyStateChangedProxy failed", ex); }
            }));
        }

        private void OnMatchStartingProxy()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                awaitingServerStart = false;
                CancelHostGrace();
                CancelServerStartFallback();
                StartCountdown();
            }));
        }

        private void MaybeScheduleHostStart()
        {
            int currentPlayers = slotUser.Count(s => !string.IsNullOrEmpty(s));
            int readyCount;
            lock (readyLock) { readyCount = readyStates.Count(kv => kv.Value && !string.IsNullOrEmpty(kv.Key)); }
            bool allPresentReady = (currentPlayers > 0 && readyCount == currentPlayers);

            if (!allPresentReady)
            {
                CancelHostGrace();
                return;
            }

            if (!string.IsNullOrEmpty(matchHost) && string.Equals(matchHost, currentPlayer, StringComparison.OrdinalIgnoreCase) && gameClient != null)
            {
                if (hostStartGraceTimer == null)
                {
                    EventHandler graceHandler = null;
                    graceHandler = async (s, e) =>
                    {
                        hostStartGraceTimer.Stop();
                        hostStartGraceTimer.Tick -= graceHandler;
                        hostStartGraceTimer = null;

                        await SafeRefreshPlayersAndRecheck();
                        int cp = slotUser.Count(su => !string.IsNullOrEmpty(su));
                        int rc;
                        lock (readyLock) { rc = readyStates.Count(kv => kv.Value && !string.IsNullOrEmpty(kv.Key)); }
                        bool stillAllReady = (cp > 0 && rc == cp);
                        if (stillAllReady && !isStarting)
                        {
                            isStarting = true;
                            try { gameClient.StartMatch(matchId.ToString(), currentPlayer); }
                            catch (Exception ex) { Log.Warn("StartMatch RPC failed", ex); isStarting = false; }
                        }
                    };
                    hostStartGraceTimer = new DispatcherTimer { Interval = hostStartGrace };
                    hostStartGraceTimer.Tick += graceHandler;
                    hostStartGraceTimer.Start();
                }
            }
            else
            {
                awaitingServerStart = true;
                if (serverStartFallbackTimer == null)
                {
                    EventHandler fallbackHandler = null;
                    fallbackHandler = (s, e) =>
                    {
                        serverStartFallbackTimer.Stop();
                        serverStartFallbackTimer.Tick -= fallbackHandler;
                        serverStartFallbackTimer = null;

                        if (awaitingServerStart && !isStarting)
                        {
                            Log.Warn("MatchStarting no recibido; disparando countdown local de respaldo.");
                            StartCountdown();
                        }
                    };
                    serverStartFallbackTimer = new DispatcherTimer { Interval = serverStartFallback };
                    serverStartFallbackTimer.Tick += fallbackHandler;
                    serverStartFallbackTimer.Start();
                }
            }
        }

        private async Task SafeRefreshPlayersAndRecheck()
        {
            try
            {
                var players = await Task.Run(() => gameClient.GetPlayers(matchId.ToString()));
                if (players != null)
                {
                    var ordered = players.OrderBy(p => p?.Position ?? 0).ToArray();
                    slotUser[0] = ordered.ElementAtOrDefault(0)?.PlayerUsername;
                    slotUser[1] = ordered.ElementAtOrDefault(1)?.PlayerUsername;
                    slotUser[2] = ordered.ElementAtOrDefault(2)?.PlayerUsername;
                    slotUser[3] = ordered.ElementAtOrDefault(3)?.PlayerUsername;

                    lock (readyLock)
                    {
                        foreach (var u in slotUser.Where(u => !string.IsNullOrEmpty(u)))
                        {
                            if (!readyStates.ContainsKey(u)) readyStates[u] = false;
                        }
                        foreach (var key in readyStates.Keys.ToList())
                        {
                            if (!slotUser.Contains(key)) readyStates.Remove(key);
                        }
                    }
                }
            }
            catch (Exception ex) { Log.Warn("SafeRefreshPlayersAndRecheck failed", ex); }
        }

        private void CancelHostGrace()
        {
            try
            {
                if (hostStartGraceTimer != null)
                {
                    hostStartGraceTimer.Stop();
                    hostStartGraceTimer.Tick -= null;
                    hostStartGraceTimer = null;
                }
            }
            catch { }
        }

        private void CancelServerStartFallback()
        {
            try
            {
                if (serverStartFallbackTimer != null)
                {
                    serverStartFallbackTimer.Stop();
                    serverStartFallbackTimer.Tick -= null;
                    serverStartFallbackTimer = null;
                }
            }
            catch { }
        }

        private void StartCountdown()
        {
            try
            {
                CancelCountdown();
                countdownValue = 3;
                startCountdownTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                startCountdownTimer.Tick += StartCountdownTimer_Tick;
                startCountdownTimer.Start();
            }
            catch (Exception ex) { Log.Warn("StartCountdown failed", ex); }
        }

        private void StartCountdownTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                countdownValue--;
                if (countdownValue <= 0)
                {
                    CancelCountdown();
                    try
                    {
                        try { OnTutorialStarted?.Invoke(); } catch (Exception ex) { Log.Warn("OnTutorialStarted threw", ex); }

                        try { mediaTutorial.Stop(); } catch { }

                        try
                        {
                            if (callback != null)
                            {
                                try { callback.PlayersUpdated -= OnPlayersUpdatedProxy; } catch { }
                                try { callback.ReadyStateChanged -= OnReadyStateChangedProxy; } catch { }
                                try { callback.MatchStarting -= OnMatchStartingProxy; } catch { }
                            }
                        }
                        catch { }

                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            NavigationService?.Navigate(new RiuvPage(matchId, currentPlayer, gameClient, callback));
                        }));
                    }
                    catch (Exception ex) { Log.Warn("Navigation to RiuvPage failed", ex); }
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
    }
}