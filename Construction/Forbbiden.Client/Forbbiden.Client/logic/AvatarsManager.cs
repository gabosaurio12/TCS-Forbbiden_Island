using Forbbiden.Client.Repositories;
using log4net;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Forbbiden.Client.Logic
{
    public class AvatarsManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AvatarsManager));
        private static readonly Lazy<AvatarsManager> InstanceLazy =
            new Lazy<AvatarsManager>(() => new AvatarsManager());

        private readonly ConcurrentDictionary<string, byte[]> MemoryCache =
            new ConcurrentDictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, SemaphoreSlim> Locks =
            new ConcurrentDictionary<string, SemaphoreSlim>(StringComparer.OrdinalIgnoreCase);

        public static AvatarsManager Instance => InstanceLazy.Value;

        private AvatarsManager() { }

        public void Invalidate(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return;
            MemoryCache.TryRemove(username, out _);
        }

        public void UpdateCache(string username, byte[] bytes)
        {
            if (string.IsNullOrWhiteSpace(username)) return;
            if (bytes == null || bytes.Length == 0)
            {
                MemoryCache.TryRemove(username, out _);
                return;
            }
            MemoryCache[username] = bytes;
            TryWriteLocal(username, null, bytes);
        }

        public async Task<byte[]> GetAvatarBytesAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return Array.Empty<byte>();
            }

            if (MemoryCache.TryGetValue(username, out var cachedBytes) &&
                cachedBytes?.Length > 0)
            {
                return cachedBytes;
            }

            var gate = Locks.GetOrAdd(username, _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync().ConfigureAwait(false);

            try
            {
                if (MemoryCache.TryGetValue(username, out cachedBytes) &&
                    cachedBytes?.Length > 0)
                {
                    return cachedBytes;
                }

                var avatar = await ProfileRepository
                    .GetAvatarByUsername(username)
                    .ConfigureAwait(false);

                var bytes = avatar?.AvatarBytes ?? Array.Empty<byte>();
                MemoryCache[username] = bytes;

                if (bytes.Length > 0)
                {
                    TryWriteLocal(username, avatar?.FileName, bytes);
                }

                return bytes;
            }
            catch (Exception ex)
            {
                Log.Warn($"GetAvatarBytesAsync failed for user {username}", ex);
                return Array.Empty<byte>();
            }
            finally
            {
                gate.Release();
            }
        }

        public async Task<ImageBrush> GetAvatarBrushAsync(string username)
        {
            try
            {
                var bytes = await GetAvatarBytesAsync(username).ConfigureAwait(false);

                if (bytes.Length == 0)
                {
                    return GetDefaultAvatarBrush();
                }

                return CreateBrushFromBytes(bytes);
            }
            catch (Exception ex)
            {
                Log.Warn($"GetAvatarBrushAsync failed for user {username}", ex);
                return GetDefaultAvatarBrush();
            }
        }

        private static ImageBrush CreateBrushFromBytes(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                bitmap.Freeze();

                var brush = new ImageBrush(bitmap);
                brush.Freeze();

                return brush;
            }
        }

        private static ImageBrush GetDefaultAvatarBrush()
        {
            try
            {
                var projectDir = ViewUtils.GetProjectDir();
                var path = Path.Combine(projectDir, "Images", "defaultAvatar.png");

                if (File.Exists(path))
                {
                    var bitmap = new BitmapImage(new Uri(path, UriKind.Absolute));
                    bitmap.Freeze();

                    var brush = new ImageBrush(bitmap);
                    brush.Freeze();

                    return brush;
                }
            }
            catch (Exception ex)
            {
                Log.Warn("GetDefaultAvatarBrush failed", ex);
            }

            return new ImageBrush();
        }

        private static void TryWriteLocal(string username, string remoteFileName, byte[] bytes)
        {
            try
            {
                var projectDir = ViewUtils.GetProjectDir();
                var avatarsDir = Path.Combine(projectDir, "avatars");

                Directory.CreateDirectory(avatarsDir);

                var localFileName = BuildLocalFileName(username, remoteFileName);
                var localPath = Path.Combine(avatarsDir, localFileName);

                File.WriteAllBytes(localPath, bytes);
            }
            catch (Exception ex)
            {
                Log.Warn("TryWriteLocal failed", ex);
            }
        }

        private static string BuildLocalFileName(string username, string remoteFileName)
        {
            string safeUser = username;

            foreach (var c in Path.GetInvalidFileNameChars())
            {
                safeUser = safeUser.Replace(c, '_');
            }

            var extension = Path.GetExtension(remoteFileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".png";
            }

            return $"{safeUser}{extension}";
        }
    }
}