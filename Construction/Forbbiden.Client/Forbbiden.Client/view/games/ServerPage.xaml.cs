using Forbbiden.Client.Logic;
using Forbbiden.Client.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Forbbiden.Client.View.Games
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

            var player = ClientSession.GetPlayer();
            if (player != null && player.PlayerId != -1)
            {
                SetAvatarImage(Avatar1, player.PlayerAvatarPath);
                Name1.Text = player.PlayerUsername;
            }
        }

        private void SetAvatarImage(Ellipse avatar, string avatarFile)
        {
            string baseDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
            string path = System.IO.Path.Combine(baseDir, "avatars", avatarFile ?? "");
            if (!File.Exists(path))
            { 
                path = System.IO.Path.Combine(baseDir, "Images", "defaultAvatar.png");
            }

            avatar.Fill = new ImageBrush(new BitmapImage(new Uri(path, UriKind.Absolute)));
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

            if (CurrentTime <= 3)
                ClockText.Text = $"00:{CurrentTime:00}";
            else
            {
                ClockText.Text = "✖_✖"; 
                ClockBroken = true;
            }

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

            ResultStack.Children.Clear();
            ResultPanel.Visibility = Visibility.Visible;

            if (PlayerHits.Count == 0)
            {
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

            int minDiff = PlayerAvatars.Keys
                .Where(p => PlayerHits.ContainsKey(p))
                .Min(p => Math.Abs(PlayerHits[p] - TargetTime));

            List<string> winners = PlayerAvatars.Keys
                .Where(p => PlayerHits.ContainsKey(p))
                .Where(p => Math.Abs(PlayerHits[p] - TargetTime) == minDiff)
                .Select(p => PlayerNames[p])
                .ToList();


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
