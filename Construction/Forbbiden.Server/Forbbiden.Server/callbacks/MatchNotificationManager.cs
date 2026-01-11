using Forbbiden.Contracts;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;

namespace Forbbiden.Server.callbacks
{
    internal class MatchNotificationManager : IMatchNotificationManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MatchNotificationManager));
        private static readonly ConcurrentDictionary<string, IMatchCallback> Subscribers =
            new ConcurrentDictionary<string, IMatchCallback>();

        public void Subscribe(string username)
        {
            IMatchCallback callback = OperationContext.Current.GetCallbackChannel<IMatchCallback>();
            if (!Subscribers.ContainsKey(username))
            {
                Subscribers.TryAdd(username, callback);
            }
        }

        public void Unsubscribe(string username)
        {
            if (Subscribers.ContainsKey(username))
            {
                Subscribers.TryRemove(username, out var _);
            }
        }

        public static void SendOnBoardCreatedCallback(string boardJson, List<string> usernames)
        {
            foreach (var username in usernames)
            {
                if (!Subscribers.TryGetValue(username, out var subscriber))
                {
                    Log.Warn("MatchNotificationManager.SendOnBoardCreatedCallback - User unsubscribed");
                    continue;
                }

                if (!(subscriber is ICommunicationObject channel))
                {
                    Subscribers.TryRemove(username, out var _);
                    continue;
                }

                if (channel.State != CommunicationState.Opened)
                {
                    Subscribers.TryRemove(username, out var _);
                    continue;
                }

                try
                {
                    subscriber.OnBoardCreatedCallback(boardJson);
                }
                catch (ObjectDisposedException ex)
                {
                    Subscribers.TryRemove(username, out var _);
                    Log.Warn("MatchNotificationmanager.SendOnBoardCreatedCallback", ex);
                }
                catch (CommunicationObjectAbortedException ex)
                {
                    Subscribers.TryRemove(username, out var _);
                    Log.Warn("MatchNotificationManager.SendOnBoardCreatedCallback", ex);
                }
                catch (CommunicationException ex)
                {
                    Subscribers.TryRemove(username, out var _);
                    Log.Warn("MatchNotificationManager.SendOnBoardCreatedCallback", ex);
                }
                catch (TimeoutException ex)
                {
                    Log.Warn("MatchNotificationManager.SendOnBoardCreatedCallback", ex);
                }
            }
        }

        public static void SendOnBoardUpdatedCallback(string boardJson, List<string> usernames)
        {
            foreach (var username in usernames)
            {
                if (!Subscribers.TryGetValue(username, out var subscriber))
                {
                    Log.Warn("MatchNotificationManager.SendOnBoardUpdatedCallback - User unsubscribed");
                }

                if (!(subscriber is ICommunicationObject channel))
                {
                    Subscribers.TryRemove(username, out var _);
                    continue;
                }

                if (channel.State != CommunicationState.Opened)
                {
                    Subscribers.TryRemove(username, out var _);
                    continue;
                }

                try
                {
                    subscriber.OnBoardUpdatedCallback(boardJson);
                }
                catch (ObjectDisposedException ex)
                {
                    Subscribers.TryRemove(username, out var _);
                    Log.Warn("MatchNotificationmanager.SendOnBoardUpdatedCallback", ex);
                }
                catch (CommunicationObjectAbortedException ex)
                {
                    Subscribers.TryRemove(username, out var _);
                    Log.Warn("MatchNotificationManager.SendOnBoardUpdatedCallback", ex);
                }
                catch (CommunicationException ex)
                {
                    Subscribers.TryRemove(username, out var _);
                    Log.Warn("MatchNotificationManager.SendOnTurnFinishedCallback", ex);
                }
                catch (TimeoutException ex)
                {
                    Log.Warn("MatchNotificationManager.SendOnTurnFinishedCallback", ex);
                }
            }
        }

        public static void SendOnPlayersTurnCallback(string username)
        {
            if (!Subscribers.TryGetValue(username, out var subscriber))
            {
                Log.Warn("MatchNotificationManager.SendOnPlayersTurnCallback - User unsubscribed");
                return;
            }

            if (!(subscriber is ICommunicationObject channel))
            {
                Subscribers.TryRemove(username, out var _);
                return;
            }

            if (channel.State != CommunicationState.Opened)
            {
                Subscribers.TryRemove(username, out var _);
                return;
            }

            try
            {
                subscriber.OnPlayersTurnCallback();
            }
            catch (ObjectDisposedException ex)
            {
                Subscribers.TryRemove(username, out var _);
                Log.Warn("MatchNotificationmanager.SendOnPlayersTurnCallback", ex);
            }
            catch (CommunicationObjectAbortedException ex)
            {
                Subscribers.TryRemove(username, out var _);
                Log.Warn("MatchNotificationManager.SendOnPlayersTurnCallback", ex);
            }
            catch (CommunicationException ex)
            {
                Subscribers.TryRemove(username, out var _);
                Log.Warn("MatchNotificationManager.SendOnTurnFinishedCallback", ex);
            }
            catch (TimeoutException ex)
            {
                Log.Warn("MatchNotificationManager.SendOnTurnFinishedCallback", ex);
            }
        }

        public static void SendOnTurnFinishedCallback(string boardJson, List<string> usernames)
        {
            foreach (var username in usernames)
            {
                if (!Subscribers.TryGetValue(username, out var subscriber))
                {
                    Log.Warn("MatchNotificationManager.SendOnTurnFinishedCallback - User unsubscribed");
                }

                if (!(subscriber is ICommunicationObject channel))
                {
                    Subscribers.TryRemove(username, out var _);
                    continue;
                }

                if (channel.State != CommunicationState.Opened)
                {
                    Subscribers.TryRemove(username, out var _);
                    continue;
                }

                try
                {
                    subscriber.OnTurnFinishedCallback(boardJson);
                }
                catch (ObjectDisposedException ex)
                {
                    Subscribers.TryRemove(username, out var _);
                    Log.Warn("MatchNotificationmanager.SendOnTurnFinishedCallback", ex);
                }
                catch (CommunicationObjectAbortedException ex)
                {
                    Subscribers.TryRemove(username, out var _);
                    Log.Warn("MatchNotificationManager.SendOnTurnFinishedCallback", ex);
                }
                catch (CommunicationException ex)
                {
                    Subscribers.TryRemove(username, out var _);
                    Log.Warn("MatchNotificationManager.SendOnTurnFinishedCallback", ex);
                }
                catch (TimeoutException ex)
                {
                    Log.Warn("MatchNotificationManager.SendOnTurnFinishedCallback", ex);
                }
            }
        }
    }
}
