using Forbbiden.Client.ErrorCodes;
using Forbbiden.Client.Exceptions;
using Forbbiden.Client.ProfileManager;
using log4net;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Forbbiden.Client.Repositories
{
    public class ProfileRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProfileRepository));
        private readonly ProfileManagerClient ProfileClient = new ProfileManagerClient();

        public async Task<bool> IsEmailAvailable(string email)
        {
            bool result;
            try
            {
                result = await ProfileClient.IsEmailAvailableAsync(email);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.IsEmailAvailable", ex);
                throw new ViewException(ServerErrorCodes.pullingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.IsEmailAvailable", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return result;
        }

        public async Task<bool> IsUsernameAvailable(string username)
        {
            bool result;
            try
            {
                result = await ProfileClient.IsUsernameAvailableAsync(username);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
                throw new ViewException(ServerErrorCodes.pullingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return result;
        }

        public async Task<bool> SendSignupEmail(string email, string token)
        {
            bool result;
            try
            {
                result = await ProfileClient.SendSignupEmailAsync(email, token);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.SendSignupEmail", ex);
                throw new ViewException(ServerErrorCodes.sendEmailError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.IsEmailAvailable", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return result;
        }

        public async Task<bool> SendVerificationEmail(string email, string token)
        {
            bool result;
            try
            {
                result = await ProfileClient.SendVerificationEmailAsync(email, token);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.SendVerificationEmail", ex);
                throw new ViewException(ServerErrorCodes.sendEmailError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.SendVerificationEmail", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return result;
        }

        public async Task<int> SignupPlayer(Player player)
        {
            int playerId;
            try
            {
                playerId = await ProfileClient.SignUpAsync(player);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.SignupPlayer", ex);
                throw new ViewException(ServerErrorCodes.pushingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.SignupPlayer", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return playerId;
        }

        public async Task<Player> LoginPlayer(string username, string password)
        {
            Player player;
            try
            {
                player = await ProfileClient.LoginAsync(username, password);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.LoginPlayer", ex);
                throw new ViewException(ServerErrorCodes.pullingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.LoginPlayer", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return player;
        }

        public async Task<Player> GetPlayerByUsername(string username, bool includeFriends)
        {
            Player player;
            try
            {
                player = await ProfileClient.GetPlayerByUsernameAsync(username, includeFriends);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.GetPlayerByUsername", ex);
                throw new ViewException(ServerErrorCodes.pullingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.GetPlayerByUsername", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return player;
        }

        public async Task<Player> GetPlayerById(int playerId, bool includeFriends)
        {
            Player player;
            try
            {
                player = await ProfileClient.GetPlayerByIdAsync(playerId, includeFriends);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.GetPlayerById", ex);
                throw new ViewException(ServerErrorCodes.pullingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.GetPlayerById", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return player;
        }

        public async Task<bool> UpdatePlayerProfile(Player player)
        {
            bool result;
            try
            {
                result = await ProfileClient.UpdatePlayerAsync(player);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.UpdatePlayerProfile", ex);
                throw new ViewException(ServerErrorCodes.updatingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.UpdatePlayerProfile", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return result;
        }

        public async Task<bool> DeletePlayer(string username)
        {
            bool result;
            try
            {
                result = await ProfileClient.DeletePlayerByUsernameAsync(username);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.DeletePlayer", ex);
                throw new ViewException(ServerErrorCodes.updatingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.DeletePlayer", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return result;
        }

        public async Task<bool> ConnectPlayer(string username)
        {
            bool result;
            try
            {
                result = await ProfileClient.ConnectPlayerByUsernameAsync(username);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.ConnectPlayer", ex);
                throw new ViewException(ServerErrorCodes.updatingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.ConnectPlayer", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return result;
        }

        public async Task<bool> DisconnectPlayer(string username)
        {
            bool result;
            try
            {
                result = await ProfileClient.DisconnectPlayerByUsernameAsync(username);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.DisconnectPlayer", ex);
                throw new ViewException(ServerErrorCodes.updatingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.DisconnectPlayer", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return result;
        }

        public async Task<bool> UploadAvatar(string username, byte[] avatarBytes, string fileName)
        {
            bool result;
            try
            {
                result = await ProfileClient.UploadAvatarAsync(username, avatarBytes, fileName);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.UploadAvatar", ex);
                throw new ViewException(ServerErrorCodes.updatingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.UploadAvatar", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return result;
        }

        public async Task<AvatarResponse> GetAvatarByUsername(string username)
        {
            AvatarResponse avatar;
            try
            {
                avatar = await ProfileClient.GetAvatarByUsernameAsync(username);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.GetAvatarByUsername", ex);
                throw new ViewException(ServerErrorCodes.pullingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.GetAvatarByUsername", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return avatar;
        }
    }
}