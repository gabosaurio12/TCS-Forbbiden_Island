using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Forbbiden.Contracts
{
    [ServiceContract]
    public interface IFriendsManager
    {
        [OperationContract]
        bool SendFriendRequest(string senderUsername, string receiverUsername);
        [OperationContract]
        bool AcceptFriendRequest(string senderUsername, string receiverUsername);
        [OperationContract]
        bool CancelFriendRequest(string senderUsername, string receiverUsername);
        [OperationContract]
        List<FriendRequest> getFriendRequests(string receiverUsername);
        [OperationContract]
        List<FriendRequest> getFriendsID(string receiverUsername);
    }

    [DataContract]
    public class FriendRequest
    {
        [DataMember]
        public int SenderID;
        [DataMember]
        public int ReceiverID;
        [DataMember]
        public bool status;
    }
}
