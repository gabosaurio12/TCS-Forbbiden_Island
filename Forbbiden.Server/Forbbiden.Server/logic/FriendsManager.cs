using Forbbiden.Contracts;
using Forbbiden.Server.utils;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.ServiceModel;

namespace Forbbiden.Server.logic
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "FriendsManager" in both code and config file together.

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class FriendsManager : IFriendsManager
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FriendsManager));
        private const string ErrorCode = "[ERROR] FriendsManager.cs - ";
        private readonly string connectionString;

        public FriendsManager()
        {
            connectionString = ConnectionStringSingleton.GetInstance().connectionString;
        }

        public bool AcceptFriendRequest(string senderUsername, string receiverUsername)
        {
            bool success = false;
            using (var db = new Forbbiden_FEIEntities(connectionString))
            {
                var profileClient = new ProfileManager();
                try
                {
                    var sender = profileClient.GetPlayerByUsername(senderUsername);
                    var receiver = profileClient.GetPlayerByUsername(receiverUsername);

                    if (sender.PlayerId != -1 && receiver.PlayerId != -1)
                    {

                        var friendRequest = db.Friends.FirstOrDefault(fr => fr.player_id == sender.PlayerId && fr.friend_id == receiver.PlayerId);

                        if (friendRequest != null)
                        {
                            friendRequest.status = 1;
                            db.SaveChanges();
                            success = true;
                        }
                    }
                }
                catch (EntityException ex)
                {
                    Console.WriteLine(ErrorCode + ex);
                    log.Error(ex);
                }
            }

            return success;
        }

        public bool SendFriendRequest(string senderUsername, string receiverUsername)
        {
            var playerManager = new ProfileManager();
            bool success = false;
            try
            {
                var sender = playerManager.GetPlayerByUsername(senderUsername);
                var receiver = playerManager.GetPlayerByUsername(receiverUsername);
                if (sender.PlayerId == -1 || receiver.PlayerId == -1)
                {
                    log.Warn("AddSendFriendRequest: One of the users does not exist.");
                    success = false;
                }
                else
                {
                    
                    using (var db = new Forbbiden_FEIEntities(connectionString))
                    {
                        var searchFriendRequest = db.Friends.FirstOrDefault(
                            sfr => sfr.player_id == sender.PlayerId && sfr.friend_id == receiver.PlayerId);

                        if (searchFriendRequest == null)
                        {
                            Friends friendRequest = new Friends
                            {
                                player_id = sender.PlayerId,
                                friend_id = receiver.PlayerId,
                                status = 0
                            };

                            db.Friends.Add(friendRequest);
                            db.SaveChanges();
                            success = true;
                        }
                    }
                }
            }
            catch (EntityException ex)
            {
                Console.WriteLine(ErrorCode + ex);
                log.Error(ex);
            }

            return success;
        }

        public bool CancelFriendRequest(string senderUsername, string receiverUsername)
        {
            bool success = false;
            
            using (var db = new Forbbiden_FEIEntities(connectionString))
            {
                var profileClient = new ProfileManager();
                try
                {
                    var sender = profileClient.GetPlayerByUsername(senderUsername);
                    var receiver = profileClient.GetPlayerByUsername(receiverUsername);
                    if (sender.PlayerId != -1 && receiver.PlayerId != -1)
                    {
                        var friendRequest = db.Friends.FirstOrDefault(fr => fr.player_id == sender.PlayerId && fr.friend_id == receiver.PlayerId);
                        if (friendRequest != null)
                        {
                            db.Friends.Remove(friendRequest);
                            db.SaveChanges();
                            success = true;
                        }
                    }
                }
                catch(EntityException ex)
                {
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
                }
            }

            return success;
        }

        public List<FriendRequest> getFriendRequests(string receiverUsername)
        {
            using (var db = new Forbbiden_FEIEntities(connectionString))
            {
                try
                {
                    var profileClient = new ProfileManager();
                    var receiver = profileClient.GetPlayerByUsername(receiverUsername);

                    if (receiver.PlayerId != -1)
                    {
                        var requests = db.Friends.Where(fr => fr.friend_id == receiver.PlayerId && fr.status == 0).ToList();

                        var friendRequests = new List<FriendRequest>();

                        foreach (Friends friend in requests)
                        {
                            var request = new FriendRequest
                            {
                                SenderID = friend.player_id,
                                ReceiverID = friend.friend_id,
                                Status = friend.status
                            };
                            friendRequests.Add(request);
                        }

                        return friendRequests;
                    }
                }
                catch (EntityException ex)
                {
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
                }
            }
            return new List<FriendRequest>();
        }

        public List<Friendship> GetFriendsByID(int playerID)
        {
            using (var db = new Forbbiden_FEIEntities(connectionString))
            {
                var friends = new List<Friends>();
                try
                {
                    friends = db.Friends.Where(f =>
                        (f.player_id == playerID ||
                        f.friend_id == playerID) &&
                        f.status == 1).ToList();
                }
                catch (EntityException ex)
                {
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
                }

                var profileManager = new ProfileManager();
                var friendships = new List<Friendship>();
                foreach (var friend in friends)
                {
                    int friendID = friend.player_id == playerID ?
                        friend.friend_id : friend.player_id;
                    var friendship = new Friendship
                    {
                        PlayerID = playerID,
                        Friend = profileManager.GetPlayerById(friendID)
                    };
                    friendships.Add(friendship);
                }

                return friendships;
            }
        }
    }
}
