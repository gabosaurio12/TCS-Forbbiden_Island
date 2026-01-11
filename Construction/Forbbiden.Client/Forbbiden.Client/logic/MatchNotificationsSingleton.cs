using Forbbiden.Client.MatchNotificationManager;
using log4net;
using System;
using System.ServiceModel;
using System.Windows;

namespace Forbbiden.Client.Logic
{
    public class MatchNotificationsSingleton : IMatchNotificationManagerCallback
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MatchNotificationsSingleton));

        private readonly MatchNotificationManagerClient Client;

        private static MatchNotificationsSingleton PrivateInstance;

        public static MatchNotificationsSingleton Instance
        {
            get
            {
                if (PrivateInstance == null)
                {
                    PrivateInstance = new MatchNotificationsSingleton();
                }
                return PrivateInstance;
            }
        }

        public event Action<string> OnBoardCreated;
        public event Action<string> OnBoardUpdated;
        public event Action OnPlayersTurn;
        public event Action<string> OnTurnFinished;

        private MatchNotificationsSingleton()
        {
            var context = new InstanceContext(this);
            Client = new MatchNotificationManagerClient(context);
        }

        public void Subscribe(string username)
        {
            Client.Subscribe(username);
        }

        public void Unsubscribe(string username)
        {
            Client.Unsubscribe(username);
        }

        public void OnBoardCreatedCallback(string boardJson)
        {
            
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnBoardCreated?.Invoke(boardJson);
                });
            }
            catch (Exception ex)
            {
                string classMethod = "MatchNotificationsSingleton.OnBoardCreatedCallback";
                Log.Error(classMethod, ex);
            }
        }

        public void OnBoardUpdatedCallback(string boardJson)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnBoardUpdated?.Invoke(boardJson);
                });
            }
            catch (Exception ex)
            {
                string classMethod = "MatchNotificationsSingleton.OnBoardUpdatedCallback";
                Log.Error(classMethod, ex);
            }
        }

        public void OnPlayersTurnCallback()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnPlayersTurn?.Invoke();
                });
            }
            catch (Exception ex)
            {
                string classMethod = "MatchNotificationsSingleton.OnBoardUpdatedCallback";
                Log.Error(classMethod, ex);
            }
        }

        public void OnTurnFinishedCallback(string boardJson)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnTurnFinished?.Invoke(boardJson);
                });
            }
            catch (Exception ex)
            {
                string classMethod = "MatchNotificationsSingleton.OnTurnFinishedCallback";
                Log.Error(classMethod, ex);
            }
        }
    }
}
