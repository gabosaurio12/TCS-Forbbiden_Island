using Forbbiden.Client.ProfileManager;
using log4net;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Forbbiden.Client.view
{
    public partial class LobbyPage : Page
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LobbyPage));
        private DispatcherTimer timer;
        private string currentPlayer; 
        private Dictionary<string, TextBlock> playerMsgMap;

        public LobbyPage(int matchId)
        {
            InitializeComponent();
            LoadPlayers();
            StartClock();
            InitializePlayerMap();

            //Chat Events
            txtChat.GotFocus += TxtChat_GotFocus;
            txtChat.KeyDown += TxtChat_KeyDown;
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
                var client = new ProfileManagerClient();
                Player player = client.GetCurrentLogin();

                // Player 1 (host)
                if (player != null)
                {
                    txtBkUser1.Text = player.PlayerUsername;
                    imgAvatar1.Fill = new ImageBrush(new System.Windows.Media.Imaging.BitmapImage(new Uri(player.PlayerAvatarPath, UriKind.RelativeOrAbsolute)));
                    currentPlayer = player.PlayerUsername; 
                }

                // Players 2,3,4 (guests placeholders)
                txtBkUser2.Text = "Guest1";
                txtBkUser3.Text = "Guest2";
                txtBkUser4.Text = "Guest3";


                // Message Colappsed
                msgUser1.Visibility = Visibility.Collapsed;
                msgUser2.Visibility = Visibility.Collapsed;
                msgUser3.Visibility = Visibility.Collapsed;
                msgUser4.Visibility = Visibility.Collapsed;
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
            if (string.IsNullOrWhiteSpace(txtChat.Text) || !txtChat.Text.StartsWith(currentPlayer + ":"))
                txtChat.Text = $"{currentPlayer}: ";
            txtChat.CaretIndex = txtChat.Text.Length;
        }

        private void TxtChat_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true; //Send Sound

                string text = txtChat.Text.Trim();
                if (string.IsNullOrEmpty(text))
                    return;

                // Prefix for Player
                if (!text.StartsWith(currentPlayer + ":"))
                    text = $"{currentPlayer}: {text}";

                string message = text.Substring(currentPlayer.Length + 2); 
                DisplayMessage(currentPlayer, message);

                txtChat.Text = $"{currentPlayer}: ";
                txtChat.CaretIndex = txtChat.Text.Length;
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
