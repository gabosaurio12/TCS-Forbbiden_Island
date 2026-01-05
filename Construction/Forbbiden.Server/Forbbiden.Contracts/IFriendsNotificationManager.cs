using System.ServiceModel;

namespace Forbbiden.Contracts
{
    [ServiceContract(CallbackContract = typeof(IFriendRequestCallback))]
    public interface IFriendsNotificationManager
    {
        [OperationContract]
        void Subscribe(string username);

        [OperationContract]
        void Unsubscribe(string username);
    }

    public interface IFriendRequestCallback
    {
        [OperationContract(IsOneWay = true)]
        void NewFriendRequest(FriendRequest friendRequest);

        [OperationContract(IsOneWay = true)]
        void RefreshPageCallback(FriendRequest friendRequest);
    }
}
