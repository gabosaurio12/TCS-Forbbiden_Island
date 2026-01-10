using Forbbiden.Client.View.info;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Forbbiden.Client.Logic
{
    public static class ViewUtils
    {
        public static Ellipse GetAvatarEllipse(string avatarPath)
        {
            string projectDir = GetProjectDir();
            string fullAvatarPath = System.IO.Path.Combine(projectDir, "avatars", avatarPath);
            var avatarBitmap = GetBitmapImage(fullAvatarPath);

            Ellipse ellipse = new Ellipse
            {
                Width = 70,
                Height = 70,
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
            bmp.UriSource = new Uri(imagePath, UriKind.Absolute);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();

            return bmp;
        }

        public static byte[] GetDecodedPixelBitmapImage(
            string filePath, int maxDimension = 256, int jpegQuality = 80)
        {
            if (!File.Exists(filePath))
            {
                return new List<byte>().ToArray();
            }

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(filePath);
            bitmap.DecodePixelWidth = maxDimension;
            bitmap.DecodePixelHeight = maxDimension;
            bitmap.EndInit();
            bitmap.Freeze();

            var encoder = new JpegBitmapEncoder();
            encoder.QualityLevel = jpegQuality;
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                return ms.ToArray();
            }
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
    }
}
