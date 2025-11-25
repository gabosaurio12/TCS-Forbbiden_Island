using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Forbbiden.Contracts
{
    [ServiceContract(CallbackContract = typeof(IFriendRequestCallback))]
    public interface IFriendsManager
    {
        [OperationContract]
        bool SendFriendRequest(string senderUsername, string receiverUsername);
        [OperationContract]
        bool AcceptFriendRequest(string senderUsername, string receiverUsername);
        [OperationContract]
        bool CancelFriendRequest(string senderUsername, string receiverUsername);
        [OperationContract]
        List<FriendRequest> GetFriendRequests(string receiverUsername);
        [OperationContract]
        List<Friendship> GetFriendsByID(int playerID);
    }

    [DataContract]
    public class FriendRequest
    {
        [DataMember]
        public int SenderID { get; set; }
        [DataMember]
        public int ReceiverID { get; set; }
        [DataMember]
        public int Status { get; set; }
    }

    [DataContract]
    public class Friendship
    {
        [DataMember]
        public int PlayerID { get; set; }
        [DataMember]
        public Player Friend { get; set; }
    }
}
