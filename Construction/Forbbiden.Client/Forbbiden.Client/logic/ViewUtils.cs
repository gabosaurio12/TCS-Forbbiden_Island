using Forbbiden.Client.View.info;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        public static Ellipse GetDefaultAvatarEllipse()
        {
            var avatarBitmap = GetDefaultAvatarBrush().ImageSource as BitmapImage;

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

        public static BitmapImage GetBitmapImageFromFileName(string imagePath)
        {
            string fullImagePath = System.IO.Path.Combine(GetProjectDir(), "Images",  imagePath);
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri(fullImagePath, UriKind.Absolute);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();

            return bmp;
        }

        public static BitmapImage GetTileBitmapImageFromFileName(string imagePath)
        {
            string fullImagePath = System.IO.Path.Combine(GetProjectDir(), "Images", "tiles", imagePath);
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri(fullImagePath, UriKind.Absolute);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();

            return bmp;
        }

        public static void SetBackground(ImageBrush background)
        {
            DateTime currentTime = DateTime.Now;
            string ampm = currentTime.ToString("tt", CultureInfo.InvariantCulture).ToLower();
            if (ampm == "pm")
            {
                string darkBackground = "FEIMainPageNight.png";
                string projectDir = Directory.GetParent(
                AppDomain.CurrentDomain.BaseDirectory).
                Parent.Parent.FullName;
                string imagesPath = System.IO.Path.Combine(
                    projectDir, "Images");
                string backgroundPath = System.IO.Path.Combine(
                    imagesPath, darkBackground);
                background.ImageSource = ViewUtils.GetBitmapImage(backgroundPath);
            }
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

        public static ImageBrush GetDefaultAvatarBrush()
        {
            string projectDir = GetProjectDir();
            string defaultAvatarPath = System.IO.Path.Combine(projectDir, "Images", "defaultAvatar.png");
            ImageBrush defaultAvatarBrush = GetImageBrush(defaultAvatarPath);
            return defaultAvatarBrush;
        }

        public static string GetProjectDir()
        {
            string projectDir = Directory.GetParent(
                    AppDomain.CurrentDomain.BaseDirectory).
                    Parent.Parent.FullName;

            return projectDir;
        }

        public static string GetAvatarsDir()
        {
            string projectDir = GetProjectDir();
            string avatarsDir = System.IO.Path.Combine(projectDir, "avatars");

            return avatarsDir;
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
