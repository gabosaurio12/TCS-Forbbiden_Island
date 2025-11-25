using Forbbiden.Client.GameManager;
using Forbbiden.Client.logic;
using Forbbiden.Client.ProfileManager;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Forbbiden.Client.view
{
    public partial class LobbyPage : Page, IGameServiceCallback
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LobbyPage));

        private DispatcherTimer timer;
        private string currentPlayer;

        private Dictionary<string, TextBlock> playerMsgMap;

        private GameServiceClient gameClient;
        private GameServiceCallback callback;

        private int matchId;
        private string username;

        //Host
        public LobbyPage(int matchId)
        {
            InitializeComponent();

            this.matchId = matchId;

            StartClock();
            LoadPlayers();           
            InitializePlayerMap();

            // Eventos de chat
            txtBxChat.GotFocus += TxtChat_GotFocus;
            txtBxChat.KeyDown += TxtChat_KeyDown;
        }

        //Join
        public LobbyPage(int matchId, string username, GameServiceClient gameClient, GameServiceCallback callback)
        {
            InitializeComponent();

            this.matchId = matchId;
            this.username = username;
            this.gameClient = gameClient;
            this.callback = callback;

            StartClock();
            LoadPlayers();
            InitializePlayerMap();

            TrySubscribeToCallbackEvents();

            txtBxChat.GotFocus += TxtChat_GotFocus;
            txtBxChat.KeyDown += TxtChat_KeyDown;
        }

        private void StartClock()
        {
            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
                txtLobbyHour.Text = DateTime.Now.ToString("hh:mm tt");
        }

        private void TrySubscribeToCallbackEvents()
        {
            if (callback == null) return;

            try
            {
                var t = callback.GetType();
                var ev = t.GetEvent("PlayerJoined");
                if (ev != null)
                    ev.AddEventHandler(callback, new Action<string>(OnPlayerJoined));

                var ev2 = t.GetEvent("PlayerLeft");
                if (ev2 != null)
                    ev2.AddEventHandler(callback, new Action<string>(OnPlayerLeft));

                var ev3 = t.GetEvent("ChatMessage");
                if (ev3 != null)
                    ev3.AddEventHandler(callback, new Action<string, string>(OnChatMessage));
            }
            catch (Exception ex)
            {
                log.Warn("No se pudieron registrar eventos del callback (si no existen, está bien). " + ex.Message);
            }
        }

        private void LoadPlayers()
        {
            try
            {
                var profileClient = new ProfileManagerClient();
                var player = ClientSession.GetPlayer();

                if (player != null && player.PlayerId != -1)
                {
                    txtBkUser1.Text = player.PlayerUsername;
                    currentPlayer = player.PlayerUsername;

                    SetAvatar(imgAvatar1, player.PlayerAvatarPath);
                }
                else
                {
                    txtBkUser1.Text = "Host";
                    currentPlayer = "Host";
                }

                txtBkUser2.Text = "Guest1";
                txtBkUser3.Text = "Guest2";
                txtBkUser4.Text = "Guest3";

                msgUser1.Visibility = Visibility.Collapsed;
                msgUser2.Visibility = Visibility.Collapsed;
                msgUser3.Visibility = Visibility.Collapsed;
                msgUser4.Visibility = Visibility.Collapsed;

                try { profileClient.Close(); } catch { profileClient.Abort(); }
            }
            catch (Exception ex)
            {
                log.Error("LobbyPage - LoadPlayers error", ex);
            }
        }

        private void SetAvatar(Ellipse avatar, string avatarFile)
        {
            try
            {
                string baseDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
                string avatarPath = System.IO.Path.Combine(baseDir, "avatars", avatarFile ?? "");

                if (!File.Exists(avatarPath))
                    avatarPath = System.IO.Path.Combine(baseDir, "Images", "defaultAvatar.png");

                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = new Uri(avatarPath, UriKind.Absolute);
                bmp.EndInit();

                avatar.Fill = new ImageBrush(bmp);
            }
            catch (Exception ex)
            {
                log.Warn("No se pudo cargar avatar, usando fallback. " + ex.Message);
            }
        }

        private void InitializePlayerMap()
        {
            playerMsgMap = new Dictionary<string, TextBlock>();
            try
            {
                playerMsgMap[txtBkUser1.Text] = msgUser1;
                playerMsgMap[txtBkUser2.Text] = msgUser2;
                playerMsgMap[txtBkUser3.Text] = msgUser3;
                playerMsgMap[txtBkUser4.Text] = msgUser4;
            }
            catch (Exception ex)
            {
                log.Warn("InitializePlayerMap partial failed: " + ex.Message);
            }
        }

        private void DisplayMessage(string playerName, string message)
        {
            if (string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(message))
                return;

            if (!playerMsgMap.ContainsKey(playerName))
                return;

            var msgBlock = playerMsgMap[playerName];
            msgBlock.Text = message;
            msgBlock.Visibility = Visibility.Visible;

            var hideTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            hideTimer.Tick += (s, e) =>
            {
                msgBlock.Visibility = Visibility.Collapsed;
                hideTimer.Stop();
            };
            hideTimer.Start();
        }
        private void TxtChat_GotFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(currentPlayer)) return;

            if (!txtBxChat.Text.StartsWith(currentPlayer + ":"))
                txtBxChat.Text = $"{currentPlayer}: ";

            txtBxChat.CaretIndex = txtBxChat.Text.Length;
        }

        private void TxtChat_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            e.Handled = true;

            if (string.IsNullOrWhiteSpace(currentPlayer)) return;

            string fullText = txtBxChat.Text.Trim();

            if (!fullText.StartsWith(currentPlayer + ":"))
                fullText = $"{currentPlayer}: {fullText}";

            var prefix = currentPlayer + ": ";
            string msg = fullText.Length > prefix.Length ? fullText.Substring(prefix.Length) : "";

            if (!string.IsNullOrEmpty(msg))
            {
                DisplayMessage(currentPlayer, msg);

                try
                {
                    if (gameClient != null)
                        gameClient.SendChatMessage(matchId.ToString(), username ?? currentPlayer, msg);
                }
                catch (Exception ex)
                {
                    log.Warn("No se pudo enviar mensaje al servidor: " + ex.Message);
                }
            }

            txtBxChat.Text = $"{currentPlayer}: ";
            txtBxChat.CaretIndex = txtBxChat.Text.Length;
        }

        public void OnPlayerJoined(string player)
        {
            Dispatcher.Invoke(() => UpdateSlot(player, joined: true));
        }

        public void OnPlayerLeft(string player)
        {
            Dispatcher.Invoke(() => UpdateSlot(player, joined: false));
        }

        public void OnChatMessage(string player, string message)
        {
            Dispatcher.Invoke(() => DisplayMessage(player, message));
        }

        public void OnGameStarting()
        {
            Dispatcher.Invoke(() => MessageBox.Show("Game is starting!"));
        }
        private void UpdateSlot(string username, bool joined)
        {
            if (joined)
            {
                if (txtBkUser2.Text == "Guest1") txtBkUser2.Text = username;
                else if (txtBkUser3.Text == "Guest2") txtBkUser3.Text = username;
                else if (txtBkUser4.Text == "Guest3") txtBkUser4.Text = username;
            }
            else
            {
                if (txtBkUser2.Text == username) txtBkUser2.Text = "Guest1";
                if (txtBkUser3.Text == username) txtBkUser3.Text = "Guest2";
                if (txtBkUser4.Text == username) txtBkUser4.Text = "Guest3";
            }

            InitializePlayerMap();
        }

        private void BtnReady_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new games.RiuvPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir la página de juego.");
                log.Error("MainPage.xaml.cs - PlayButton_Click", ex);
            }
        }

        private void btnReady_Server(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new games.ServerPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir la página de juego.");
                log.Error("MainPage.xaml.cs - PlayButton_Click", ex);
            }
        }
    }
}
