using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Forbbiden.Contracts
{
    [ServiceContract]
    public interface IFriendsManager
    {
        [OperationContract]
        [FaultContract(typeof(Fault))]
        bool SendFriendRequest(string senderUsername, string receiverUsername);
        [OperationContract]
        [FaultContract(typeof(Fault))]
        bool AcceptFriendRequest(string senderUsername, string receiverUsername);
        [OperationContract]
        [FaultContract(typeof(Fault))]
        bool CancelFriendRequest(string senderUsername, string receiverUsername);
        [OperationContract]
        [FaultContract(typeof(Fault))]
        bool DeleteFriend(string friendUsername, string playerUsername);
        [OperationContract]
        [FaultContract(typeof(Fault))]
        List<FriendRequest> GetFriendRequests(string receiverUsername);
        [OperationContract]
        [FaultContract(typeof(Fault))]
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
