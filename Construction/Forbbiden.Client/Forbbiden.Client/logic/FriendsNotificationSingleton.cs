using Forbbiden.Client.FriendsNotificationManager;
using log4net;
using System;
using System.ServiceModel;
using System.Windows;

namespace Forbbiden.Client.logic
{
    public class FriendsNotificationSingleton : IFriendsNotificationManagerCallback
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FriendsNotificationSingleton));

        private static FriendsNotificationSingleton PrivateInstance;

        public static FriendsNotificationSingleton Instance
        {
            get
            {
                if (PrivateInstance == null)
                {
                    PrivateInstance = new FriendsNotificationSingleton();
                }
                return PrivateInstance;
            }
        }

        public event Action<FriendRequest> OnNewFriendRequest;

        public event Action<FriendRequest> OnRefreshPage;

        private readonly FriendsNotificationManagerClient Client;

        private FriendsNotificationSingleton()
        {
            var context = new InstanceContext(this);
            Client = new FriendsNotificationManagerClient(context);
        }

        public void Subscribe(string username)
        {
            Client.Subscribe(username);
        }

        public void Unsubscribe(string username)
        {
            Client.Unsubscribe(username);
        }

        public void NewFriendRequest(FriendRequest friendRequest)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnNewFriendRequest?.Invoke(friendRequest);
                });
            }
            catch (Exception ex)
            {
                string classMethod = "FriendsNotificationSingleton.NewFriendRequest";
                Log.Error(classMethod, ex);
            }
        }

        public void RefreshPageCallback(FriendRequest friendRequest)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnRefreshPage?.Invoke(friendRequest);
                });
            }
            catch (Exception ex)
            {
                string classMethod = "FriendsNotificationSingleton.RefreshPageCallback";
                Log.Error(classMethod, ex);
            }
        }
    }
}
