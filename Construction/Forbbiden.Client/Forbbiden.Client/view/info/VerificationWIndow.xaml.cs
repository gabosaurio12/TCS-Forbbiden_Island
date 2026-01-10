using Forbbiden.Client.Exceptions;
using Forbbiden.Client.Logic;
using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.Repositories;
using Forbbiden.Client.TokenManager;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Forbbiden.Client.View.info
{
    /// <summary>
    /// Interaction logic for VerificationWIndow.xaml
    /// </summary>
    public partial class VerificationWindow : Window
    {
        private readonly int PlayerID;
        public event Action OnVerified;
        private bool IsPasswordChange;

        public VerificationWindow()
        {
            InitializeComponent();
        }

        public VerificationWindow(int playerId, bool isPasswordChange)
        {
            InitializeComponent();
            PlayerID = playerId;
            IsPasswordChange = isPasswordChange;
        }

        private void OpenNotification(string title, string message)
        {
            var notificationWindow = new NotificationWindow(title, message)
            {
                Owner = GetWindow(this)
            };
            notificationWindow.ShowDialog();
        }

        private string GetToken()
        {
            string token = txtBx1.Text.Trim();
            token += txtBx2.Text.Trim();
            token += txtBx3.Text.Trim();
            token += txtBx4.Text.Trim();          
            token += txtBx5.Text.Trim();
            token += txtBx6.Text.Trim();
            return token;
        }

        private async Task VerifyPlayer()
        {
            Player player = new Player();
            var profileRepository = new ProfileRepository();

            try
            {
                player = await profileRepository.GetPlayerById(PlayerID, false);
            }
            catch (ViewException ex)
            {
                ExceptionViewManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
            }
            if (player.PlayerId != -1)
            {
                player.IsVerified = 1;
                bool isUpdated = false;
                try
                {
                    isUpdated = await profileRepository.UpdatePlayerProfile(player);
                }
                catch (ViewException ex)
                {
                    ExceptionViewManager.ShowViewExceptionNotification(ex, Window.GetWindow(this));
                }
                if (isUpdated)
                {
                    string title = Properties.Resources.player_verified;
                    string message = Properties.Resources.player_verified_message;

                    ViewUtils.OpenNotificationWindow(title, message, GetWindow(this));
                    OnVerified?.Invoke();
                    Close();
                }
            }
        }

        private void ClearTokenTxtBx()
        {
            txtBx1?.Clear();
            txtBx2?.Clear();
            txtBx3?.Clear();
            txtBx4?.Clear();
            txtBx5?.Clear();
            txtBx6?.Clear();
        }

        private async Task VerifyToken()
        {
            string token = GetToken();
            if (!String.IsNullOrWhiteSpace(token))
            {
                var tokenManager = new TokenManagerClient();
                bool isToken = await tokenManager.VerifyTokenAsync(token, PlayerID);
               
                if (isToken)
                {
                    if (IsPasswordChange)
                    {
                        DialogResult = true;
                    }
                    else
                    {
                        _ = VerifyPlayer();
                    }
                }
                else
                {
                    string title = Properties.Resources.wrong_token;
                    string message = Properties.Resources.wrong_token_message;
                    OpenNotification(title, message);
                    ClearTokenTxtBx();
                }
            }
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            _ = VerifyToken();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
