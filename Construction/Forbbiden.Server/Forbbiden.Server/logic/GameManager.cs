using Forbbiden.Contracts;
using Forbbiden.Server.Model;
using Forbbiden.Server.utils;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.ServiceModel;

namespace Forbbiden.Server.logic
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class GameManager : IGameManager
    {
        private readonly Dictionary<string, HashSet<string>> roomBans = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        private static readonly ILog Log = LogManager.GetLogger(typeof(GameManager));
        private readonly Dictionary<string, List<IGameManagerCallback>> rooms = new Dictionary<string, List<IGameManagerCallback>>();
        private readonly Dictionary<string, List<PlayerInfo>> roomPlayers = new Dictionary<string, List<PlayerInfo>>();
        private readonly Dictionary<IGameManagerCallback, string> callbackToRoom = new Dictionary<IGameManagerCallback, string>();
        private readonly Dictionary<string, HashSet<string>> matchReady = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        private readonly object syncRoot = new object();
        private readonly string connectionString;

        public GameManager()
        {
            connectionString = ConnectionStringSingleton.GetInstance().ConnectionString;
        }

        public bool JoinGame(string matchId, string playerName, byte[] avatarBytes, string avatarFileName)
        {
            if (roomBans.TryGetValue(matchId, out var bans) && bans.Contains(playerName))
                return false;

            int playerIdFromDb = ResolveOrCreatePlayerId(matchId, playerName);
            if (playerIdFromDb == int.MinValue) return false;

            var callback = OperationContext.Current.GetCallbackChannel<IGameManagerCallback>();
            lock (syncRoot)
            {
                if (!rooms.ContainsKey(matchId))
                {
                    rooms[matchId] = new List<IGameManagerCallback>();
                    roomPlayers[matchId] = new List<PlayerInfo>();
                    matchReady[matchId] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }

                if (!rooms[matchId].Contains(callback))
                    rooms[matchId].Add(callback);

                var existing = roomPlayers[matchId].FirstOrDefault(p => string.Equals(p.PlayerUsername, playerName, StringComparison.OrdinalIgnoreCase));
                if (existing == null)
                {
                    var pos = roomPlayers[matchId].Count;
                    var pinfo = new PlayerInfo
                    {
                        PlayerId = playerIdFromDb,
                        PlayerUsername = playerName,
                        PlayerName = playerName,
                        IsHost = (pos == 0),
                        Position = pos,
                        AvatarBytes = (avatarBytes != null && avatarBytes.Length > 0) ? avatarBytes : null,
                        AvatarFileName = string.IsNullOrEmpty(avatarFileName) ? null : avatarFileName
                    };
                    roomPlayers[matchId].Add(pinfo);
                }
                else
                {
                    existing.PlayerId = playerIdFromDb;
                    if (avatarBytes != null && avatarBytes.Length > 0)
                    {
                        existing.AvatarBytes = avatarBytes;
                        if (!string.IsNullOrEmpty(avatarFileName)) existing.AvatarFileName = avatarFileName;
                    }
                    else if (!string.IsNullOrEmpty(avatarFileName))
                    {
                        existing.AvatarFileName = avatarFileName;
                        existing.AvatarBytes = null;
                    }
                }
                callbackToRoom[callback] = matchId;
            }

            BroadcastPlayersUpdate(matchId);
            return true;
        }
        public void LeaveGame(string matchId, string playerName)
        {
            int? playerId = null;
            var callback = OperationContext.Current.GetCallbackChannel<IGameManagerCallback>();
            lock (syncRoot)
            {
                if (!rooms.ContainsKey(matchId)) return;

                if (rooms[matchId].Contains(callback))
                    rooms[matchId].Remove(callback);

                if (callbackToRoom.ContainsKey(callback))
                    callbackToRoom.Remove(callback);

                var toRemove = roomPlayers[matchId].FirstOrDefault(p => string.Equals(p.PlayerUsername, playerName, StringComparison.OrdinalIgnoreCase));
                if (toRemove != null)
                {
                    playerId = toRemove.PlayerId;
                    roomPlayers[matchId].Remove(toRemove);
                }

                if (matchReady.TryGetValue(matchId, out var set))
                    set.Remove(playerName);

                if (roomPlayers[matchId].Count == 0)
                {
                    rooms.Remove(matchId);
                    roomPlayers.Remove(matchId);
                    matchReady.Remove(matchId);
                    roomBans.Remove(matchId);
                }
                else
                {
                    for (int i = 0; i < roomPlayers[matchId].Count; i++)
                        roomPlayers[matchId][i].Position = i;
                }
            }

            if (playerId.HasValue)
                RemovePlayerFromDb(matchId, playerId.Value);

            BroadcastPlayersUpdate(matchId);
        }

        public void SendChatMessage(string matchId, string playerName, string message)
        {
            List<IGameManagerCallback> toNotify;
            lock (syncRoot)
            {
                if (!rooms.ContainsKey(matchId)) return;
                toNotify = new List<IGameManagerCallback>(rooms[matchId]);
            }

            foreach (var client in toNotify)
            {
                try { client.OnChatMessage(playerName, message); }
                catch { }
            }
        }

        public List<PlayerInfo> GetPlayers(string matchId)
        {
            lock (syncRoot)
            {
                if (!roomPlayers.ContainsKey(matchId))
                    return new List<PlayerInfo>();

                return roomPlayers[matchId].Select(p => new PlayerInfo
                {
                    PlayerId = p.PlayerId,
                    PlayerUsername = p.PlayerUsername,
                    PlayerName = p.PlayerName,
                    IsHost = p.IsHost,
                    Position = p.Position,
                    AvatarBytes = p.AvatarBytes,
                    AvatarFileName = p.AvatarFileName
                }).ToList();
            }
        }

        public void SetReady(string matchId, string username, bool ready)
        {
            if (string.IsNullOrEmpty(matchId) || string.IsNullOrEmpty(username)) return;

            lock (syncRoot)
            {
                if (!matchReady.ContainsKey(matchId))
                    matchReady[matchId] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                var set = matchReady[matchId];
                if (ready) set.Add(username);
                else set.Remove(username);
            }

            BroadcastReadyState(matchId, username, ready);

            int currentPlayers = 0;
            int readyCount = 0;
            lock (syncRoot)
            {
                if (roomPlayers.TryGetValue(matchId, out var players))
                    currentPlayers = players.Count;
                if (matchReady.TryGetValue(matchId, out var rset))
                    readyCount = rset.Count;
            }

            if (currentPlayers > 0 && readyCount == currentPlayers)
            {
                BroadcastMatchStarting(matchId);
            }
        }

        public void StartMatch(string matchId, string username)
        {
            if (string.IsNullOrEmpty(matchId) || string.IsNullOrEmpty(username))
            {
                return;
            }

            bool isHost = false;
            int currentPlayers = 0;
            int readyCount = 0;
            lock (syncRoot)
            {
                if (roomPlayers.TryGetValue(matchId, out var players))
                {
                    currentPlayers = players.Count;
                    var host = players.FirstOrDefault(p => p.IsHost);
                    if (host != null && string.Equals(
                        host.PlayerUsername,
                        username,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        isHost = true;
                    }
                }
                if (matchReady.TryGetValue(matchId, out var rset))
                {
                    readyCount = rset.Count;
                }
            }

            if (!isHost)
            {
                return;
            }

            if (currentPlayers > 0 && readyCount == currentPlayers)
            {
                BroadcastMatchStarting(matchId);
            }
        }

        private void BroadcastPlayersUpdate(string matchId)
        {
            List<IGameManagerCallback> toNotify;
            List<PlayerInfo> playersSnapshot;
            lock (syncRoot)
            {
                if (!rooms.ContainsKey(matchId)) return;
                toNotify = new List<IGameManagerCallback>(rooms[matchId]);
                playersSnapshot = roomPlayers[matchId].Select(p => new PlayerInfo
                {
                    PlayerId = p.PlayerId,
                    PlayerUsername = p.PlayerUsername,
                    PlayerName = p.PlayerName,
                    IsHost = p.IsHost,
                    Position = p.Position,
                    AvatarBytes = p.AvatarBytes,
                    AvatarFileName = p.AvatarFileName
                }).ToList();
            }

            SendCallbackToEachPlayer(toNotify, playersSnapshot);
        }

        private void SendCallbackToEachPlayer(List<IGameManagerCallback> toNotify, List<PlayerInfo> playersSnapshot)
        {
            var failed = new List<IGameManagerCallback>();
            foreach (var client in toNotify)
            {
                try {
                    client.OnPlayersUpdated(playersSnapshot);
                }
                catch (Exception ex)
                {
                    Log.Warn("GameManager.SendCallbackToEachPlayer", ex);
                    failed.Add(client);
                }
            }

            if (failed.Count > 0)
            {
                lock (syncRoot)
                {
                    CleanupDisconnectedClients(failed);
                }
            }
        }

        private void CleanupDisconnectedClients(List<IGameManagerCallback> failed)
        {
            foreach (var bad in failed)
            {
                if (callbackToRoom.TryGetValue(bad, out var r))
                {
                    if (rooms.ContainsKey(r))
                    {
                        rooms[r].Remove(bad);
                    }
                    callbackToRoom.Remove(bad);
                }
            }

            var empties = rooms.Where(kv => kv.Value.Count == 0).Select(kv => kv.Key).ToList();
            foreach (var e in empties)
            {
                rooms.Remove(e);
                roomPlayers.Remove(e);
                matchReady.Remove(e);
            }
        }

        private void BroadcastReadyState(string matchId, string username, bool ready)
        {
            List<IGameManagerCallback> toNotify;
            lock (syncRoot)
            {
                if (!rooms.ContainsKey(matchId)) return;
                toNotify = new List<IGameManagerCallback>(rooms[matchId]);
            }

            var failed = new List<IGameManagerCallback>();
            foreach (var client in toNotify)
            {
                try { client.ReadyStateChanged(username, ready); }
                catch { failed.Add(client); }
            }

            if (failed.Count > 0)
            {
                lock (syncRoot)
                {
                    foreach (var bad in failed)
                    {
                        if (callbackToRoom.TryGetValue(bad, out var r))
                        {
                            if (rooms.ContainsKey(r)) rooms[r].Remove(bad);
                            callbackToRoom.Remove(bad);
                        }
                    }
                }
            }
        }

        private void BroadcastMatchStarting(string matchId)
        {
            List<IGameManagerCallback> toNotify;
            lock (syncRoot)
            {
                if (!rooms.ContainsKey(matchId)) return;
                toNotify = new List<IGameManagerCallback>(rooms[matchId]);
            }

            var failed = new List<IGameManagerCallback>();
            foreach (var client in toNotify)
            {
                try { client.MatchStarting(); }
                catch { failed.Add(client); }
            }

            if (failed.Count > 0)
            {
                lock (syncRoot)
                {
                    foreach (var bad in failed)
                    {
                        if (callbackToRoom.TryGetValue(bad, out var r))
                        {
                            if (rooms.ContainsKey(r)) rooms[r].Remove(bad);
                            callbackToRoom.Remove(bad);
                        }
                    }
                }
            }
        }

        private int ResolveOrCreatePlayerId(string matchId, string username)
        {
            if (!int.TryParse(matchId, out int mid)) return int.MinValue;
            try
            {
                using (var db = new Forbidden_FEIEntities(connectionString))
                {
                    var player = db.Player.FirstOrDefault(p => p.player_username == username);
                    int playerId;
                    if (player != null)
                    {
                        playerId = player.player_id;
                    }
                    else
                    {
                        int minGuestId = db.match_players
                            .Where(mp => mp.match_id == mid && mp.player_id < 0)
                            .Select(mp => mp.player_id)
                            .DefaultIfEmpty(0)
                            .Min();
                        playerId = minGuestId - 1;
                    }

                    bool exists = db.match_players.Any(mp => mp.match_id == mid && mp.player_id == playerId);
                    if (!exists)
                    {
                        db.match_players.Add(new match_players
                        {
                            match_id = mid,
                            player_id = playerId
                        });
                        db.SaveChanges();
                    }

                    return playerId;
                }
            }
            catch (EntityException ex)
            {
                Log.Error("ResolveOrCreatePlayerId DB error", ex);
                return int.MinValue;
            }
            catch (Exception ex)
            {
                Log.Error("ResolveOrCreatePlayerId error", ex);
                return int.MinValue;
            }
        }

        private void RemovePlayerFromDb(string matchId, int playerId)
        {
            if (!int.TryParse(matchId, out int mid)) return;
            try
            {
                using (var db = new Forbidden_FEIEntities(connectionString))
                {
                    var rows = db.match_players.Where(mp => mp.match_id == mid && mp.player_id == playerId).ToList();
                    if (rows.Any())
                    {
                        db.match_players.RemoveRange(rows);
                        db.SaveChanges();
                    }
                }
            }
            catch (EntityException ex)
            {
                Log.Error("RemovePlayerFromDb DB error", ex);
            }
            catch (Exception ex)
            {
                Log.Error("RemovePlayerFromDb error", ex);
            }
        }

        public void KickPlayer(string matchId, string hostUsername, string targetUsername)
        {
            if (string.IsNullOrWhiteSpace(matchId) ||
                string.IsNullOrWhiteSpace(hostUsername) ||
                string.IsNullOrWhiteSpace(targetUsername))
                return;

            lock (syncRoot)
            {
                if (!roomPlayers.ContainsKey(matchId)) return;

                var players = roomPlayers[matchId];
                var host = players.FirstOrDefault(p => p.IsHost);
                if (host == null || !string.Equals(host.PlayerUsername, hostUsername, StringComparison.OrdinalIgnoreCase))
                    return;

                if (!roomBans.ContainsKey(matchId))
                    roomBans[matchId] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                roomBans[matchId].Add(targetUsername);
            }

            LeaveGame(matchId, targetUsername);
        }
    }       
} 