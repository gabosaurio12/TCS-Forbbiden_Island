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
        private int SenderID { get; set; }
        [DataMember]
        private int ReceiverID { get; set; }
        [DataMember]
        private bool Status { get; set; }
    }
}
