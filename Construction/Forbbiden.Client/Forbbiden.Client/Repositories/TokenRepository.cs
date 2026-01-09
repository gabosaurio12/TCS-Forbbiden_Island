using Forbbiden.Client.TokenManager;
using log4net;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Forbbiden.Client.Repositories
{
    public class TokenRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TokenRepository));
        private readonly TokenManagerClient TokenClient = new TokenManagerClient();

        public async Task<Token> GenerateToken(int playerId)
        {
            Token token = null;
            try
            {
                token = await TokenClient.GenerateTokenAsync(playerId);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("TokenRepository.GenerateToken", ex);
            }
            catch (TimeoutException ex)
            {
                Log.Error("TokenRepository.GenerateToken", ex);
            }

            return token;
        }

        public async Task<Token> GetToken(int playerId)
        {
            Token token = null;
            try
            {
                token = await TokenClient.GetTokenAsync(playerId);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("TokenRepository.GenerateToken", ex);
            }
            catch (TimeoutException ex)
            {
                Log.Error("TokenRepository.GenerateToken", ex);
            }

            return token;
        }

        public async Task<bool> VerifyToken(string token, int playerId)
        {
            bool result = false;
            try
            {
                result = await TokenClient.VerifyTokenAsync(token, playerId);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("TokenRepository.VerifyToken", ex);
            }
            catch (TimeoutException ex)
            {
                Log.Error("TokenRepository.VerifyToken", ex);
            }
            return result;
        }
    }
}
