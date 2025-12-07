using System.Runtime.Serialization;
using System.ServiceModel;

namespace Forbbiden.Contracts
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IFriendsNotificationManager" in both code and config file together.
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
        void NewFriendship(FriendRequest friendRequest);
    }
}
