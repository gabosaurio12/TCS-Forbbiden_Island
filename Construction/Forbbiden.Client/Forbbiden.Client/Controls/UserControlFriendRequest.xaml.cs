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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Forbbiden.Client.Controls
{
    /// <summary>
    /// Interaction logic for UserControlFriendRequest.xaml
    /// </summary>
    public partial class UserControlFriendRequest : UserControl
    {
        public UserControlFriendRequest()
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
