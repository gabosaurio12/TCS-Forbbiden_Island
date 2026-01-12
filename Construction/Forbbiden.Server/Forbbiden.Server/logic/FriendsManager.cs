using Forbbiden.Contracts;
using Forbbiden.Server.callbacks;
using Forbbiden.Server.exceptionHandlers;
using Forbbiden.Server.Model;
using Forbbiden.Server.utils;
using log4net;
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
        private readonly string ConnectionString;

        public FriendsManager()
        {
            ConnectionString = ConnectionStringSingleton.GetInstance().ConnectionString;
        }

        public bool AcceptFriendRequest(string senderUsername, string receiverUsername)
        {
            bool success = false;
            using (var db = new Forbbiden_FEIEntities(ConnectionString))
            {
                var profileClient = new ProfileManager();
                try
                {
                    var sender = profileClient.GetPlayerByUsername(senderUsername);
                    var receiver = profileClient.GetPlayerByUsername(receiverUsername);

                    if (sender.PlayerId != -1 && receiver.PlayerId != -1)
                    {

                        var friendRequest = db.Friends.FirstOrDefault(
                            fr => fr.player_id == sender.PlayerId && fr.friend_id == receiver.PlayerId);

                        if (friendRequest != null)
                        {
                            friendRequest.status = 1;
                            db.SaveChanges();

                            FriendRequest friendRequestCallback = new FriendRequest
                            {
                                SenderID = sender.PlayerId,
                                ReceiverID = receiver.PlayerId,
                                Status = 1
                            };

                            if (FriendsNotificationManager.SendRefreshPageCallback(friendRequestCallback, senderUsername))
                            {
                                success = true;
                            }

                            success = true;
                        }
                    }
                }
                catch (EntityException ex)
                {
                    string classMethod = "FriendsManager.AcceptFriendRequest";
                    ExceptionHandler.HandleEntityException(ex, classMethod);
                }
            }

            return success;
        }

        private static (Contracts.Player sender, Contracts.Player receiver) GetSenderReceiver(
            string senderUsername, string receiverUsername)
        {
            var profileManager = new ProfileManager();

            Contracts.Player sender = new Contracts.Player();
            Contracts.Player receiver = new Contracts.Player();

            try
            {
                sender = profileManager.GetPlayerByUsername(senderUsername);
                receiver = profileManager.GetPlayerByUsername(receiverUsername);
            }
            catch (EntityException ex)
            {
                string classMethod = "FriendsManager.SendFriendRequest";
                ExceptionHandler.HandleEntityException(ex, classMethod);
            }

            return (sender, receiver);
        }

        public bool SendFriendRequest(string senderUsername, string receiverUsername)
        {
            bool success = false;

            var (sender, receiver) = GetSenderReceiver(senderUsername, receiverUsername);

            if (sender.PlayerId != -1 && receiver.PlayerId != -1)
            { 
                using (var db = new Forbbiden_FEIEntities(ConnectionString))
                {
                    Friends searchFriendRequest = new Friends();
                    try
                    {
                        searchFriendRequest = db.Friends.FirstOrDefault(
                        sfr => sfr.player_id == sender.PlayerId && sfr.friend_id == receiver.PlayerId);
                    }
                    catch (EntityException ex)
                    {
                        string classMethod = "FriendsManager.SendFriendRequest";
                        ExceptionHandler.HandleEntityException(ex, classMethod);
                    }

                    if (searchFriendRequest == null)
                    {
                        Friends friendRequest = new Friends
                        {
                            player_id = sender.PlayerId,
                            friend_id = receiver.PlayerId,
                            status = 0
                        };

                        try
                        {
                            db.Friends.Add(friendRequest);
                            db.SaveChanges();
                        }
                        catch (EntityException ex)
                        {
                            string classMethod = "FriendsManager.SendFriendRequest";
                            ExceptionHandler.HandleEntityException(ex, classMethod);
                        }

                        FriendRequest friendRequestCallback = new FriendRequest
                        {
                            SenderID = sender.PlayerId,
                            ReceiverID = receiver.PlayerId,
                            Status = 0
                        };

                        success = FriendsNotificationManager.SendRequestCallback(friendRequestCallback, receiverUsername);
                    }
                }
            }

            return success;
        }

        public bool CancelFriendRequest(string senderUsername, string receiverUsername)
        {
            bool success = false;
            
            using (var db = new Forbbiden_FEIEntities(ConnectionString))
            {
                var profileClient = new ProfileManager();
                try
                {
                    var sender = profileClient.GetPlayerByUsername(senderUsername);
                    var receiver = profileClient.GetPlayerByUsername(receiverUsername);
                    if (sender.PlayerId != -1 && receiver.PlayerId != -1)
                    {
                        var friendRequest = db.Friends.FirstOrDefault(
                            fr => fr.player_id == sender.PlayerId && fr.friend_id == receiver.PlayerId);
                        if (friendRequest != null)
                        {
                            db.Friends.Remove(friendRequest);
                            db.SaveChanges();
                            success = true;
                        }
                    }
                }
                catch (EntityException ex)
                {
                    string classMethod = "FriendsManager.CancelFriendRequest";
                    ExceptionHandler.HandleEntityException(ex, classMethod);
                }
            }

            return success;
        }

        public bool DeleteFriend(string friendUsername, string playerUsername)
        {
            bool success = false;

            using (var db = new Forbbiden_FEIEntities(ConnectionString))
            {
                var profileClient = new ProfileManager();
                try
                {
                    var friend = profileClient.GetPlayerByUsername(friendUsername);
                    var player = profileClient.GetPlayerByUsername(playerUsername);
                    if (friend.PlayerId != -1 && player.PlayerId != -1)
                    {
                        var friendship = db.Friends.FirstOrDefault(fr =>
                            (
                                (fr.player_id == player.PlayerId && fr.friend_id == friend.PlayerId) ||
                                (fr.player_id == friend.PlayerId && fr.friend_id == player.PlayerId)
                            )
                            && fr.status == 1
                        );

                        if (friendship != null)
                        {
                            db.Friends.Remove(friendship);
                            db.SaveChanges();

                            FriendsNotificationManager.SendRefreshPageCallback(new FriendRequest(), friendUsername);

                            success = true;
                        }
                    }
                }
                catch (EntityException ex)
                {
                    string classMethod = "FriendsManager.DeleteFriend";
                    ExceptionHandler.HandleEntityException(ex, classMethod);
                }
            }

            return success;
        }

        public List<FriendRequest> GetFriendRequests(string receiverUsername)
        {
            using (var db = new Forbbiden_FEIEntities(ConnectionString))
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
                    string classMethod = "FriendsManager.GetFriendRequests";
                    ExceptionHandler.HandleEntityException(ex, classMethod);
                }
            }
            return new List<FriendRequest>();
        }

        public List<Friendship> GetFriendsByID(int playerID)
        {
            using (var db = new Forbbiden_FEIEntities(ConnectionString))
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
                    string classMethod = "FriendsManager.GetFriedsByID";
                    ExceptionHandler.HandleEntityException(ex, classMethod);
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
