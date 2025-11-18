using Forbbiden.Client.GameManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Forbbiden.Client.logic
{
    public class GameServiceCallback : IGameServiceCallback
    {
        public void OnPlayerJoined(string playerName)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"{playerName} se unió al lobby.");
            });
        }

        public void OnPlayerLeft(string playerName)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"{playerName} salió del lobby.");
            });
        }

        public void OnChatMessage(string playerName, string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"{playerName}: {message}");
            });
        }
    }
}
