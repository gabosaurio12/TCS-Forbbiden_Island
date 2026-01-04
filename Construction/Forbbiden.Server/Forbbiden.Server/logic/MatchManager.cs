using Forbbiden.Contracts;
using Forbbiden.Server.exceptionHandlers;
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
        private static readonly ILog log = LogManager.GetLogger(typeof(MatchManager));
        private const string CLASS_NAME = "MatchManager.cs";
        private readonly string Guest = "Guest";
        private readonly string connectionString;
        public MatchManager()
        {
            connectionString = ConnectionStringSingleton.GetInstance().connectionString;
        }

        // CreateMatch: ahora acepta MatchName (<=20 chars) y Capacity (2..4)
        public int CreateMatch(CreateMatchRequest request)
        {
            log.Info("Creating new match");
            int matchId = 0;

            // Validaciones básicas del request
            if (request == null)
                throw new FaultException("Invalid request.");

            if (string.IsNullOrWhiteSpace(request.HostUsername))
                throw new FaultException("Host username is required.");

            // MatchName validación: si se pasa debe tener <= 20 chars
            if (!string.IsNullOrEmpty(request.MatchName) && request.MatchName.Length > 20)
                throw new FaultException("Match name must be max 20 characters.");

            // Capacity validación: 2..4, si no especificado usar 4
            int capacity = request.Capacity;
            if (capacity < 2 || capacity > 4)
                capacity = 4;

            // Validaciones básicas del request
            if (request == null)
                throw new FaultException("Invalid request.");

            if (string.IsNullOrWhiteSpace(request.HostUsername))
                throw new FaultException("Host username is required.");

            // MatchName validación: si se pasa debe tener <= 20 chars
            if (!string.IsNullOrEmpty(request.MatchName) && request.MatchName.Length > 20)
                throw new FaultException("Match name must be max 20 characters.");

            // Capacity validación: 2..4, si no especificado usar 4
            int capacity = request.Capacity;
            if (capacity < 2 || capacity > 4)
                capacity = 4;

            try
            {
                using (var db = new Forbbiden_FEIEntities(connectionString))
                {
                    int hostId = db.Player
                        .Where(p => p.player_username == request.HostUsername)
                        .Select(p => p.player_id)
                        .FirstOrDefault();

                    if (hostId == 0)
                        throw new FaultException("Host player not found.");

                    var newMatch = new Matches
                    {
                        match_name = string.IsNullOrEmpty(request.MatchName) ? null : request.MatchName,
                        match_capacity = capacity,
                        match_difficulty = request.Difficulty,
                        match_visibility = request.Visibility,
                        host_id = hostId,
                        created_at = DateTime.Now
                    };

                    db.Configuration.AutoDetectChangesEnabled = false;
                    db.Matches.Add(newMatch);
                    db.SaveChanges();
                    db.Configuration.AutoDetectChangesEnabled = true;

                    bool hostExists = db.match_players
                        .Any(mp => mp.match_id == newMatch.match_id && mp.player_id == hostId);

                    if (!hostExists)
                    {
                        db.match_players.Add(new match_players
                        {
                            match_id = newMatch.match_id,
                            player_id = hostId
                        });
                        db.SaveChanges();
                    }

                    matchId = newMatch.match_id;
                }
            }
            catch (DbEntityValidationException ex)
            {
                ExceptionHandler.HandleEntityValidationException(ex, CLASS_NAME);
            }
            catch (EntityException ex)
            {
                ExceptionHandler.HandleEntityException(ex, CLASS_NAME);
            }

            return matchId;
        }

        // JoinMatch: ahora puede encontrar la sala por id, por match_name o por host username.
        public bool JoinMatch(JoinMatchRequest request)
        {
            if (request == null)
                throw new FaultException("Invalid request.");

            try
            {
                using (var db = new Forbbiden_FEIEntities(connectionString))
                {
                    Matches match = null;

                    // 1) Si MatchId provisto (>0), buscar por id
                    if (request.MatchId > 0)
                    {
                        match = db.Matches.FirstOrDefault(m => m.match_id == request.MatchId);
                    }
                    // 2) Si MatchName provisto, buscar por match_name (primer match activo que coincida)
                    if (match == null && !string.IsNullOrEmpty(request.MatchName))
                    {
                        match = db.Matches.FirstOrDefault(m => m.match_name == request.MatchName);
                    }
                    // 3) Si HostUsername provisto, encontrar hostId y buscar la match más reciente de ese host
                    if (match == null && !string.IsNullOrEmpty(request.HostUsername))
                    {
                        var host = db.Player.FirstOrDefault(p => p.player_username == request.HostUsername);
                        if (host != null)
                        {
                            match = db.Matches
                                .Where(m => m.host_id == host.player_id)
                                .OrderByDescending(m => m.created_at)
                                .FirstOrDefault();
                        }
                    }

                    if (match == null)
                        return false;

                    // Comprueba capacidad
                    int currentPlayersCount = db.match_players.Count(mp => mp.match_id == match.match_id);

                    // match_capacity in the EF model is non-nullable int -> use directly and validate range
                    int capacity = match.match_capacity;
                    if (capacity < 2 || capacity > 4)
                        capacity = 4;

                    if (currentPlayersCount >= capacity)
                    {
                        // Sala llena
                        return false;
                    }

                    // Determinar playerId (usuario registrado o guest)
                    Player player = null;
                    int playerId;
                    if (!string.IsNullOrEmpty(request.Username))
                    {
                        player = db.Player.FirstOrDefault(p => p.player_username == request.Username);
                    }

                    if (player != null)
                    {
                        playerId = player.player_id;
                    }
                    else
                    {
                        // guest negative id logic: encontrar el minimo negative ya usado en esta sala y restar 1
                        int minGuestId = db.match_players
                            .Where(mp => mp.match_id == match.match_id && mp.player_id < 0)
                            .Select(mp => mp.player_id)
                            .DefaultIfEmpty(0)
                            .Min();
                        playerId = minGuestId - 1;
                    }

                    bool alreadyJoined = db.match_players
                        .Any(mp => mp.match_id == match.match_id && mp.player_id == playerId);

                    if (alreadyJoined)
                        return false;

                    db.match_players.Add(new match_players
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
                ExceptionHandler.HandleEntityException(ex, CLASS_NAME);
            }

            return false;
        }

        // ListMatches: ahora incluye MatchName y Capacity en Contracts.Match
        public List<Contracts.Match> ListMatches()
        {
            log.Info("Listing all matches");

            try
            {
                using (var db = new Forbbiden_FEIEntities(connectionString))
                {
                    var matches = (from m in db.Matches
                                   join host in db.Player on m.host_id equals host.player_id
                                   select new Contracts.Match
                                   {
                                       MatchId = m.match_id,
                                       MatchName = m.match_name,
                                       // match_capacity is non-nullable int in EF model
                                       Capacity = (m.match_capacity >= 2 && m.match_capacity <= 4) ? m.match_capacity : 4,
                                       Difficulty = m.match_difficulty,
                                       Visibility = m.match_visibility,
                                       // created_at is non-nullable DateTime in EF model
                                       CreatedAt = m.created_at,
                                       HostUsername = host.player_username,
                                       Players = (from mp in db.match_players
                                                  where mp.match_id == m.match_id
                                                  join p in db.Player on mp.player_id equals 
                                                  p.player_id into joined
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
                ExceptionHandler.HandleEntityException(ex, CLASS_NAME);
            }
            return new List<Match>();
        }

        // GetMatchById: incluye MatchName y Capacity
        public Contracts.Match GetMatchById(int matchId)
        {
            Contracts.Match match = new Contracts.Match();
            try
            {
                using (var db = new Forbbiden_FEIEntities(connectionString))
                {
                    match = (from m in db.Matches
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
                                     Players = (from mp in db.match_players
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
                ExceptionHandler.HandleEntityException(ex, CLASS_NAME);
            }
            return match;
        }

        // Añadir dentro de la clase MatchManager (Server)
        public bool DeleteMatch(int matchId)
        {
            log.Info($"Deleting match {matchId}");

            try
            {
                using (var db = new Forbbiden_FEIEntities(connectionString))
                {
                    using (var tx = db.Database.BeginTransaction())
                    {
                        try
                        {
                            var match = db.Matches.FirstOrDefault(m => m.match_id == matchId);
                            if (match == null)
                            {
                                log.Warn($"DeleteMatch: match {matchId} not found");
                                return false;
                            }

                            // Eliminar participantes (match_players) asociados
                            var players = db.match_players.Where(mp => mp.match_id == matchId).ToList();
                            if (players.Any())
                            {
                                db.match_players.RemoveRange(players);
                                db.SaveChanges();
                            }

                            // Eliminar la match
                            db.Matches.Remove(match);
                            db.SaveChanges();

                            tx.Commit();
                            log.Info($"Match {matchId} deleted successfully");
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

        // Añadir dentro de la clase MatchManager (Server)
        public bool DeleteMatch(int matchId)
        {
            log.Info($"Deleting match {matchId}");

            try
            {
                using (var db = new Forbbiden_FEIEntities(connectionString))
                {
                    using (var tx = db.Database.BeginTransaction())
                    {
                        try
                        {
                            var match = db.Matches.FirstOrDefault(m => m.match_id == matchId);
                            if (match == null)
                            {
                                log.Warn($"DeleteMatch: match {matchId} not found");
                                return false;
                            }

                            // Eliminar participantes (match_players) asociados
                            var players = db.match_players.Where(mp => mp.match_id == matchId).ToList();
                            if (players.Any())
                            {
                                db.match_players.RemoveRange(players);
                                db.SaveChanges();
                            }

                            // Eliminar la match
                            db.Matches.Remove(match);
                            db.SaveChanges();

                            tx.Commit();
                            log.Info($"Match {matchId} deleted successfully");
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