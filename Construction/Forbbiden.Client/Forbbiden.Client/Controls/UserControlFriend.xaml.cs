using System.Windows.Controls;
using System.Windows.Media;

namespace Forbbiden.Client.Controls
{
    /// <summary>
    /// Interaction logic for UserControlFriend.xaml
    /// </summary>
    public partial class UserControlFriend : UserControl
    {
        public UserControlFriend()
        {
            InitializeComponent();
        }

        public void SetAvatarImage(ImageBrush avatarImage)
        {
            avatarEllipse.Fill = avatarImage;
        }

        public void SetFriendUsername(string username)
        {
            usernameTxtBk.Text = username;
        }
    }
}
