using Forbbiden.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace Forbbiden.Server.logic
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple
    )]
    public class GameManager : IGameManager
    {
        // Callbacks por sala (matchId -> callbacks)
        private readonly Dictionary<string, List<IGameManagerCallback>> rooms = new Dictionary<string, List<IGameManagerCallback>>();

        // PlayerInfo por sala (mantiene posición y avatar)
        private readonly Dictionary<string, List<PlayerInfo>> roomPlayers = new Dictionary<string, List<PlayerInfo>>();

        // Callback => matchId (para limpieza)
        private readonly Dictionary<IGameManagerCallback, string> callbackToRoom = new Dictionary<IGameManagerCallback, string>();

        // Ready states por sala: matchId -> set of usernames ready
        private readonly Dictionary<string, HashSet<string>> matchReady = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        private readonly object syncRoot = new object();

        // ------------------- JoinGame -------------------
        public bool JoinGame(string matchId, string playerName, byte[] avatarBytes, string avatarFileName)
        {
            var callback = OperationContext.Current.GetCallbackChannel<IGameManagerCallback>();
            lock (syncRoot)
            {
                if (!rooms.ContainsKey(matchId))
                {
                    rooms[matchId] = new List<IGameManagerCallback>();
                    roomPlayers[matchId] = new List<PlayerInfo>();
                    matchReady[matchId] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }

                // registrar callback si no está
                if (!rooms[matchId].Contains(callback))
                    rooms[matchId].Add(callback);

                // Evitar duplicados por username; si ya existe no agregamos duplicado
                var existing = roomPlayers[matchId].FirstOrDefault(p => string.Equals(p.PlayerUsername, playerName, StringComparison.OrdinalIgnoreCase));
                if (existing == null)
                {
                    var pos = roomPlayers[matchId].Count;
                    var pinfo = new PlayerInfo
                    {
                        PlayerId = 0,
                        PlayerUsername = playerName,
                        PlayerName = playerName,
                        IsHost = (pos == 0), // el primero es host por convención (ajusta si necesitas otra lógica)
                        Position = pos,
                        AvatarBytes = (avatarBytes != null && avatarBytes.Length > 0) ? avatarBytes : null,
                        AvatarFileName = string.IsNullOrEmpty(avatarFileName) ? null : avatarFileName
                    };

                    // añadir
                    roomPlayers[matchId].Add(pinfo);
                }
                else
                {
                    // reconexión / actualización: actualizamos avatar si llega
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

            // broadcast con snapshot
            BroadcastPlayersUpdate(matchId);
            return true;
        }

        // ------------------- LeaveGame -------------------
        public void LeaveGame(string matchId, string playerName)
        {
            var callback = OperationContext.Current.GetCallbackChannel<IGameManagerCallback>();
            lock (syncRoot)
            {
                if (!rooms.ContainsKey(matchId)) return;

                // remover callback
                if (rooms[matchId].Contains(callback))
                    rooms[matchId].Remove(callback);

                if (callbackToRoom.ContainsKey(callback))
                    callbackToRoom.Remove(callback);

                // remover player del roomPlayers
                var removedCount = roomPlayers[matchId].RemoveAll(p => string.Equals(p.PlayerUsername, playerName, StringComparison.OrdinalIgnoreCase));

                // limpiar ready si estaba marcado
                if (matchReady.TryGetValue(matchId, out var set) && set.Remove(playerName))
                {
                    // notificar a los demás que este jugador ya no está listo
                    BroadcastReadyState(matchId, playerName, false);
                }

                // reindex positions
                if (roomPlayers[matchId].Count == 0)
                {
                    // si ya no quedan jugadores limpiamos estructuras
                    rooms.Remove(matchId);
                    roomPlayers.Remove(matchId);
                    matchReady.Remove(matchId);
                }
                else
                {
                    for (int i = 0; i < roomPlayers[matchId].Count; i++)
                        roomPlayers[matchId][i].Position = i;
                }
            }

            BroadcastPlayersUpdate(matchId);
        }

        // ------------------- SendChatMessage -------------------
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
                catch { /* ignorar client muerto aquí; cleanup en BroadcastPlayersUpdate */ }
            }
        }

        // ------------------- GetPlayers -------------------
        public List<PlayerInfo> GetPlayers(string matchId)
        {
            lock (syncRoot)
            {
                if (!roomPlayers.ContainsKey(matchId))
                    return new List<PlayerInfo>();

                // devolver copia
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

        // ------------------- SetReady -------------------
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

            // notificar cambio a todos en la sala (UNA vez)
            BroadcastReadyState(matchId, username, ready);

            // comprobar auto-start: si sala está llena y todos listos -> iniciar
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
                // iniciar partida automáticamente
                BroadcastMatchStarting(matchId);
                // Si quieres, aquí además puedes marcar la match como started y limpiar estructuras
            }
        }

        // ------------------- StartMatch -------------------
        public void StartMatch(string matchId, string username)
        {
            if (string.IsNullOrEmpty(matchId) || string.IsNullOrEmpty(username)) return;

            // validar que el username es host de la sala (según roomPlayers)
            bool isHost = false;
            int currentPlayers = 0;
            int readyCount = 0;
            lock (syncRoot)
            {
                if (roomPlayers.TryGetValue(matchId, out var players))
                {
                    currentPlayers = players.Count;
                    var host = players.FirstOrDefault(p => p.IsHost);
                    if (host != null && string.Equals(host.PlayerUsername, username, StringComparison.OrdinalIgnoreCase))
                        isHost = true;
                }

                if (matchReady.TryGetValue(matchId, out var rset))
                    readyCount = rset.Count;
            }

            if (!isHost) return;

            // política: permitir Start sólo si todos los presentes están listos
            if (currentPlayers > 0 && readyCount == currentPlayers)
            {
                BroadcastMatchStarting(matchId);
                // transición server-side if needed
            }
            else
            {
                // no permitido: host intentó iniciar sin que todos estén listos -> ignorar
            }
        }

        // ------------------- Broadcast helpers -------------------
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

            var failed = new List<IGameManagerCallback>();
            foreach (var client in toNotify)
            {
                try { client.OnPlayersUpdated(playersSnapshot); }
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

                    var empties = rooms.Where(kv => kv.Value.Count == 0).Select(kv => kv.Key).ToList();
                    foreach (var e in empties)
                    {
                        rooms.Remove(e);
                        roomPlayers.Remove(e);
                        matchReady.Remove(e);
                    }
                }
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
    }
}