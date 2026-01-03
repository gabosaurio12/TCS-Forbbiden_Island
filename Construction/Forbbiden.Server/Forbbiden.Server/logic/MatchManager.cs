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
        public int CreateMatch(CreateMatchRequest request)
        {
            log.Info("Creating new match");
            int matchId = 0;

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

        public bool JoinMatch(JoinMatchRequest request)
        {
            try
            {
                using (var db = new Forbbiden_FEIEntities(connectionString))
                {
                    var match = db.Matches.FirstOrDefault(m => m.match_id == request.MatchId);
                    if (match == null)
                        return false;

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
                                       Difficulty = m.match_difficulty,
                                       Visibility = m.match_visibility,
                                       CreatedAt = m.created_at ?? DateTime.Now,
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
    }
}
