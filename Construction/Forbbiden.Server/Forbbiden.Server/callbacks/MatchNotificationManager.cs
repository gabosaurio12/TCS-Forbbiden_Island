using Forbbiden.Contracts;
using log4net;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Forbbiden.Server.callbacks
{
    internal class MatchNotificationManager : IMatchNotificationManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MatchNotificationManager));
        private static readonly Dictionary<string, IMatchCallback> Subscribers =
            new Dictionary<string, IMatchCallback>();

        public void Subscribe(string username)
        {
            IMatchCallback callback = OperationContext.Current.GetCallbackChannel<IMatchCallback>();
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

        public static bool SendOnBoardCreatedCallback(string boardJson, List<string> usernames)
        {
            bool success = true;
            foreach(var username in usernames)
            {
                if (!Subscribers.TryGetValue(username, out var subscriber))
                {
                    Log.Warn("MatchNotificationManager.SendOnBoardCreatedCallback - User unsubscribed");
                    success = false;
                }
                else
                {
                    try
                    {
                        subscriber.OnBoardCreatedCallback(boardJson);
                    }
                    catch (CommunicationObjectAbortedException ex)
                    {
                        success = false;
                        Subscribers.Remove(username);
                        Log.Warn("MatchNotificationManager.SendOnBoardCreatedCallback", ex);
                        throw;
                    }
                    catch (CommunicationException ex)
                    {
                        success = false;
                        Subscribers.Remove(username);
                        Log.Warn("MatchNotificationManager.SendOnBoardCreatedCallback", ex);
                        throw;
                    }
                    catch (TimeoutException ex)
                    {
                        success = false;
                        Log.Warn("MatchNotificationManager.SendOnBoardCreatedCallback", ex);
                        throw;
                    }
                }
            }

            return success;
        }

        public static bool SendOnBoardUpdatedCallback(string boardJson, List<string> usernames)
        {
            bool success = true;
            foreach (var username in usernames)
            {
                if (!Subscribers.TryGetValue(username, out var subscriber))
                {
                    Log.Warn("MatchNotificationManager.SendOnBoardUpdatedCallback - User unsubscribed");
                    success = false;
                }
                else
                {
                    try
                    {
                        subscriber.OnBoardUpdatedCallback(boardJson);
                    }
                    catch (CommunicationObjectAbortedException ex)
                    {
                        success = false;
                        Subscribers.Remove(username);
                        Log.Warn("MatchNotificationManager.SendOnBoardUpdatedCallback", ex);
                    }
                    catch (CommunicationException ex)
                    {
                        success = false;
                        Subscribers.Remove(username);
                        Log.Warn("MatchNotificationManager.SendOnBoardUpdatedCallback", ex);
                    }
                    catch (TimeoutException ex)
                    {
                        success = false;
                        Log.Warn("MatchNotificationManager.SendOnBoardUpdatedCallback", ex);
                    }
                }
            }

            return success;
        }

        public static bool SendOnPlayersTurnCallback(string username)
        {
            bool success = true;
            if (!Subscribers.TryGetValue(username, out var subscriber))
            {
                Log.Warn("MatchNotificationManager.SendOnPlayersTurnCallback - User unsubscribed");
                success = false;
            }
            else
            {
                try
                {
                    subscriber.OnPlayersTurnCallback();
                }
                catch (CommunicationObjectAbortedException ex)
                {
                    success = false;
                    Subscribers.Remove(username);
                    Log.Warn("MatchNotificationManager.SendOnPlayersTurnCallback", ex);
                }
                catch (CommunicationException ex)
                {
                    success = false;
                    Subscribers.Remove(username);
                    Log.Warn("MatchNotificationManager.SendOnPlayersTurnCallback", ex);
                }
                catch (TimeoutException ex)
                {
                    success = false;
                    Log.Warn("MatchNotificationManager.SendOnPlayersTurnCallback", ex);
                }
            }

            return success;
        }

        public static bool SendOnTurnFinishedCallback(string boardJson, List<string> usernames)
        {
            bool success = true;
            foreach (var username in usernames)
            {
                if (!Subscribers.TryGetValue(username, out var subscriber))
                {
                    Log.Warn("MatchNotificationManager.SendOnTurnFinishedCallback - User unsubscribed");
                    success = false;
                }
                else
                {
                    try
                    {
                        subscriber.OnTurnFinishedCallback(boardJson);
                    }
                    catch (CommunicationObjectAbortedException ex)
                    {
                        success = false;
                        Subscribers.Remove(username);
                        Log.Warn("MatchNotificationManager.SendOnTurnFinishedCallback", ex);
                    }
                    catch (CommunicationException ex)
                    {
                        success = false;
                        Subscribers.Remove(username);
                        Log.Warn("MatchNotificationManager.SendOnTurnFinishedCallback", ex);
                    }
                    catch (TimeoutException ex)
                    {
                        success = false;
                        Log.Warn("MatchNotificationManager.SendOnTurnFinishedCallback", ex);
                    }
                }
            }

            return success;
        }
    }
}
