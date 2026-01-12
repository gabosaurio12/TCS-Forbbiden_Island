using Forbbiden.Client.ErrorCodes;
using Forbbiden.Client.Exceptions;
using Forbbiden.Client.MatchManager;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Forbbiden.Client.Repositories
{
    public class MatchRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MatchRepository));
        private readonly MatchManagerClient MatchClient = new MatchManagerClient();

        public async Task<int> CreateMatch(CreateMatchRequest request)
        {
            int matchId;
            try
            {
                matchId = await MatchClient.CreateMatchAsync(request);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("MatchRepository.CreateMatch", ex);
                throw new ViewException(ServerErrorCodes.pushingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("MatchRepository.CreateMatch", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return matchId;
        }

        public async Task<string> GetInviteCode(int matchId)
        {
            string code;
            try
            {
                code = await MatchClient.GetInviteCodeAsync(matchId);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("MatchRepository.GetInviteCode", ex);
                throw new ViewException(ServerErrorCodes.pullingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("MatchRepository.GetInviteCode", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return code;
        }

        public async Task<bool> ValidateInvite(int matchId, string code)
        {
            bool result;
            try
            {
                result = await MatchClient.ValidateInviteAsync(matchId, code);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("MatchRepository.ValidateInvite", ex);
                throw new ViewException(ServerErrorCodes.pullingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("MatchRepository.ValidateInvite", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return result;
        }

        public async Task<bool> JoinMatch(JoinMatchRequest request)
        {
            bool joined;
            try
            {
                joined = await MatchClient.JoinMatchAsync(request);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("MatchRepository.JoinMatch", ex);
                throw new ViewException(ServerErrorCodes.pushingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("MatchRepository.JoinMatch", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return joined;
        }

        public async Task<List<Match>> ListMatches()
        {
            Match[] matches;
            try
            {
                matches = await MatchClient.ListMatchesAsync();
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("MatchRepository.ListMatches", ex);
                throw new ViewException(ServerErrorCodes.pullingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("MatchRepository.ListMatches", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return matches?.ToList() ?? new List<Match>();
        }

        public async Task<Match> GetMatchById(int matchId)
        {
            Match match;
            try
            {
                match = await MatchClient.GetMatchByIdAsync(matchId);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("MatchRepository.GetMatchById", ex);
                throw new ViewException(ServerErrorCodes.pullingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("MatchRepository.GetMatchById", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return match;
        }

        public async Task<bool> DeleteMatch(int matchId)
        {
            bool deleted;
            try
            {
                deleted = await MatchClient.DeleteMatchAsync(matchId);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("MatchRepository.DeleteMatch", ex);
                throw new ViewException(ServerErrorCodes.pullingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("MatchRepository.DeleteMatch", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return deleted;
        }
    }
}