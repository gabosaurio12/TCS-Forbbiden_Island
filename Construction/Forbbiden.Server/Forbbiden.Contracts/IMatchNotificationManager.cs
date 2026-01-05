using System.Collections.Generic;
using System.ServiceModel;

namespace Forbbiden.Contracts
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IMatchNotificationManager" in both code and config file together.
    [ServiceContract(CallbackContract = typeof(IMatchCallback))]
    public interface IMatchNotificationManager
    {
        [OperationContract]
        void Subscribe(string username);
        [OperationContract]
        void Unsubscribe(string username);
    }

    public interface IMatchCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnBoardCreatedCallback(string boardJson);
        [OperationContract(IsOneWay = true)]
        void OnBoardUpdatedCallback(string boardJson);
        [OperationContract(IsOneWay = true)]
        void OnPlayersTurnCallback();
        [OperationContract(IsOneWay = true)]
        void OnTurnFinishedCallback(string boardJson);
    }
}
