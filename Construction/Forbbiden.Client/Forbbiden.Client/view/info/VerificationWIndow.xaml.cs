using Forbbiden.Client.logic;
using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.TokenManager;
using log4net;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;

namespace Forbbiden.Client.view.info
{
    /// <summary>
    /// Interaction logic for VerificationWIndow.xaml
    /// </summary>
    public partial class VerificationWindow : Window
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(VerificationWindow));

        private readonly int PlayerID;
        public event Action OnVerified;

        public VerificationWindow()
        {
            InitializeComponent();
        }

        public VerificationWindow(int playerId)
        {
            InitializeComponent();
            PlayerID = playerId;
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
            var profileManager = new ProfileManagerClient();

            Player player = new Player();

            try
            {
                player = await profileManager.GetPlayerByIdAsync(PlayerID, false);
            }
            catch (FaultException<DBFault> ex)
            {
                Log.Error("VerificationWindow.VerifyPlayer", ex);
                ViewUtils.ShowPullError(GetWindow(this));
            }
            if (player.PlayerId != -1)
            {
                player.IsVerified = 1;
                bool isUpdated = false;
                try
                {
                    isUpdated = await profileManager.UpdatePlayerAsync(player);
                }
                catch (FaultException<DBFault> ex)
                {
                    Log.Error("VerificationWindow.VerifyPlayer", ex);
                    ViewUtils.ShowPushError(GetWindow(this));
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
            if (!String.IsNullOrEmpty(token))
            {
                var tokenManager = new TokenManagerClient();
                bool isToken = false;                
                try
                {
                    isToken = await tokenManager.VerifyTokenAsync(token, PlayerID);
                }
                catch (FaultException<DBFault> ex)
                {
                    Log.Error("VerificationWindow.VerifyToken", ex);
                    ViewUtils.ShowPullError(GetWindow(this));
                }
                if (isToken)
                {
                    _ = VerifyPlayer();
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
