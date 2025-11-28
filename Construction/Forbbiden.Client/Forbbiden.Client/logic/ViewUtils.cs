using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.view.info;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Forbbiden.Client.logic
{
    public static class ViewUtils
    {
        public static Ellipse GetAvatarEllipse(string avatarPath)
        {
            string projectDir = ViewUtils.GetProjectDir();
            string fullAvatarPath = System.IO.Path.Combine(projectDir, "avatars", avatarPath);
            var avatarBitmap = GetBitmapImage(fullAvatarPath);

            Ellipse ellipse = new Ellipse
            {
                Width = 100,
                Height = 100,
                Stroke = Brushes.LightGray,
                StrokeThickness = 5,
                Margin = new Thickness(0, 0, 0, 0),
                Fill = new ImageBrush
                {
                    ImageSource = avatarBitmap,
                    Stretch = Stretch.UniformToFill
                }
            };

            return ellipse;
        }
        public static BitmapImage GetBitmapImage(string imagePath)
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            Console.WriteLine(imagePath);
            bmp.UriSource = new Uri(imagePath, UriKind.Absolute);
            bmp.EndInit();

            return bmp;
        }

        public static ImageBrush GetImageBrush(string avatarPath)
        {
            ImageBrush avatarImage = new ImageBrush(new BitmapImage(new Uri(avatarPath)));

            return avatarImage;
        }

        public static string GetProjectDir()
        {
            string projectDir = Directory.GetParent(
                    AppDomain.CurrentDomain.BaseDirectory).
                    Parent.Parent.FullName;

            return projectDir;
        }

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);

            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as T;
        }

        public static void OpenNotificationWindow(string title, string message, Window window)
        {
            var notificationWindow = new NotificationWindow(title, message)
            {
                Owner = window
            };
            notificationWindow.ShowDialog();
        }
        public static void ShowPullError(Window window)
        {
            string title = Properties.Langs.Resources.error;
            string message = Properties.Langs.Resources.pull_database_error;
            OpenNotificationWindow(title, message, window);
        }

        public static void ShowPushError(Window window)
        {
            string title = Properties.Langs.Resources.error;
            string message = Properties.Langs.Resources.push_database_error;
            OpenNotificationWindow(title, message, window);
        }

        public static void HandlePageLoadError(Window window)
        {
            string title = Properties.Langs.Resources.error;
            string message = Properties.Langs.Resources.load_page_error;
            OpenNotificationWindow(title, message, window);
        }
    }
}
