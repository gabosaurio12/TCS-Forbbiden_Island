using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Forbbiden.Contracts
{
    [ServiceContract]
    public interface IFriendsManager
    {
        [OperationContract]
        bool AddSendFriendRequest(string senderUsername, string receiverUsername);
        [OperationContract]
        bool AcceptFriendRequest(string senderUsername, string receiverUsername);
    }
}
