using System.Collections.Generic;
using System.ServiceModel;

namespace Forbbiden.Contracts
{
    [ServiceContract(CallbackContract = typeof(IGameManagerCallback))]
    public interface IGameManager
    {

        [OperationContract]
        bool JoinGame(string matchId, string playerName, byte[] avatarBytes, string avatarFileName);

        [OperationContract]
        void LeaveGame(string matchId, string playerName);

        [OperationContract]
        void SendChatMessage(string matchId, string playerName, string message);


        [OperationContract]
        List<PlayerInfo> GetPlayers(string matchId);

        [OperationContract]
        void SetReady(string matchId, string username, bool ready);

        [OperationContract]
        void StartMatch(string matchId, string username);
    }

    public interface IGameManagerCallback
    {

        [OperationContract(IsOneWay = true)]
        void OnPlayersUpdated(List<PlayerInfo> players);

        [OperationContract(IsOneWay = true)]
        void OnChatMessage(string playerName, string message);

        [OperationContract(IsOneWay = true)]
        void OnGameStarting();

        [OperationContract(IsOneWay = true)]
        void ReadyStateChanged(string username, bool ready);

        [OperationContract(IsOneWay = true)]
        void MatchStarting();
    }
}