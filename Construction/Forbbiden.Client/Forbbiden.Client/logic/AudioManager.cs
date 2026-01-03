using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forbbiden.Client.logic
{
    internal class AudioManager : IDisposable
    {
        private MediaPlayer backgroundPlayer;
        private MediaPlayer effectPlayer;
        private bool backgroundLoop;

        public double MusicVolume { get; set; } = 0.25;
        public double EffectsVolume { get; set; } = 1.0;

        public void PlayBackground(string relativePath, bool loop = true)
        {
            StopBackGround();

            backgroundLoop = loop;

            backgroundPlayer = new MediaPlayer();
            backgroundPlayer.Open(new Uri(relativePath, UriKind.Relative));
            backgroundPlayer.Volume = MusicVolume;

            if (backgroundLoop)
            {
                backgroundPlayer.MediaEnded += Background_MediaEnded;
            }

            backgroundPlayer.Play();
        }


        private void Background_MediaEnded(object sender, EventArgs e)
        {
            if (!backgroundLoop || backgroundPlayer == null)
            {
                return;
            }

            backgroundPlayer.Position = TimeSpan.Zero;
            backgroundPlayer.Play();
        }

        public void StopBackGround()
        {
            if (backgroundPlayer != null)
            {
                backgroundPlayer.MediaEnded -= Background_MediaEnded;
                backgroundPlayer?.Stop();
                backgroundPlayer?.Close();
                backgroundPlayer = null;
            }
        }

        public void PlayEffect(string relativePath)
        {
            effectPlayer?.Stop();
            effectPlayer?.Close();

            effectPlayer = new MediaPlayer();
            effectPlayer.Open(new Uri(relativePath, UriKind.Relative));
            effectPlayer.Volume = EffectsVolume;
            effectPlayer.Play();
        }

        public void StopAll()
        {
            StopBackGround();
            effectPlayer?.Stop();
            effectPlayer?.Close();
            effectPlayer = null;
        }

        public void Dispose()
        {
            StopAll();
        }
    }
}
