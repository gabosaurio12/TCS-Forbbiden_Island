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
        private const string ERROR_CODE = "[ERROR] MatchManager.cs - ";

        public int CreateMatch(CreateMatchRequest request)
        {
            log.Info("Creating new match");

            try
            {
                using (var db = new Forbbiden_FEIEntities())
                {
                    var newMatch = new Matches
                    {
                        match_difficulty = request.Difficulty,
                        match_visibility = request.Visibility
                    };

                    db.Matches.Add(newMatch);
                    db.SaveChanges();

                    log.Info($"Match created successfully (ID: {newMatch.match_id})");
                    return newMatch.match_id;
                }
            }
            catch (DbEntityValidationException ex)
            {
                Console.WriteLine(ERROR_CODE + ex.Message);
                log.Error(CLASS_NAME, ex);
                throw new FaultException("Error validating match entity.");
            }
            catch (EntityException ex)
            {
                Console.WriteLine(ERROR_CODE + ex.Message);
                log.Error(CLASS_NAME, ex);
                throw new FaultException("Database connection error while creating match.");
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

                    var player = db.Player.FirstOrDefault(p => p.player_username == request.Username);
                    if (player == null)
                    {
                        log.Warn($"Player {request.Username} not found");
                        return false;
                    }

                    bool alreadyJoined = db.match_players
                        .Any(mp => mp.match_id == match.match_id && mp.player_id == player.player_id);

                    if (alreadyJoined)
                    {
                        log.Warn($"Player {request.Username} already in match {match.match_id}");
                        return false;
                    }
                    var newJoin = new match_players
                    {
                        match_id = match.match_id,
                        player_id = player.player_id
                    };

                    db.match_players.Add(newJoin);
                    db.SaveChanges();

                    log.Info($"Player {request.Username} successfully joined match {match.match_id}");
                    return true;
                }
            }
            catch (EntityException ex)
            {
                Console.WriteLine(ERROR_CODE + ex.Message);
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
                    var dbMatches = db.Matches.ToList();

                    // Convertimos las entidades del servidor a los contratos
                    return dbMatches.Select(m => new Contracts.Match
                    {
                        MatchId = m.match_id,
                        Difficulty = m.match_difficulty,
                        Visibility = m.match_visibility,
                        Players = new List<PlayerInfo>() // pendiente de llenar
                    }).ToList();
                }
            }
            catch (EntityException ex)
            {
                Console.WriteLine(ERROR_CODE + ex.Message);
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
                    var match = db.Matches.FirstOrDefault(m => m.match_id == matchId);
                    if (match == null)
                    {
                        log.Warn("Match not found");
                        return null;
                    }

                    return new Contracts.Match
                    {
                        MatchId = match.match_id,
                        Difficulty = match.match_difficulty,
                        Visibility = match.match_visibility,
                        Players = new List<PlayerInfo>()
                    };
                }
            }
            catch (EntityException ex)
            {
                Console.WriteLine(ERROR_CODE + ex.Message);
                log.Error(CLASS_NAME, ex);
                throw new FaultException("Database connection error while retrieving match.");
            }
        }
    }
}
