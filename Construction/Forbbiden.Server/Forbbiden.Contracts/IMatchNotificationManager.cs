using System.ServiceModel;

namespace Forbbiden.Contracts
{
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
