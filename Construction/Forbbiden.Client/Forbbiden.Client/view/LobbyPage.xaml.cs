using Forbbiden.Client.GameManager;
using Forbbiden.Client.logic;
using Forbbiden.Client.ProfileManager;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
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
        private static readonly ILog Log = LogManager.GetLogger(typeof(LobbyPage));

        private DispatcherTimer Timer;
        private string CurrentPlayer;

        private Dictionary<string, TextBlock> PlayerMsgMap;

        private readonly GameServiceClient GameClient;
        private readonly GameServiceCallback Callback;

        private readonly int MatchId;
        private readonly string Username;

        //Host
        public LobbyPage(int matchId)
        {
            InitializeComponent();

            this.MatchId = matchId;

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

            this.MatchId = matchId;
            this.Username = username;
            this.GameClient = gameClient;
            this.Callback = callback;

            StartClock();
            LoadPlayers();
            InitializePlayerMap();

            TrySubscribeToCallbackEvents();

            txtBxChat.GotFocus += TxtChat_GotFocus;
            txtBxChat.KeyDown += TxtChat_KeyDown;
        }

        private void StartClock()
        {
            Timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            Timer.Tick += Timer_Tick;
            Timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
                txtLobbyHour.Text = DateTime.Now.ToString("hh:mm tt");
        }

        private void TrySubscribeToCallbackEvents()
        {
            if (Callback == null) return;

            try
            {
                var t = Callback.GetType();
                var ev = t.GetEvent("PlayerJoined");
                if (ev != null)
                    ev.AddEventHandler(Callback, new Action<string>(OnPlayerJoined));

                var ev2 = t.GetEvent("PlayerLeft");
                if (ev2 != null)
                    ev2.AddEventHandler(Callback, new Action<string>(OnPlayerLeft));

                var ev3 = t.GetEvent("ChatMessage");
                if (ev3 != null)
                    ev3.AddEventHandler(Callback, new Action<string, string>(OnChatMessage));
            }
            catch (Exception ex)
            {
                string message = "Callback events were not registered (if doesn't exist, it's fine).";
                Log.Warn(message, ex);
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
                    CurrentPlayer = player.PlayerUsername;

                    SetAvatar(imgAvatar1, player.PlayerAvatarPath);
                }
                else
                {
                    txtBkUser1.Text = "Host";
                    CurrentPlayer = "Host";
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
                Log.Error("LobbyPage - LoadPlayers error", ex);
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
                string message = "We couldn't load the avatar, fallbakc is being used";
                Log.Warn(message, ex);
            }
        }

        private void InitializePlayerMap()
        {
            PlayerMsgMap = new Dictionary<string, TextBlock>();
            try
            {
                PlayerMsgMap[txtBkUser1.Text] = msgUser1;
                PlayerMsgMap[txtBkUser2.Text] = msgUser2;
                PlayerMsgMap[txtBkUser3.Text] = msgUser3;
                PlayerMsgMap[txtBkUser4.Text] = msgUser4;
            }
            catch (Exception ex)
            {
                string message = "InitializePlayerMap partial failed";
                Log.Warn(message, ex);
            }
        }

        private void DisplayMessage(string playerName, string message)
        {
            if (string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(message))
                return;

            if (!PlayerMsgMap.ContainsKey(playerName))
                return;

            var msgBlock = PlayerMsgMap[playerName];
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
            if (string.IsNullOrWhiteSpace(CurrentPlayer)) return;

            if (!txtBxChat.Text.StartsWith(CurrentPlayer + ":"))
                txtBxChat.Text = $"{CurrentPlayer}: ";

            txtBxChat.CaretIndex = txtBxChat.Text.Length;
        }

        private void TxtChat_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            e.Handled = true;

            if (string.IsNullOrWhiteSpace(CurrentPlayer)) return;

            string fullText = txtBxChat.Text.Trim();

            if (!fullText.StartsWith(CurrentPlayer + ":"))
                fullText = $"{CurrentPlayer}: {fullText}";

            var prefix = CurrentPlayer + ": ";
            string msg = fullText.Length > prefix.Length ? fullText.Substring(prefix.Length) : "";

            if (!string.IsNullOrEmpty(msg))
            {
                DisplayMessage(CurrentPlayer, msg);

                try
                {
                    if (GameClient != null)
                        GameClient.SendChatMessage(MatchId.ToString(), Username ?? CurrentPlayer, msg);
                }
                catch (Exception ex)
                {
                    string message = "Sending message to Server failed";
                    Log.Warn(message, ex);
                }
            }

            txtBxChat.Text = $"{CurrentPlayer}: ";
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
                ViewUtils.HandlePageLoadError(Window.GetWindow(this));
                Log.Error("MainPage.xaml.cs - PlayButton_Click", ex);
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
                ViewUtils.HandlePageLoadError(Window.GetWindow(this));
                Log.Error("MainPage.xaml.cs - PlayButton_Click", ex);
            }
        }
    }
}
