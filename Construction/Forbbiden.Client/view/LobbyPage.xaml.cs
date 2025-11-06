using Forbbiden.Client.MatchManager;
using Forbbiden.Client.ProfileManager;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Forbbiden.Client.view
{
    public partial class LobbyPage : Page
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LobbyPage));
        private DispatcherTimer timer;
        private string currentPlayer; 
        private Dictionary<string, TextBlock> playerMsgMap;
        private int currentMatchId;

        public LobbyPage(int matchId)
        {
            InitializeComponent();
            currentMatchId=matchId;
            LoadPlayers();
            StartClock();
            InitializePlayerMap();

            //Chat Events
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

        private void LoadPlayers()
        {
            try
            {
                var matchClient = new MatchManagerClient();
                var match = matchClient.GetMatchById(currentMatchId);
                if (match == null || match.Players == null)
                    return;

                var playerSlots = new List<(TextBlock nameBlock, Ellipse avatarEllipse, TextBlock msgBlock)>
        {
            (txtBkUser1, imgAvatar1, msgUser1),
            (txtBkUser2, imgAvatar2, msgUser2),
            (txtBkUser3, imgAvatar3, msgUser3),
            (txtBkUser4, imgAvatar4, msgUser4)
        };

                // Inicializar slots con datos por defecto
                for (int i = 0; i < playerSlots.Count; i++)
                {
                    playerSlots[i].nameBlock.Text = "Vacant";
                    playerSlots[i].avatarEllipse.Fill = new ImageBrush(
                        new BitmapImage(new Uri("/Images/defaultAvatar.png", UriKind.RelativeOrAbsolute)));
                    playerSlots[i].msgBlock.Visibility = Visibility.Collapsed;
                }

                int guestCounter = 1;
                var tempMsgMap = new Dictionary<string, TextBlock>();

                for (int i = 0; i < match.Players.Count && i < 4; i++)
                {
                    var player = match.Players[i];
                    var slot = playerSlots[i];

                    // Nombre único: PlayerUsername si existe, sino Guest_i
                    string playerName = string.IsNullOrEmpty(player.PlayerUsername)
                                        ? $"Guest_{guestCounter++}"
                                        : player.PlayerUsername;

                    slot.nameBlock.Text = playerName;

                    string avatarPath = "/Images/defaultAvatar.png";
                    // Si quieres soporte avatar real desde ProfileManager:
                    // avatarPath = string.IsNullOrEmpty(player.PlayerAvatarPath) ? "/Images/defaultAvatar.png" : player.PlayerAvatarPath;

                    slot.avatarEllipse.Fill = new ImageBrush(
                        new BitmapImage(new Uri(avatarPath, UriKind.RelativeOrAbsolute)));

                    if (player.IsHost)
                        currentPlayer = playerName;

                    // Llenar diccionario
                    tempMsgMap[playerName] = slot.msgBlock;
                }

                playerMsgMap = tempMsgMap;
            }
            catch (Exception ex)
            {
                log.Error("LobbyPage.xaml.cs - LoadPlayers", ex);
            }
        }




        private void InitializePlayerMap()
        {
            playerMsgMap = new Dictionary<string, TextBlock>()
            {
                { txtBkUser1.Text, msgUser1 },
                { txtBkUser2.Text, msgUser2 },
                { txtBkUser3.Text, msgUser3 },
                { txtBkUser4.Text, msgUser4 }
            };
        }

        private void TxtChat_GotFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBxChat.Text) || !txtBxChat.Text.StartsWith(currentPlayer + ":"))
                txtBxChat.Text = $"{currentPlayer}: ";
            txtBxChat.CaretIndex = txtBxChat.Text.Length;
        }

        private void TxtChat_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true; //Send Sound

                string text = txtBxChat.Text.Trim();
                if (string.IsNullOrEmpty(text))
                    return;

                // Prefix for Player
                if (!text.StartsWith(currentPlayer + ":"))
                    text = $"{currentPlayer}: {text}";

                string message = text.Substring(currentPlayer.Length + 2); 
                DisplayMessage(currentPlayer, message);

                txtBxChat.Text = $"{currentPlayer}: ";
                txtBxChat.CaretIndex = txtBxChat.Text.Length;
            }
        }

        private void DisplayMessage(string playerName, string message)
        {
            if (playerMsgMap.ContainsKey(playerName))
            {
                TextBlock msgBlock = playerMsgMap[playerName];
                msgBlock.Text = message;
                msgBlock.Visibility = Visibility.Visible;

                // Chat Cleaner
                DispatcherTimer hideTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
                hideTimer.Tick += (s, e) =>
                {
                    msgBlock.Visibility = Visibility.Collapsed;
                    hideTimer.Stop();
                };
                hideTimer.Start();
            }
        }

    }
}
