using Forbbiden.Client.ErrorCodes;
using Forbbiden.Client.Exceptions;
using Forbbiden.Client.FriendsManager;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Forbbiden.Client.Repositories
{

    public class FriendsRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FriendsRepository));
        private static readonly FriendsManagerClient FriendsClient = new FriendsManagerClient();

        public static async Task<bool> SendFriendRequest(string senderUsername, string receiverUsername)
        {
            bool result = false;
            try
            {
                result = await FriendsClient.SendFriendRequestAsync(senderUsername, receiverUsername);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("FriendsRepository.SendFriendRequest", ex);
                throw new ViewException(ServerErrorCodes.pushingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("FriendsRepository.SendFriendRequest", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            catch (CommunicationException ex)
            {
                Log.Error("FriendsRepository.SendFriendRequest", ex);
            }

            return result;
        }

        public static async Task<bool> AcceptFriendRequest(string senderUsername, string receiverUsername)
        {
            bool result;
            try
            {
                result = await FriendsClient.AcceptFriendRequestAsync(senderUsername, receiverUsername);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("FriendsRepository.AcceptFriendRequest", ex);
                throw new ViewException(ServerErrorCodes.pullingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("FriendsRepository.AcceptFriendRequest", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return result;
        }

        public static async Task<bool> CancelFriendRequest(string senderUsername, string receiverUsername)
        {
            bool result;
            try
            {
                result = await FriendsClient.CancelFriendRequestAsync(senderUsername, receiverUsername);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("FriendsRepository.CancelFriendRequest", ex);
                throw new ViewException(ServerErrorCodes.pullingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("FriendsRepository.CancelFriendRequest", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return result;
        }

        public static async Task<bool> DeleteFriend(string friendUsername, string playerUsername)
        {
            bool result;
            try
            {
                result = await FriendsClient.DeleteFriendAsync(friendUsername, playerUsername);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("FriendsRepository.DeleteFriend", ex);
                throw new ViewException(ServerErrorCodes.pullingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("FriendsRepository.DeleteFriend", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return result;
        }

        public static async Task<List<FriendRequest>> GetFriendRequests(string receiverUsername)
        {
            FriendRequest[] result;
            try
            {
                result = await FriendsClient.GetFriendRequestsAsync(receiverUsername);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("FriendsRepository.GetFriendRequests", ex);
                throw new ViewException(ServerErrorCodes.pullingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("FriendsRepository.GetFriendRequests", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }
            return result.ToList();
        }

        public static async Task<List<FriendsManager.Friendship>> GetFriendsByID(int playerID)
        {
            FriendsManager.Friendship[] result;
            try
            {
                result = await FriendsClient.GetFriendsByIDAsync(playerID);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("FriendsRepository.GetFriendRequests", ex);
                throw new ViewException(ServerErrorCodes.pullingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("FriendsRepository.GetFriendRequests", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }
            return result.ToList();
        }
    }
}