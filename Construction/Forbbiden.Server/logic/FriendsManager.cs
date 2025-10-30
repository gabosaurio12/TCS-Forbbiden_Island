using Forbbiden.Contracts;
using log4net;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
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
        private const string CLASS_NAME = "FriendsManager.cs";
        private const string ERROR_CODE = "[ERROR] FriendsManager.cs - ";

        public bool AcceptFriendRequest(string senderUsername, string receiverUsername)
        {
            throw new NotImplementedException();
        }

        public bool SendFriendRequest(string senderUsername, string receiverUsername)
        {
            var playerManager = new ProfileManager();
            var sender = playerManager.GetPlayerByUsername(senderUsername);
            var receiver = playerManager.GetPlayerByUsername(receiverUsername);
            bool success = false;

            if (sender == null || receiver == null)
            {
                log.Warn("AddSendFriendRequest: One of the users does not exist.");
                return false;
            }
            else
            {
                using (var db = new Forbbiden_FEIEntities())
                {
                    Friend_Request friendRequest = new Friend_Request
                    {
                        player_id = sender.PlayerId,
                        friend_id = receiver.PlayerId,
                        status = 0
                    };
                    try
                    {
                        db.Friend_Request.Add(friendRequest);
                        db.SaveChanges();
                        success = true;
                    }
                    catch (EntityException ex)
                    {
                        Console.WriteLine(ERROR_CODE + ex.Message);
                        log.Error(CLASS_NAME, ex);
                        throw;
                        
                    }
                }
            }

            return success;
        }

        public bool CancelFriendRequest(string senderUsername, string receiverUsername)
        {
            bool success = false;
            try
            {
                using (var db = new Forbbiden_FEIEntities())
                {
                    var sender = db.Player.FirstOrDefault(s => s.player_username == senderUsername);
                    var receiver = db.Player.FirstOrDefault(r => r.player_username == receiverUsername);
                    var friendRequest = db.Friend_Request.FirstOrDefault(fr => fr.player_id == sender.player_id && fr.friend_id == receiver.player_id);
                    if (friendRequest != null)
                    {
                        db.Friend_Request.Remove(friendRequest);
                        db.SaveChanges();
                        success = true;
                    }
                    
                }
            }
            catch (EntityException ex)
            {
                Console.WriteLine(ERROR_CODE + ex.Message);
                log.Error(CLASS_NAME, ex);
                throw;
            }

            return success;
        }
    }
}
