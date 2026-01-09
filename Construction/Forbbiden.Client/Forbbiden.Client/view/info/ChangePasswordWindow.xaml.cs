using Forbbiden.Client.logic;
using Forbbiden.Client.Logic;
using Forbbiden.Client.Logic.Validations;
using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.Repositories;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Forbbiden.Client.view.info
{
    /// <summary>
    /// Interaction logic for ChangePasswordPage.xaml
    /// </summary>
    public partial class ChangePasswordWindow : Window
    {
        public string HashedPassword { get; private set; }
        public ChangePasswordWindow()
        {
            InitializeComponent();
        }

        private async Task VerifyChangeAsync()
        {
            var tokenRepo = new TokenRepository();
            var token = await tokenRepo.GenerateToken(ClientSession.CurrentPlayerId);
            var profileRepo = new ProfileRepository();
            var result = await profileRepo.SendVerificationEmail(ClientSession.Email, token.TokenString);

            if (result)
            {
                string title = Properties.Resources.verification_token_sent_title;
                string message = Properties.Resources.verification_token_sent;
                ViewUtils.OpenNotificationWindow(title, message, this);

                var verificationWindow = new VerificationWindow(ClientSession.CurrentPlayerId, true)
                {
                    Owner = this
                };

                if (verificationWindow.ShowDialog() == true)
                {
                    var password = PasswordTxtBx.Text;
                    HashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                    DialogResult = true;
                }
            }
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            var password = PasswordTxtBx.Text;
            var validationResults = ValidationUtils.ValidatePassword(password);
            if (!validationResults.IsValid)
            {
                PasswordTxtBx.BorderBrush = Brushes.Red;
                ErrorsNotificationManager.ShowPasswordValidationErrors(validationResults.Errors, this);
            }
            else
            {
                _ = VerifyChangeAsync();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
