using Forbbiden.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Forbbiden.Server.logic
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "FriendRequestCallback" in both code and config file together.
    public class FriendRequestCallback : IFriendRequestCallback
    {
        public void OnFriendRequestReceived(FriendRequest friendRequest)
        {
            throw new NotImplementedException();
        }
    }
}
