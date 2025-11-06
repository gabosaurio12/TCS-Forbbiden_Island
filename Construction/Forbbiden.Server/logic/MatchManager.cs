using Forbbiden.Contracts;
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

        public int CreateMatch(CreateMatchRequest request)
        {
            log.Info("Creating new match");

            try
            {
                using (var db = new Forbbiden_FEIEntities())
                {
                    int hostId = db.Player
                        .Where(p => p.player_username == request.HostUsername)
                        .Select(p => p.player_id)
                        .FirstOrDefault();

                    if (hostId == 0)
                    {
                        throw new FaultException("Host player not found.");
                    }

                    var newMatch = new Matches
                    {
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

                    return newMatch.match_id;
                }

            }
            catch (DbEntityValidationException ex)
            {
                log.Error(CLASS_NAME, ex);
                throw new FaultException("Error validating match entity.");
            }
            catch (EntityException ex)
            {
                log.Error(CLASS_NAME, ex);
                throw new FaultException("Database connection error while creating match.");
            }
            catch (Exception ex)
            {
                log.Error(CLASS_NAME, ex);
                Console.WriteLine("Detalles del error: " + ex);
                throw new FaultException("Unexpected error while creating match: " + ex.Message);
            }
        }

        public bool JoinMatch(JoinMatchRequest request)
        {
            log.Info($"Player {request.Username} joining match {request.MatchId}");

            try
            {
                using (var db = new Forbbiden_FEIEntities())
                {
                    var match = db.Matches.FirstOrDefault(m => m.match_id == request.MatchId);
                    if (match == null)
                    {
                        log.Warn($"Match {request.MatchId} not found");
                        return false;
                    }

                    Player player = db.Player.FirstOrDefault(p => p.player_username == request.Username);
                    int playerId;

                    if (player != null)
                    {
                        playerId = player.player_id;
                    }
                    else
                    {
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
                    {
                        log.Warn($"Player {request.Username} already in match {match.match_id}");
                        return false;
                    }

                    db.match_players.Add(new match_players
                    {
                        match_id = match.match_id,
                        player_id = playerId
                    });

                    db.SaveChanges();

                    log.Info($"Player {request.Username} successfully joined match {match.match_id}");
                    return true;
                }
            }
            catch (EntityException ex)
            {
                log.Error(CLASS_NAME, ex);
                throw new FaultException("Database connection error while joining match.");
            }
        }

        public List<Contracts.Match> ListMatches()
        {
            log.Info("Listing all matches");

            try
            {
                using (var db = new Forbbiden_FEIEntities())
                {
                    var matches = (from m in db.Matches
                                   join host in db.Player on m.host_id equals host.player_id
                                   select new Contracts.Match
                                   {
                                       MatchId = m.match_id,
                                       Difficulty = m.match_difficulty,
                                       Visibility = m.match_visibility,
                                       CreatedAt = m.created_at ?? DateTime.Now,
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
                                                          : "Guest" + Math.Abs(mp.player_id),
                                                      PlayerName = mp.player_id > 0
                                                          ? p.player_name
                                                          : "Guest",
                                                      IsHost = mp.player_id == m.host_id
                                                  }).ToList()
                                   }).ToList();

                    return matches;
                }
            }
            catch (EntityException ex)
            {
                log.Error(CLASS_NAME, ex);
                throw new FaultException("Error retrieving matches from database.");
            }
        }

        public Contracts.Match GetMatchById(int matchId)
        {
            log.Info($"Retrieving match by ID: {matchId}");

            try
            {
                using (var db = new Forbbiden_FEIEntities())
                {
                    var match = (from m in db.Matches
                                 join host in db.Player on m.host_id equals host.player_id
                                 where m.match_id == matchId
                                 select new Contracts.Match
                                 {
                                     MatchId = m.match_id,
                                     Difficulty = m.match_difficulty,
                                     Visibility = m.match_visibility,
                                     CreatedAt = m.created_at ?? DateTime.Now,
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
                                                        : "Guest" + Math.Abs(mp.player_id),
                                                    PlayerName = mp.player_id > 0
                                                        ? p.player_name
                                                        : "Guest",
                                                    IsHost = mp.player_id == m.host_id
                                                }).ToList()
                                 }).FirstOrDefault();

                    if (match == null)
                    {
                        log.Warn("Match not found");
                        return null;
                    }

                    return match;
                }
            }
            catch (EntityException ex)
            {
                log.Error(CLASS_NAME, ex);
                throw new FaultException("Database connection error while retrieving match.");
            }
        }
    }
}
