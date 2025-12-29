using Forbbiden.Client.logic;
using Forbbiden.Client.ProfileManager;
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

namespace Forbbiden.Client.view.games
{
    public partial class ServerPage : Page
    {
        DispatcherTimer clockTimer;
        DispatcherTimer initialTimer;
        int currentTime = 0; 
        int targetTime;
        bool clockRunning = false;

        Dictionary<string, int> playerHits = new Dictionary<string, int>();
        Dictionary<string, Ellipse> playerAvatars;
        Dictionary<string, string> playerNames;

        bool clockBroken = false;

        public ServerPage()
        {
            InitializeComponent();
            Loaded += ServerPage_Loaded;
            KeyDown += ServerPage_KeyDown;
            Focusable = true;
            Focus();
        }

        private void ServerPage_Loaded(object sender, RoutedEventArgs e)
        {
            SetupPlayers();
            StartInitialCountdown();
        }

        private void SetupPlayers()
        {
            
            playerAvatars = new Dictionary<string, Ellipse>
            {
                { "Jugador1", Avatar1 },
                { "Jugador2", Avatar2 },
                { "Jugador3", Avatar3 },
                { "Jugador4", Avatar4 }
            };

            playerNames = new Dictionary<string, string>
            {
                { "Jugador1", "Jugador 1" },
                { "Jugador2", "Jugador 2" },
                { "Jugador3", "Jugador 3" },
                { "Jugador4", "Jugador 4" }
            };

            try
            {
                var profileClient = new ProfileManagerClient();
                var player = ClientSession.GetPlayer();
                if (player != null && player.PlayerId != -1)
                {
                    SetAvatarImage(Avatar1, player.PlayerAvatarPath);
                    Name1.Text = player.PlayerUsername;
                }
            }
            catch { }
        }

        private void SetAvatarImage(Ellipse avatar, string avatarFile)
        {
            try
            {
                string baseDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
                string path = System.IO.Path.Combine(baseDir, "avatars", avatarFile ?? "");
                if (!File.Exists(path))
                    path = System.IO.Path.Combine(baseDir, "Images", "defaultAvatar.png");

                avatar.Fill = new ImageBrush(new BitmapImage(new Uri(path, UriKind.Absolute)));
            }
            catch { }
        }

        private void StartInitialCountdown()
        {
            targetTime = MatchLogic.Rand.Next(10, 31); 
            TargetTimeText.Text = $"Apaga el servidor a los 00:{targetTime:00}";
            TargetTimeText.Visibility = Visibility.Visible;

            initialTimer = new DispatcherTimer();
            initialTimer.Interval = TimeSpan.FromSeconds(4); 
            initialTimer.Tick += (s, e) =>
            {
                initialTimer.Stop();
                TargetTimeText.Visibility = Visibility.Collapsed;
                StartGame();
            };
            initialTimer.Start();
        }

        private void StartGame()
        {
            currentTime = 0;
            clockBroken = false;
            ClockText.Text = "00:00";
            clockRunning = true;

            clockTimer = new DispatcherTimer();
            clockTimer.Interval = TimeSpan.FromSeconds(1);
            clockTimer.Tick += ClockTimer_Tick;
            clockTimer.Start();
        }

        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            currentTime++;

            // Mostrar tiempo los primeros 3 segundos
            if (currentTime <= 3)
                ClockText.Text = $"00:{currentTime:00}";
            else
            {
                ClockText.Text = "✖_✖"; 
                clockBroken = true;
            }

            // Terminar automáticamente 5 segundos después del objetivo
            if (currentTime > targetTime + 5)
                EndGame();
        }

        private void ServerPage_KeyDown(object sender, KeyEventArgs e)
        {
            if (!clockRunning) return;
            if (e.Key == Key.Space)
                RegisterHit("Jugador1");
        }

        private void btnPlayer_Click(object sender, RoutedEventArgs e)
        {
            if (!clockRunning) return;
            Button btn = sender as Button;
            string playerId = btn?.Tag?.ToString() ?? "JugadorX";
            RegisterHit(playerId);
        }

        private void RegisterHit(string playerId)
        {
            if (playerHits.ContainsKey(playerId)) return;

            playerHits[playerId] = currentTime;

            if (playerAvatars.ContainsKey(playerId))
            {
                playerAvatars[playerId].Opacity = 1;
                playerAvatars[playerId].Stroke = Brushes.OrangeRed;
                playerAvatars[playerId].StrokeThickness = 6;
            }
        }

        private void EndGame()
        {
            if (!clockRunning) return;

            clockTimer.Stop();
            clockRunning = false;

            if (clockBroken)
                ClockText.Text = $"00:{targetTime:00}"; 

            // Mostrar resultados en pantalla
            ResultStack.Children.Clear();
            ResultPanel.Visibility = Visibility.Visible;

            if (playerHits.Count == 0)
            {
                //Sin Resultados
                TextBlock loseText = new TextBlock
                {
                    FontSize = 36,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Red,
                    FontFamily = new FontFamily("Irish Grover"),
                    TextAlignment = TextAlignment.Center,
                    Text = "Todos perdieron ☠💥"
                };
                ResultStack.Children.Add(loseText);
                return;
            }

            int minDiff = int.MaxValue;
            List<string> winners = new List<string>();

            foreach (var p in playerAvatars.Keys)
            {
                if (playerHits.ContainsKey(p))
                {
                    int diff = Math.Abs(playerHits[p] - targetTime);
                    if (diff < minDiff)
                    {
                        minDiff = diff;
                        winners.Clear();
                        winners.Add(playerNames[p]);
                    }
                    else if (diff == minDiff)
                    {
                        winners.Add(playerNames[p]);
                    }
                }
            }

            TextBlock winnerText = new TextBlock
            {
                FontSize = 36,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Orange,
                FontFamily = new FontFamily("Irish Grover"),
                TextAlignment = TextAlignment.Center,
                Text = winners.Count == 1 ? $"Ganador: {winners[0]}" :
                       $"Empate: {string.Join(", ", winners)}"
            };

            ResultStack.Children.Add(winnerText);
        }
    }
}
