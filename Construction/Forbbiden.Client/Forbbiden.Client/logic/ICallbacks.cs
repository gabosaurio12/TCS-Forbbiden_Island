using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Forbbiden.Client.logic
{
    [ServiceContract(CallbackContract = typeof(ICallbacks))]
    public interface ICallbacks
    {
        [OperationContract(IsOneWay = true)]
        void OnFriendRequestReceived(FriendsManager.FriendRequest request);
    }
}
