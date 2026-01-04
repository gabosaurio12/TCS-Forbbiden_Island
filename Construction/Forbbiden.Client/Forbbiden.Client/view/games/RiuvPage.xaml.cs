using Forbbiden.Client.logic;
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

namespace Forbbiden.Client.view.games
{
    public partial class RiuvPage : Page
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RiuvPage));

        private DispatcherTimer countdownTimer;
        private DispatcherTimer preCountdownTimer;

        private int remainingSeconds = 15;
        private int preCountdown = 3;
        private int correctHits = 0;

        private readonly List<char> possibleKeys;
        private char currentKey;

        private AudioManager audioManager;

        public RiuvPage()
        {
            InitializeComponent();

            audioManager = new AudioManager();

            Loaded += RiuvPage_Loaded;
            Unloaded += RiuvPage_Unloaded;

            possibleKeys = new List<char>();
            possibleKeys.AddRange(Enumerable.Range('A', 26).Select(c => (char)c));
            possibleKeys.AddRange(Enumerable.Range(0, 10).Select(n => n.ToString()[0]));

            LoadPlayers();
            SetRandomKey();
            StartPreCountdown();

            Focusable = true;
            Focus();
            KeyDown += RiuvPage_KeyDown;
        }

        private void RiuvPage_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
        }

        private void RiuvPage_Unloaded(object sender, RoutedEventArgs e)
        {
            audioManager?.Dispose();
            audioManager = null;

            countdownTimer?.Stop();
            preCountdownTimer?.Stop();

            KeyDown -= RiuvPage_KeyDown;
        }

        private void StartPreCountdown()
        {
            txtTimer.Text = preCountdown.ToString();
            audioManager.PlayEffect("sounds/gameCountdown.mp3");

            preCountdownTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            preCountdownTimer.Tick += (s, e) =>
            {
                preCountdown--;

                if (preCountdown > 0)
                {
                    txtTimer.Text = preCountdown.ToString();
                }
                else if (preCountdown == 0)
                {
                    txtTimer.Text = "YA";
                }
                else
                {
                    preCountdownTimer.Stop();
                    txtTimer.Text = remainingSeconds.ToString("D2");
                    audioManager.PlayBackground("sounds/riuvGameMusic.mp3", loop: true);
                    StartCountdown();
                }
            };
            preCountdownTimer.Start();
        }

        private void StartCountdown()
        {
            countdownTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            countdownTimer.Tick += CountdownTimer_Tick;
            countdownTimer.Start();
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            remainingSeconds--;
            txtTimer.Text = remainingSeconds.ToString("D2");

            if (remainingSeconds <= 0)
            {
                countdownTimer.Stop();
                KeyDown -= RiuvPage_KeyDown;
                audioManager.StopAll();
                MessageBox.Show($"Time's up! Teclas correctas: {correctHits}");
            }
        }

        private void SetRandomKey()
        {
            var rand = new Random();
            currentKey = possibleKeys[rand.Next(possibleKeys.Count)];
            txtKey1.Text = currentKey.ToString();
            txtKey1.Foreground = Brushes.Black;
        }

        private async void RiuvPage_KeyDown(object sender, KeyEventArgs e)
        {
            if (remainingSeconds <= 0) return;

            string pressedKey = e.Key.ToString().ToUpper();
            if (pressedKey.Length == 2 && pressedKey.StartsWith("D"))
                pressedKey = pressedKey[1].ToString();

            if (pressedKey == currentKey.ToString())
            {
                correctHits++;
                SetRandomKey();
            }
            else
            {
                txtKey1.Foreground = Brushes.Red;
                await Task.Delay(500);
                txtKey1.Foreground = Brushes.Black;
                SetRandomKey();
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
                    txtName1.Text = player.PlayerUsername;
                    SetAvatar(imgAvatar1, player.PlayerAvatarPath);
                }
                else
                {
                    txtName1.Text = "Player 1";
                    SetAvatar(imgAvatar1, null);
                }

                txtName2.Text = "Player 2";
                txtName3.Text = "Player 3";
                txtName4.Text = "Player 4";

                SetAvatar(imgAvatar2, null);
                SetAvatar(imgAvatar3, null);
                SetAvatar(imgAvatar4, null);

                try { profileClient.Close(); } catch { profileClient.Abort(); }
            }
            catch (Exception ex)
            {
                log.Error("RiuvPage - LoadPlayers error", ex);
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
                log.Error("RiuvPage.SetAvatar", ex);
            }
        }

    }
}
