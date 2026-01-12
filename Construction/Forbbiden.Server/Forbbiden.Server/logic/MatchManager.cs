using Forbbiden.Contracts;
using Forbbiden.Server.exceptionHandlers;
using Forbbiden.Server.Model;
using Forbbiden.Server.utils;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Validation;
using System.Linq;
using System.ServiceModel;

namespace Forbbiden.Server.logic
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class MatchManager : IMatchManager
    {
        private readonly Dictionary<int, string> inviteCodes = new Dictionary<int, string>();
        private readonly object inviteLock = new object();
        private static readonly ILog log = LogManager.GetLogger(typeof(MatchManager));
        private const string ClassName = "MatchManager.cs";
        private readonly string Guest = "Guest";
        private readonly string connectionString;
        public MatchManager()
        {
            connectionString = ConnectionStringSingleton.GetInstance().ConnectionString;
        }

        private string GenerateInviteCode(int length = 6)
        {
            const string chars = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
            var rnd = new Random();
            return new string(Enumerable.Range(0, length).Select(_ => chars[rnd.Next(chars.Length)]).ToArray());
        }

        public int CreateMatch(CreateMatchRequest request)
        {
            log.Info("Creating new match");
            int matchId = 0;

            if (request == null)
                throw new FaultException("Invalid request.");
            if (string.IsNullOrWhiteSpace(request.HostUsername))
                throw new FaultException("Host username is required.");
            if (!string.IsNullOrEmpty(request.MatchName) && request.MatchName.Length > 20)
                throw new FaultException("Match name must be max 20 characters.");

            int capacity = request.Capacity;
            if (capacity < 2 || capacity > 4)
                capacity = 4;

            try
            {
                using (var db = new Forbidden_FEIEntities(connectionString))
                {
                    int hostId = db.Player
                        .Where(p => p.player_username == request.HostUsername)
                        .Select(p => p.player_id)
                        .FirstOrDefault();

                    if (hostId == 0)
                        throw new FaultException("Host player not found.");

                    var newMatch = new Model.Match
                    {
                        match_name = string.IsNullOrEmpty(request.MatchName) ? null : request.MatchName,
                        match_capacity = capacity,
                        match_difficulty = request.Difficulty,
                        match_visibility = request.Visibility,
                        host_id = hostId,
                        created_at = DateTime.Now,
                    };

                    db.Configuration.AutoDetectChangesEnabled = false;
                    db.Match.Add(newMatch);
                    db.SaveChanges();
                    db.Configuration.AutoDetectChangesEnabled = true;

                    bool hostExists = db.MatchPlayers
                        .Any(mp => mp.match_id == newMatch.match_id && mp.player_id == hostId);

                    if (!hostExists)
                    {
                        db.MatchPlayers.Add(new MatchPlayers
                        {
                            match_id = newMatch.match_id,
                            player_id = hostId
                        });
                        db.SaveChanges();
                    }

                    matchId = newMatch.match_id;
                    if (matchId > 0 && string.Equals(request.Visibility, "Private", StringComparison.OrdinalIgnoreCase))
                    {
                        var code = GenerateInviteCode();
                        lock (inviteLock) { inviteCodes[matchId] = code; }
                    }
                    return matchId;
                }
            }
            catch (DbEntityValidationException ex)
            {
                ExceptionHandler.HandleEntityValidationException(ex, ClassName, ExceptionHandler.PushingError);
            }
            catch (EntityException ex)
            {
                ExceptionHandler.HandleEntityException(ex, ClassName, ExceptionHandler.PushingError);
            }

            return matchId;
        }

        public string GetInviteCode(int matchId)
        {
            lock (inviteLock)
            {
                return inviteCodes.TryGetValue(matchId, out var code) ? code : null;
            }
        }

        public bool ValidateInvite(int matchId, string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            lock (inviteLock)
            {
                if (inviteCodes.TryGetValue(matchId, out var stored))
                    return string.Equals(stored, code, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public bool JoinMatch(JoinMatchRequest request)
        {
            if (request == null)
                throw new FaultException("Invalid request.");

            try
            {
                using (var db = new Forbidden_FEIEntities(connectionString))
                {
                    Model.Match match = null;

                    if (request.MatchId > 0)
                        match = db.Match.FirstOrDefault(m => m.match_id == request.MatchId);
                    if (match == null && !string.IsNullOrEmpty(request.MatchName))
                        match = db.Match.FirstOrDefault(m => m.match_name == request.MatchName);
                    if (match == null && !string.IsNullOrEmpty(request.HostUsername))
                    {
                        var host = db.Player.FirstOrDefault(p => p.player_username == request.HostUsername);
                        if (host != null)
                        {
                            match = db.Match
                                .Where(m => m.host_id == host.player_id)
                                .OrderByDescending(m => m.created_at)
                                .FirstOrDefault();
                        }
                    }

                    if (match == null)
                        return false;

                    int currentPlayersCount = db.MatchPlayers.Count(mp => mp.match_id == match.match_id);
                    int capacity = match.match_capacity;
                    if (capacity < 2 || capacity > 4)
                        capacity = 4;

                    if (currentPlayersCount >= capacity)
                        return false;

                    Model.Player player = null;
                    int playerId;
                    if (!string.IsNullOrEmpty(request.Username))
                        player = db.Player.FirstOrDefault(p => p.player_username == request.Username);

                    if (player != null)
                        playerId = player.player_id;
                    else
                    {
                        int minGuestId = db.MatchPlayers
                            .Where(mp => mp.match_id == match.match_id && mp.player_id < 0)
                            .Select(mp => mp.player_id)
                            .DefaultIfEmpty(0)
                            .Min();
                        playerId = minGuestId - 1;
                    }

                    bool alreadyJoined = db.MatchPlayers
                        .Any(mp => mp.match_id == match.match_id && mp.player_id == playerId);

                    if (alreadyJoined)
                        return false;

                    db.MatchPlayers.Add(new MatchPlayers
                    {
                        match_id = match.match_id,
                        player_id = playerId
                    });

                    db.SaveChanges();
                    return true;
                }
            }
            catch (EntityException ex)
            {
                ExceptionHandler.HandleEntityException(ex, ClassName, ExceptionHandler.PushingError);
            }

            return false;
        }

        public List<Contracts.Match> ListMatches()
        {
            log.Info("Listing all matches");

            try
            {
                using (var db = new Forbidden_FEIEntities(connectionString))
                {
                    var matches = (from m in db.Match
                                   join host in db.Player on m.host_id equals host.player_id
                                   select new Contracts.Match
                                   {
                                       MatchId = m.match_id,
                                       MatchName = m.match_name,
                                       Capacity = (m.match_capacity >= 2 && m.match_capacity <= 4) ? m.match_capacity : 4,
                                       Difficulty = m.match_difficulty,
                                       Visibility = m.match_visibility,
                                       CreatedAt = m.created_at,
                                       HostUsername = host.player_username,
                                       Players = (from mp in db.MatchPlayers
                                                  where mp.match_id == m.match_id
                                                  join p in db.Player on mp.player_id equals p.player_id into joined
                                                  from p in joined.DefaultIfEmpty()
                                                  select new PlayerInfo
                                                  {
                                                      PlayerId = mp.player_id,
                                                      PlayerUsername = mp.player_id > 0
                                                          ? p.player_username
                                                          : Guest + Math.Abs(mp.player_id),
                                                      PlayerName = mp.player_id > 0
                                                          ? p.player_name
                                                          : Guest,
                                                      IsHost = mp.player_id == m.host_id
                                                  }).ToList()
                                   }).ToList();

                    return matches;
                }
            }
            catch (EntityException ex)
            {
                ExceptionHandler.HandleEntityException(ex, ClassName, ExceptionHandler.PullingError);
            }
            return new List<Contracts.Match>();
        }

        public Contracts.Match GetMatchById(int matchId)
        {
            Contracts.Match match = new Contracts.Match();
            try
            {
                using (var db = new Forbidden_FEIEntities(connectionString))
                {
                    match = (from m in db.Match
                             join host in db.Player on m.host_id equals host.player_id
                             where m.match_id == matchId
                             select new Contracts.Match
                             {
                                 MatchId = m.match_id,
                                 MatchName = m.match_name,
                                 Capacity = (m.match_capacity >= 2 && m.match_capacity <= 4) ? m.match_capacity : 4,
                                 Difficulty = m.match_difficulty,
                                 Visibility = m.match_visibility,
                                 CreatedAt = m.created_at,
                                 HostUsername = host.player_username,
                                 Players = (from mp in db.MatchPlayers
                                            where mp.match_id == m.match_id
                                            join p in db.Player on mp.player_id equals p.player_id into joined
                                            from p in joined.DefaultIfEmpty()
                                            select new PlayerInfo
                                            {
                                                PlayerId = mp.player_id,
                                                PlayerUsername = mp.player_id > 0
                                                    ? p.player_username
                                                    : Guest + Math.Abs(mp.player_id),
                                                PlayerName = mp.player_id > 0
                                                    ? p.player_name
                                                    : Guest,
                                                IsHost = mp.player_id == m.host_id
                                            }).ToList()
                             }).FirstOrDefault();

                    return match;
                }
            }
            catch (EntityException ex)
            {
                ExceptionHandler.HandleEntityException(ex, ClassName, ExceptionHandler.PullingError);
            }
            return match;
        }

        public bool DeleteMatch(int matchId)
        {
            log.Info($"Deleting match {matchId}");

            try
            {
                using (var db = new Forbidden_FEIEntities(connectionString))
                {
                    using (var tx = db.Database.BeginTransaction())
                    {
                        try
                        {
                            var match = db.Match.FirstOrDefault(m => m.match_id == matchId);
                            if (match == null)
                            {
                                log.Warn($"DeleteMatch: match {matchId} not found");
                                return false;
                            }

                            var players = db.MatchPlayers.Where(mp => mp.match_id == matchId).ToList();
                            if (players.Any())
                            {
                                db.MatchPlayers.RemoveRange(players);
                                db.SaveChanges();
                            }

                            db.Match.Remove(match);
                            db.SaveChanges();

                            tx.Commit();
                            log.Info($"Match {matchId} deleted successfully");
                            lock (inviteLock) { inviteCodes.Remove(matchId); }
                            return true;
                        }
                        catch (Exception ex)
                        {
                            try { tx.Rollback(); } catch { }
                            log.Error($"Error deleting match {matchId}", ex);
                            return false;
                        }
                    }
                }
            }
            catch (EntityException ex)
            {
                log.Error("Database error in DeleteMatch", ex);
                return false;
            }
        }
    }
}