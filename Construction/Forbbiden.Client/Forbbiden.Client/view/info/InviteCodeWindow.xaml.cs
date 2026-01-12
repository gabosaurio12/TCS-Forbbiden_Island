using Forbbiden.Client.Logic;
using System.Windows;

namespace Forbbiden.Client.View.Info
{
    public partial class InviteCodeWindow : Window
    {
        public string Code { get; private set; }

        public InviteCodeWindow()
        {
            InitializeComponent();
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            Code = codeTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(Code))
            {
                ViewUtils.OpenNotificationWindow(
                    Properties.Resources.invite_invalid_title,
                    Properties.Resources.invite_invalid_message,
                    this);
                return;
            }
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}