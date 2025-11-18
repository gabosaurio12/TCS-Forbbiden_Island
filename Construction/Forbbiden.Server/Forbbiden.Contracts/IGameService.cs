using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Forbbiden.Contracts
{ 
    [ServiceContract(CallbackContract = typeof(IGameServiceCallback))]
    public interface IGameService
    {
        [OperationContract]
        bool JoinGame(string matchId, string playerName);

        [OperationContract]
        void LeaveGame(string matchId, string playerName);

        [OperationContract]
        void SendChatMessage(string matchId, string playerName, string message);

        [OperationContract]
        List<string> GetPlayers(string matchId);
    }

    public interface IGameServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnPlayerJoined(string playerName);

        [OperationContract(IsOneWay = true)]
        void OnPlayerLeft(string playerName);

        [OperationContract(IsOneWay = true)]
        void OnChatMessage(string playerName, string message);
    }

}
