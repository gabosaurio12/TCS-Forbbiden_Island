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
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
                throw new ViewException(ServerErrorCodes.sendEmailError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
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
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
                throw new ViewException(ServerErrorCodes.sendEmailError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
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
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
                throw new ViewException(ServerErrorCodes.pushingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
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
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
                throw new ViewException(ServerErrorCodes.pullingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
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
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
                throw new ViewException(ServerErrorCodes.pullingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
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
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
                throw new ViewException(ServerErrorCodes.pullingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
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
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
                throw new ViewException(ServerErrorCodes.updatingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
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
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
                throw new ViewException(ServerErrorCodes.updatingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
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
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
                throw new ViewException(ServerErrorCodes.updatingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
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
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
                throw new ViewException(ServerErrorCodes.updatingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return result;
        }

        public async Task<string> UploadAvatar(string username, byte[] avatarBytes, string fileName)
        {
            string avatarFileName;
            try
            {
                avatarFileName = await ProfileClient.UploadAvatarAsync(
                    username, avatarBytes, fileName);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
                throw new ViewException(ServerErrorCodes.updatingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return avatarFileName;
        }

        public async Task<byte[]> DownloadAvatar(string avatarFileName)
        {
            byte[] avatarBytes;
            try
            {
                avatarBytes = await ProfileClient.GetAvatarAsync(avatarFileName);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
                throw new ViewException(ServerErrorCodes.avatarDownloadError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }

            return avatarBytes;
        }
    }
}
