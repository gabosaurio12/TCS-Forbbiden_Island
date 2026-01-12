using System.Collections.Generic;
using System.ServiceModel;

namespace Forbbiden.Contracts
{
    [ServiceContract(CallbackContract = typeof(IGameManagerCallback))]
    public interface IGameManager
    {
        [OperationContract]
        [FaultContract(typeof(Fault))]
        bool JoinGame(string matchId, string playerName, byte[] avatarBytes, string avatarFileName);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        void LeaveGame(string matchId, string playerName);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        void SendChatMessage(string matchId, string playerName, string message);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        List<PlayerInfo> GetPlayers(string matchId);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        void SetReady(string matchId, string username, bool ready);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        void StartMatch(string matchId, string username);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        void KickPlayer(string matchId, string hostUsername, string targetUsername);
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