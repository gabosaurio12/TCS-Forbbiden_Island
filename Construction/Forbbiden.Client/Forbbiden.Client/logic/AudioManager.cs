using System;
using System.Windows.Media;

namespace Forbbiden.Client.Logic
{
    internal class AudioManager : IDisposable
    {
        private MediaPlayer BackgroundPlayer;
        private MediaPlayer EffectPlayer;
        private bool BackgroundLoop;

        public double MusicVolume { get; set; } = 0.25;
        public double EffectsVolume { get; set; } = 1.0;

        private bool Disposed;

        public void PlayBackground(string relativePath, bool loop = true)
        {
            StopBackGround();

            BackgroundLoop = loop;

            BackgroundPlayer = new MediaPlayer();
            BackgroundPlayer.Open(new Uri(relativePath, UriKind.Relative));
            BackgroundPlayer.Volume = MusicVolume;

            if (BackgroundLoop)
            {
                BackgroundPlayer.MediaEnded += Background_MediaEnded;
            }

            BackgroundPlayer.Play();
        }


        private void Background_MediaEnded(object sender, EventArgs e)
        {
            if (!BackgroundLoop || BackgroundPlayer == null)
            {
                return;
            }

            BackgroundPlayer.Position = TimeSpan.Zero;
            BackgroundPlayer.Play();
        }

        public void StopBackGround()
        {
            if (BackgroundPlayer == null)
            {
                return;
            }

            BackgroundPlayer.MediaEnded -= Background_MediaEnded;
            BackgroundPlayer.Stop();
            BackgroundPlayer.Close();
            BackgroundPlayer = null;
        }

        public void PlayEffect(string relativePath)
        {
            EffectPlayer?.Stop();
            EffectPlayer?.Close();

            EffectPlayer = new MediaPlayer();
            EffectPlayer.Open(new Uri(relativePath, UriKind.Relative));
            EffectPlayer.Volume = EffectsVolume;
            EffectPlayer.Play();
        }

        public void StopAll()
        {
            StopBackGround();
            EffectPlayer?.Stop();
            EffectPlayer?.Close();
            EffectPlayer = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }
            if (disposing)
            {
                StopAll();
            }
            Disposed = true;
        }
    }
}
