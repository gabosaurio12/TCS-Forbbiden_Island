using Forbbiden.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Forbbiden.Contracts
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IFriendRequestCallback" in both code and config file together.
    [ServiceContract]
    public interface IFriendRequestCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnFriendRequestReceived(FriendRequest friendRequest);
    }
}
