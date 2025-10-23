using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Forbbiden.Contracts;

namespace Forbbiden.Server.logic
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class FriendsManager : IFriendsManager
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FriendsManager));

        public bool AcceptFriendRequest(string senderUsername, string receiverUsername)
        {
            throw new NotImplementedException();
        }

        public bool AddSendFriendRequest(string senderUsername, string receiverUsername)
        {
            throw new NotImplementedException();
        }
    }
}
