using Forbbiden.Contracts;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Forbbiden.Server.logic
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Multiple
    )]
    public class GameService : IGameService
    {
        private readonly Dictionary<string, List<IGameServiceCallback>> rooms =
            new Dictionary<string, List<IGameServiceCallback>>();

        private readonly Dictionary<string, List<string>> roomPlayers =
            new Dictionary<string, List<string>>();

        public bool JoinGame(string matchId, string playerName)
        {
            var callback = OperationContext.Current.GetCallbackChannel<IGameServiceCallback>();

            if (!rooms.ContainsKey(matchId))
            {
                rooms[matchId] = new List<IGameServiceCallback>();
                roomPlayers[matchId] = new List<string>();
            }

            rooms[matchId].Add(callback);
            roomPlayers[matchId].Add(playerName);

            foreach (var client in rooms[matchId])
                client.OnPlayerJoined(playerName);

            return true;
        }

        public void LeaveGame(string matchId, string playerName)
        {
            var callback = OperationContext.Current.GetCallbackChannel<IGameServiceCallback>();

            if (!rooms.ContainsKey(matchId))
                return;

            rooms[matchId].Remove(callback);
            roomPlayers[matchId].Remove(playerName);

            foreach (var client in rooms[matchId])
                client.OnPlayerLeft(playerName);
        }

        public void SendChatMessage(string matchId, string playerName, string message)
        {
            if (!rooms.ContainsKey(matchId))
                return;

            foreach (var client in rooms[matchId])
                client.OnChatMessage(playerName, message);
        }

        public List<string> GetPlayers(string matchId)
        {
            if (!roomPlayers.ContainsKey(matchId))
                return new List<string>();

            return roomPlayers[matchId];
        }
    }
}
