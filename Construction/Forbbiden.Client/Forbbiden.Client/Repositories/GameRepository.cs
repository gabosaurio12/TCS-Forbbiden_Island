using Forbbiden.Client.ErrorCodes;
using Forbbiden.Client.Exceptions;
using Forbbiden.Client.GameManager;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Forbbiden.Client.Repositories
{

    public class GameRepository : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GameRepository));
        private readonly GameManagerClient GameClient;

        public GameRepository(IGameManagerCallback callback, string endpointConfigurationName = "NetTcpBinding_IGameManager")
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            var context = new InstanceContext(callback);
            GameClient = new GameManagerClient(context, endpointConfigurationName);
        }

        public GameManagerClient Client => GameClient;

        public async Task<bool> JoinGame(string matchId, string playerName, byte[] avatarBytes, string avatarFileName)
        {
            bool result;
            try
            {
                result = await GameClient.JoinGameAsync(matchId, playerName, avatarBytes, avatarFileName);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("GameRepository.JoinGame", ex);
                throw new ViewException(ServerErrorCodes.pushingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("GameRepository.JoinGame", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return result;
        }

        public async Task LeaveGame(string matchId, string playerName)
        {
            try
            {
                await GameClient.LeaveGameAsync(matchId, playerName);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("GameRepository.LeaveGame", ex);
                throw new ViewException(ServerErrorCodes.pushingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("GameRepository.LeaveGame", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }
        }

        public async Task SendChatMessage(string matchId, string playerName, string message)
        {
            try
            {
                await GameClient.SendChatMessageAsync(matchId, playerName, message);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("GameRepository.SendChatMessage", ex);
                throw new ViewException(ServerErrorCodes.pushingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("GameRepository.SendChatMessage", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }
        }

        public async Task<List<PlayerInfo>> GetPlayers(string matchId)
        {
            List<PlayerInfo> result;
            try
            {
                result = (await GameClient.GetPlayersAsync(matchId))?.ToList() ?? new List<PlayerInfo>();
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("GameRepository.GetPlayers", ex);
                throw new ViewException(ServerErrorCodes.pullingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("GameRepository.GetPlayers", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return result;
        }

        public async Task SetReady(string matchId, string username, bool ready)
        {
            try
            {
                await GameClient.SetReadyAsync(matchId, username, ready);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("GameRepository.SetReady", ex);
                throw new ViewException(ServerErrorCodes.pushingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("GameRepository.SetReady", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }
        }

        public async Task StartMatch(string matchId, string username)
        {
            try
            {
                await GameClient.StartMatchAsync(matchId, username);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("GameRepository.StartMatch", ex);
                throw new ViewException(ServerErrorCodes.pushingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("GameRepository.StartMatch", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }
        }

        public async Task KickPlayer(string matchId, string hostUsername, string targetUsername)
        {
            try
            {
                await GameClient.KickPlayerAsync(matchId, hostUsername, targetUsername);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("GameRepository.KickPlayer", ex);
                throw new ViewException(ServerErrorCodes.pushingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("GameRepository.KickPlayer", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }
        }

        public void Dispose()
        {
            try
            {
                if (GameClient.State == CommunicationState.Faulted)
                {
                    GameClient.Abort();
                }
                else
                {
                    GameClient.Close();
                }
            }
            catch
            {
                GameClient.Abort();
            }
        }
    }
}
