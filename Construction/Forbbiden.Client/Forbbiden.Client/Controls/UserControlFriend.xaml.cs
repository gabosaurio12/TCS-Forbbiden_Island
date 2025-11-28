using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

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

        public void SetAvatarImage(Ellipse ellipse, ImageBrush avatarImage)
        {
            ellipse.Fill = avatarImage;
        }

        public void SetFriendUsername(TextBlock textBlock, string username)
        {
            textBlock.Text = username;
        }
    }
}
