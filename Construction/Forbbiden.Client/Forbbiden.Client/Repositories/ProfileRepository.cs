using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.TokenManager;
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
            bool result = false;
            try
            {
                result = await ProfileClient.IsEmailAvailableAsync(email);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
            }

            return result;
        }

        public async Task<bool> IsUsernameAvailable(string username)
        {
            bool result = false;
            try
            {
                result = await ProfileClient.IsUsernameAvailableAsync(username);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.IsUsernameAvailable", ex);
            }

            return result;
        }

        public async Task<bool> SendSignupEmail(string email, string token)
        {
            bool result = false;
            try
            {
                result = await ProfileClient.SendSignupEmailAsync(email, token);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.SendEmail", ex);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.SendEmail", ex);
            }
            return result;  
        }

        public async Task<bool> SendVerificationEmail(string email, string token)
        {
            bool result = false;
            try
            {
                result = await ProfileClient.SendVerificationEmailAsync(email, token);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.SendEmail", ex);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.SendEmail", ex);
            }
            return result;
        }

        public async Task<int> SignupPlayer(Player player)
        {
            int playerId = -1;
            try
            {
                playerId = await ProfileClient.SignUpAsync(player);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.SignupPlayer", ex);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.SignupPlayer", ex);
            }
            return playerId;
        }

        public async Task<Player> LoginPlayer(string username, string password)
        {
            Player player = null;
            try
            {
                player = await ProfileClient.LoginAsync(username, password);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.LoginPlayer", ex);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.LoginPlayer", ex);
            }
            return player;
        }

        public async Task<Player> GetPlayerByUsername(string username, bool includeFriends)
        {
            Player player = null;
            try
            {
                player = await ProfileClient.GetPlayerByUsernameAsync(username, includeFriends);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.GetPlayerByUsername", ex);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.GetPlayerByUsername", ex);
            }
            return player;
        }

        public async Task<Player> GetPlayerById(int playerId, bool includeFriends)
        {
            Player player = null;
            try
            {
                player = await ProfileClient.GetPlayerByIdAsync(playerId, includeFriends);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.GetPlayerById", ex);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.GetPlayerById", ex);
            }
            return player;
        }

        public async Task<bool> UpdatePlayerProfile(Player player)
        {
            bool result = false;
            try
            {
                result = await ProfileClient.UpdatePlayerAsync(player);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.UpdatePlayerProfile", ex);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.UpdatePlayerProfile", ex);
            }
            return result;
        }

        public async Task<bool> DeletePlayer(string username)
        {
            bool result = false;
            try
            {
                result = await ProfileClient.DeletePlayerByUsernameAsync(username);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.DeletePlayer", ex);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.DeletePlayer", ex);
            }
            return result;
        }

        public async Task<bool> ConnectPlayer(string username)
        {
            bool result = false;
            try
            {
                result = await ProfileClient.ConnectPlayerByUsernameAsync(username);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.ConnectPlayer", ex);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.ConnectPlayer", ex);
            }
            return result;
        }

        public async Task<bool> DisconnectPlayer(string username)
        {
            bool result = false;
            try
            {
                result = await ProfileClient.DisconnectPlayerByUsernameAsync(username);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.DisconnectPlayer", ex);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.DisconnectPlayer", ex);
            }
            return result;
        }

        public async Task<string> UploadAvatar(string username, byte[] avatarBytes, string fileName)
        {
            string avatarFileName = null;
            try
            {
                avatarFileName = await ProfileClient.UploadAvatarAsync(username, avatarBytes, fileName);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.UploadAvatar", ex);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.UploadAvatar", ex);
            }
            return avatarFileName;
        }

        public async Task<byte[]> DownloadAvatar(string avatarFileName)
        {
            byte[] avatarBytes = null;
            try
            {
                avatarBytes = await ProfileClient.GetAvatarAsync(avatarFileName);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("ProfileRepository.DownloadAvatar", ex);
            }
            catch (TimeoutException ex)
            {
                Log.Error("ProfileRepository.DownloadAvatar", ex);
            }
            return avatarBytes;
        }
    }
}
