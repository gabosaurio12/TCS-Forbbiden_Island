using System;

namespace Forbbiden.Client.logic
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "CallbacksManager" in both code and config file together.
    public class CallbacksManager : ICallbacks
    {
        public event Action<FriendsManager.FriendRequest> FriendRequestReceived;

        public void OnFriendRequestReceived(FriendsManager.FriendRequest request)
        {
            FriendRequestReceived?.Invoke(request);
        }
    }
}
