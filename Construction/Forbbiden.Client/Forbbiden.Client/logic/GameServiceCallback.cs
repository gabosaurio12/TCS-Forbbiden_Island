using System;
using System.Windows;
using Forbbiden.Client.GameManager;

namespace Forbbiden.Client.Logic
{

    public class GameServiceCallback : IGameManagerCallback
    {
        public event Action<PlayerInfo[]> PlayersUpdated;
        public event Action<string, string> ChatMessageReceived;
        public event Action GameStarting;

        public event Action<string, bool> ReadyStateChanged;
        public event Action MatchStarting;

        public void OnPlayersUpdated(PlayerInfo[] players)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                PlayersUpdated?.Invoke(players);
            });
        }

        public void OnChatMessage(string playerName, string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ChatMessageReceived?.Invoke(playerName, message);
            });
        }

        public void OnGameStarting()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GameStarting?.Invoke();
            });
        }

        void IGameManagerCallback.ReadyStateChanged(string username, bool ready)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try { ReadyStateChanged?.Invoke(username, ready); }
                catch { /* proteger la invocación */ }
            });
        }

        void IGameManagerCallback.MatchStarting()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try { MatchStarting?.Invoke(); }
                catch { /* proteger la invocación */ }
            });
        }
    }
}