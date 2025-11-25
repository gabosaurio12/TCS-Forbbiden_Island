using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.TokenManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Forbbiden.Client.view.info
{
    /// <summary>
    /// Interaction logic for VerificationWIndow.xaml
    /// </summary>
    public partial class VerificationWIndow : Window
    {
        private readonly int PlayerID;

        public VerificationWIndow()
        {
            InitializeComponent();
        }

        public VerificationWIndow(int playerId)
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

        private async void VerifyPlayer()
        {
            var profileManager = new ProfileManagerClient();
            var player = await profileManager.GetPlayerByIdAsync(PlayerID, false);
            if (player != null)
            {
                player.IsVerified = 1;
                var updated = await profileManager.UpdatePlayerAsync(player);
                if (updated)
                {
                    string title = Properties.Langs.Resources.player_verified;
                    string message = Properties.Langs.Resources.player_verified_message;

                    OpenNotification(title, message);
                    Close();
                }
                else
                {
                    string title = Properties.Langs.Resources.error;
                    string message = Properties.Langs.Resources.push_database_error;

                    OpenNotification(title, message);
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

        private async void VerifyToken()
        {
            string token = GetToken();
            if (!String.IsNullOrEmpty(token))
            {
                var tokenManager = new TokenManagerClient();
                var isToken = await tokenManager.VerifyTokenAsync(token, PlayerID);
                if (isToken)
                {
                    VerifyPlayer();
                }
                else
                {
                    string title = Properties.Langs.Resources.wrong_token;
                    string message = Properties.Langs.Resources.wrong_token_message;
                    OpenNotification(title, message);
                    ClearTokenTxtBx();
                }
            }
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            VerifyToken();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
