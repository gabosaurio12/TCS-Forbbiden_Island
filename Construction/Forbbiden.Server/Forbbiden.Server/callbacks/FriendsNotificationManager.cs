using Forbbiden.Contracts;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.ServiceModel;

namespace Forbbiden.Server.callbacks
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "FriendsNotificationManager" in both code and config file together.
    public class FriendsNotificationManager : IFriendsNotificationManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FriendsNotificationManager));
        private static readonly Dictionary<string, IFriendRequestCallback> Subscribers =
            new Dictionary<string, IFriendRequestCallback>();

        public void Subscribe(string username)
        {
            IFriendRequestCallback callback = OperationContext.Current.GetCallbackChannel<IFriendRequestCallback>();
            if (!Subscribers.ContainsKey(username))
            {
                Subscribers.Add(username, callback);
            }
        }

        public void Unsubscribe(string username)
        {
            if (Subscribers.ContainsKey(username))
            {
                Subscribers.Remove(username);
            }
        }

        public static bool SendRequestCallback(FriendRequest friendRequest, string receiverUsername)
        {
            bool success = false;

            if (!Subscribers.TryGetValue(receiverUsername, out var subscriber))
            {
                Log.Warn("[WARNING] - FriendsManager.SendFriendRequest - User unsuscribed");
            }
            else
            {
                try
                {
                    subscriber.NewFriendRequest(friendRequest);
                    success = true;
                }
                catch (CommunicationObjectAbortedException ex)
                {
                    Subscribers.Remove(receiverUsername);

                    Log.Warn("WARNING - FriendsNotificationManager.SendFriendRequest", ex);
                }
                catch (CommunicationException ex)
                {
                    Subscribers.Remove(receiverUsername);

                    Log.Warn("[WARNING] - FriendsManager.SendFriendRequest -", ex);
                }
                catch (TimeoutException ex)
                {
                    Log.Warn("[WARNING] - FriendsManager.SendFriendRequest -", ex);
                }
            }

            return success;
        }

        public static bool SendAcceptedRequestCallback(FriendRequest friendRequest, string senderUsername)
        {
            bool success = false;

            if (!Subscribers.TryGetValue(senderUsername, out var subscriber))
            {
                Log.Warn("[WARNING] - FriendsManager.SendFriendRequest - User unsuscribed");
            }
            else
            {
                try
                {
                    subscriber.NewFriendship(friendRequest);
                    success = true;
                }
                catch (CommunicationObjectAbortedException ex)
                {
                    Subscribers.Remove(senderUsername);

                    Log.Warn("WARNING - FriendsNotificationManager.SendFriendRequest", ex);
                }
                catch (CommunicationException ex)
                {
                    Subscribers.Remove(senderUsername);

                    Log.Warn("[WARNING] - FriendsManager.SendFriendRequest -", ex);
                }
                catch (TimeoutException ex)
                {
                    Log.Warn("[WARNING] - FriendsManager.SendFriendRequest -", ex);
                }
            }

            return success;
        }
    }
}
