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
        DispatcherTimer ClockTimer;
        DispatcherTimer InitialTimer;
        int CurrentTime = 0; 
        int TargetTime;
        bool ClockRunning = false;

        readonly Dictionary<string, int> PlayerHits = new Dictionary<string, int>();
        Dictionary<string, Ellipse> PlayerAvatars;
        Dictionary<string, string> PlayerNames;

        bool ClockBroken = false;

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
            
            PlayerAvatars = new Dictionary<string, Ellipse>
            {
                { "Jugador1", Avatar1 },
                { "Jugador2", Avatar2 },
                { "Jugador3", Avatar3 },
                { "Jugador4", Avatar4 }
            };

            PlayerNames = new Dictionary<string, string>
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
            TargetTime = MatchLogic.Rand.Next(10, 31); 
            TargetTimeText.Text = $"Apaga el servidor a los 00:{TargetTime:00}";
            TargetTimeText.Visibility = Visibility.Visible;

            InitialTimer = new DispatcherTimer();
            InitialTimer.Interval = TimeSpan.FromSeconds(4); 
            InitialTimer.Tick += (s, e) =>
            {
                InitialTimer.Stop();
                TargetTimeText.Visibility = Visibility.Collapsed;
                StartGame();
            };
            InitialTimer.Start();
        }

        private void StartGame()
        {
            CurrentTime = 0;
            ClockBroken = false;
            ClockText.Text = "00:00";
            ClockRunning = true;

            ClockTimer = new DispatcherTimer();
            ClockTimer.Interval = TimeSpan.FromSeconds(1);
            ClockTimer.Tick += ClockTimer_Tick;
            ClockTimer.Start();
        }

        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            CurrentTime++;

            // Mostrar tiempo los primeros 3 segundos
            if (CurrentTime <= 3)
                ClockText.Text = $"00:{CurrentTime:00}";
            else
            {
                ClockText.Text = "✖_✖"; 
                ClockBroken = true;
            }

            // Terminar automáticamente 5 segundos después del objetivo
            if (CurrentTime > TargetTime + 5)
                EndGame();
        }

        private void ServerPage_KeyDown(object sender, KeyEventArgs e)
        {
            if (!ClockRunning) return;
            if (e.Key == Key.Space)
                RegisterHit("Jugador1");
        }

        private void btnPlayer_Click(object sender, RoutedEventArgs e)
        {
            if (!ClockRunning) return;
            Button btn = sender as Button;
            string playerId = btn?.Tag?.ToString() ?? "JugadorX";
            RegisterHit(playerId);
        }

        private void RegisterHit(string playerId)
        {
            if (PlayerHits.ContainsKey(playerId)) return;

            PlayerHits[playerId] = CurrentTime;

            if (PlayerAvatars.ContainsKey(playerId))
            {
                PlayerAvatars[playerId].Opacity = 1;
                PlayerAvatars[playerId].Stroke = Brushes.OrangeRed;
                PlayerAvatars[playerId].StrokeThickness = 6;
            }
        }

        private void EndGame()
        {
            if (!ClockRunning) return;

            ClockTimer.Stop();
            ClockRunning = false;

            if (ClockBroken)
                ClockText.Text = $"00:{TargetTime:00}"; 

            // Mostrar resultados en pantalla
            ResultStack.Children.Clear();
            ResultPanel.Visibility = Visibility.Visible;

            if (PlayerHits.Count == 0)
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

            foreach (var p in PlayerAvatars.Keys)
            {
                if (PlayerHits.ContainsKey(p))
                {
                    int diff = Math.Abs(PlayerHits[p] - TargetTime);
                    if (diff < minDiff)
                    {
                        minDiff = diff;
                        winners.Clear();
                        winners.Add(PlayerNames[p]);
                    }
                    else if (diff == minDiff)
                    {
                        winners.Add(PlayerNames[p]);
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
