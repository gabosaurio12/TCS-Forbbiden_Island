using Forbbiden.Contracts;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Forbbiden.Server.logic
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "FriendsManager" in both code and config file together.

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
            var playerManager = new ProfileManager();
            var sender = playerManager.GetPlayerByUsername(senderUsername);
            var receiver = playerManager.GetPlayerByUsername(receiverUsername);
            if (sender == null || receiver == null)
            {
                log.Warn("AddSendFriendRequest: One of the users does not exist.");
                return false;
            }

            return true;
        }
    }
}
