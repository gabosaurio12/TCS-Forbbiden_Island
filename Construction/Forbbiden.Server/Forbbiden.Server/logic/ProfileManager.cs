using Forbbiden.Contracts;
using Forbbiden.Server.exceptionHandlers;
using Forbbiden.Server.Model;
using Forbbiden.Server.utils;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;

namespace Forbbiden.Server.logic
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ProfileManager : IProfileManager
    {
        private readonly string ConnectionString;

        public string DefaultAvatarName = "defaultAvatar.png";
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProfileManager));

        public ProfileManager()
        {
            ConnectionString = ConnectionStringSingleton.GetInstance().ConnectionString;
        }

        public bool IsEmailAvailable(string email)
        {
            string classMethod = "ProfileManager.ValidateEmail ";
            bool isAvailable = false;

            if (string.IsNullOrWhiteSpace(email))
                return isAvailable;

            try
            {
                using (var db = new Forbidden_FEIEntities(ConnectionString))
                {
                    var emailResult = db.Player.FirstOrDefault(p => p.player_email == email);
                    isAvailable = emailResult == null;
                }
            }

            catch (SqlException ex) when (ex.Number == 2 || ex.Number == 53)
            {
                ExceptionHandler.HandleDBException(ex, classMethod, ExceptionHandler.SqlMessage);
            }
            catch (EntityException ex)
            {
                ExceptionHandler.HandleEntityException(ex, classMethod, ExceptionHandler.PullingError);
            }

            return isAvailable;
        }

        public bool IsUsernameAvailable(string username)
        {
            string classMethod = "ProfileManager.IsUsernameAvailable ";
            bool usernameFound = false;

            if (string.IsNullOrWhiteSpace(username))
                return usernameFound;

            using (var db = new Forbidden_FEIEntities(ConnectionString))
            {
                try
                {
                    var playerResult = db.Player.FirstOrDefault(u => u.player_username == username);
                    usernameFound = playerResult == null;
                }

                catch (SqlException ex) when (ex.Number == 2 || ex.Number == 53)
                {
                    ExceptionHandler.HandleDBException(ex, classMethod, ExceptionHandler.SqlMessage);
                }
                catch (EntityException ex)
                {
                    ExceptionHandler.HandleEntityException(ex, classMethod, ExceptionHandler.PullingError);
                }
            }
            return usernameFound;
        }

        public bool SendSignupEmail(string email, string token)
        {
            bool success = false;

            if (string.IsNullOrWhiteSpace(email) || token == null)
                return success;

            string emisor = Properties.email.Default.emailAddress;

            var message = new MailMessage
            {
                From = new MailAddress(emisor)
            };
            try
            {
                message.To.Add(new MailAddress(email));
            }
            catch (FormatException ex)
            {
                Log.Warn("ProfileManager.SendSignupEmail", ex);
                return success;
            }
            message.Subject = "Welcome to Forbbiden Island FEI Edition";

            string htmlBody = File.ReadAllText("SignupEmailMessage.html");
            htmlBody = htmlBody.Replace("{{TOKEN}}", token);

            message.IsBodyHtml = true;
            message.Body = htmlBody;

            using (var client = new SmtpClient(Properties.email.Default.smtp))
            {
                client.Port = 587;
                string emailCode = Properties.email.Default.emailCode;
                client.Credentials = new System.Net.NetworkCredential(emisor, emailCode);
                client.EnableSsl = true;

                try
                {
                    client.Send(message);
                    success = true;
                }
                catch (SmtpException ex)
                {
                    string classMethod = "ProfileManager.SendSignupEmail";
                    ExceptionHandler.HandleSmtpException(ex, classMethod, ExceptionHandler.EmailError);
                }
            }

            return success;
        }

        public bool SendVerificationEmail(string email, string token)
        {
            bool success = false;

            if (string.IsNullOrWhiteSpace(email) || token == null)
            {
                return success;
            }

            string sender = Properties.email.Default.emailAddress;

            var message = new MailMessage
            {
                From = new MailAddress(sender),
                Subject = "Password changed",
                IsBodyHtml = true
            };
            try
            {
                message.To.Add(new MailAddress(email));
            }
            catch (FormatException ex)
            {
                Log.Warn("ProfileManager.SendSignupEmail", ex);
                return success;
            }
            message.Subject = "Password changed";

            string htmlBody = File.ReadAllText("VerificationEmailMessage.html")
                .Replace("{{TOKEN}}", token);

            message.Body = htmlBody;

            using (var client = new SmtpClient(Properties.email.Default.smtp))
            {
                client.Port = 587;
                client.Credentials = new System.Net.NetworkCredential(
                    sender,
                    Properties.email.Default.emailCode
                );
                client.EnableSsl = true;

                try
                {
                    client.Send(message);
                    success = true;
                }
                catch (SmtpException ex)
                {
                    ExceptionHandler.HandleSmtpException(ex, "ProfileManager.SendVerificationEmail",
                        ExceptionHandler.EmailError);
                }
            }

            return success;
        }

        public Contracts.Player Login(string username, string password)
        {
            Log.Info("Logging in player");
            string classMethod = "ProfileManager.Login";

            var player = new Contracts.Player { PlayerId = -1 };

            using (var db = new Forbidden_FEIEntities(ConnectionString))
            {
                try
                {
                    var searchPlayer = db.Player.FirstOrDefault(p => p.player_username == username);

                    if (searchPlayer != null)
                    {
                        if (searchPlayer.player_status == 0)
                        {
                            string normalizedPassword = password.Normalize(NormalizationForm.FormC);
                            bool passwordIsCorrect = BCrypt.Net.BCrypt.Verify(normalizedPassword, searchPlayer.player_password);
                            if (passwordIsCorrect)
                            {
                                player = SetPlayer(searchPlayer, false);
                            }
                            else
                            {
                                player.PlayerId = -2;
                            }
                        }
                    }
                }
                catch (SqlException ex) when (ex.Number == 2 || ex.Number == 53)
                {
                    ExceptionHandler.HandleDBException(ex, classMethod, ExceptionHandler.SqlMessage);
                }
                catch (EntityException ex)
                {
                    ExceptionHandler.HandleEntityException(ex, classMethod, ExceptionHandler.PullingError);
                }
            }

            return player;
        }

        public int SignUp(Contracts.Player player)
        {
            int playerId = -1;
            string classMethod = "ProfileManager.SignUp";

            using (var db = new Forbidden_FEIEntities(ConnectionString))
            { 
                string normalizedPassord = player.PlayerPassword.Normalize(NormalizationForm.FormC);
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(normalizedPassord);

                var newPlayer = new Model.Player
                {
                    player_username = player.PlayerUsername,
                    player_password = hashedPassword,
                    player_email = player.PlayerEmail,
                    player_name = string.Empty,
                    player_avatar_file = null,
                    player_avatar_name = DefaultAvatarName,
                    player_status = 0,
                    is_verified = 0
                };

                try
                {
                    db.Player.Add(newPlayer);
                    db.SaveChanges();
                    playerId = newPlayer.player_id;

                    Log.Info("New player signed up");
                }
                catch (SqlException ex) when (ex.Number == 2 || ex.Number == 53)
                {
                    ExceptionHandler.HandleDBException(ex, classMethod, ExceptionHandler.SqlMessage);
                }
                catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
                {
                    return -2;
                }

                catch (EntityException ex)
                {
                    ExceptionHandler.HandleEntityException(ex, classMethod, ExceptionHandler.PushingError);
                }
            }

            return playerId;
        }

        public AvatarResponse GetAvatarByUsername(string username)
        {
            string classMethod = "ProfileManager.GetAvatarByUsername";
            using (var db = new Forbidden_FEIEntities(ConnectionString))
            {
                try
                {
                    var player = db.Player.FirstOrDefault(p => p.player_username == username);
                    if (player == null)
                    {
                        return new AvatarResponse
                        {
                            AvatarBytes = Array.Empty<byte>(),
                            FileName = null
                        };
                    }

                    return new AvatarResponse
                    {
                        AvatarBytes = player.player_avatar_file ?? Array.Empty<byte>(),
                        FileName = player.player_avatar_name
                    };
                }
                catch (SqlException ex) when (ex.Number == 2 || ex.Number == 53)
                {
                    ExceptionHandler.HandleDBException(ex, classMethod, ExceptionHandler.SqlMessage);
                }
                catch (EntityException ex)
                {
                    ExceptionHandler.HandleEntityException(ex, classMethod, ExceptionHandler.PullingError);
                }
            }

            return null;
        }

        private Contracts.Player SetPlayer(Model.Player player, bool includeFriends = true)
        {
            var friendsList = includeFriends
                ? GetFriendsByID(player.player_id)
                : new List<Friendship>();

            return new Contracts.Player
            {
                PlayerId = player.player_id,
                PlayerUsername = player.player_username,
                PlayerName = player.player_name ?? string.Empty,
                PlayerPassword = player.player_password,
                PlayerEmail = player.player_email,
                PlayerAvatarBytes = player.player_avatar_file,
                PlayerAvatarName = player.player_avatar_name,
                Status = (int)player.player_status,
                IsVerified = (int)player.is_verified,
                SocialMedia = player.PlayerSocialmedia.Select(sm => new SocialMedia
                {
                    PlayerId = player.player_id,
                    SocialMediaId = sm.social_media_id,
                    SocialMediaName = sm.social_media_name.Trim(),
                    SocialLink = sm.social_link
                }).ToList(),
                Friends = friendsList
            };
        }

        public Contracts.Player GetPlayerByUsername(string username, bool includeFriends = true)
        {
            Log.Info("Retrieving player by username");
            string classMethod = "ProfileManager.GetPlayerByUsername";

            Contracts.Player player = null;
            using (var db = new Forbidden_FEIEntities(ConnectionString))
            {
                try
                {
                    var playerResult = db.Player.FirstOrDefault(u => u.player_username == username);
                    if (playerResult != null)
                    {
                        Log.Info("Player found");

                        player = SetPlayer(playerResult, includeFriends);
                    }
                }
                catch (SqlException ex) when (ex.Number == 2 || ex.Number == 53)
                {
                    ExceptionHandler.HandleDBException(ex, classMethod, ExceptionHandler.SqlMessage);
                }
                catch (EntityException ex)
                {
                    ExceptionHandler.HandleEntityException(ex, classMethod, ExceptionHandler.PullingError);
                }

                if (player == null)
                {
                    player = new Contracts.Player
                    {
                        PlayerId = -1
                    };
                }

                return player;
            }

        }

        public Contracts.Player GetPlayerById(int playerId, bool includeFriends = true)
        {
            Log.Info("Retrieving player by ID");
            string classMethod = "ProfileManager.GetPlayerById";
            Contracts.Player player = null;

            if (playerId > 0)
            {
                using (var db = new Forbidden_FEIEntities(ConnectionString))
                {
                    try
                    {
                        var playerResult = db.Player.FirstOrDefault(p => p.player_id == playerId);
                        if (playerResult != null)
                        {
                            player = SetPlayer(playerResult, includeFriends);
                        }
                    }
                    catch (SqlException ex) when (ex.Number == 2 || ex.Number == 53)
                    {
                        ExceptionHandler.HandleDBException(ex, classMethod, ExceptionHandler.SqlMessage);
                    }
                    catch (EntityException ex)
                    {
                        ExceptionHandler.HandleEntityException(ex, classMethod, ExceptionHandler.PullingError);
                    }
                }
            }

            if (player == null)
            {
                player = new Contracts.Player
                {
                    PlayerId = -1
                };
            }

            return player;
        }

        public List<Friendship> GetFriendsByID(int playerID)
        {
            string classMethod = "ProfileManager.GetFriendsByID";
            using (var db = new Forbidden_FEIEntities(ConnectionString))
            {
                var friends = new List<Friends>();
                try
                {
                    friends = db.Friends.Where(f =>
                        (f.player_id == playerID ||
                        f.friend_id == playerID) &&
                        f.status == 1).ToList();
                }
                catch (SqlException ex) when (ex.Number == 2 || ex.Number == 53)
                {
                    ExceptionHandler.HandleDBException(ex, classMethod, ExceptionHandler.SqlMessage);
                }
                catch (EntityException ex)
                {
                    ExceptionHandler.HandleEntityException(ex, classMethod, ExceptionHandler.PullingError);
                }
                catch (DbException ex)
                {
                    ExceptionHandler.HandleDBException(ex, classMethod, ExceptionHandler.SqlMessage);
                }

                var friendships = new List<Friendship>();
                foreach (var friend in friends)
                {
                    int friendID = friend.player_id == playerID ?
                        friend.friend_id : friend.player_id;
                    var friendship = new Friendship
                    {
                        PlayerID = playerID,
                        Friend = GetPlayerById(friendID, includeFriends: false)
                    };
                    friendships.Add(friendship);
                }

                return friendships;
            }
        }

        public bool UploadAvatar(string username, byte[] avatarBytes, string fileName)
        {
            string classMethod = "ProfileManager.UploadAvatar";
            if (avatarBytes == null || avatarBytes.Length == 0)
            {
                throw new FaultException("Avatar vacío o nulo.");
            }

            using (var db = new Forbidden_FEIEntities(ConnectionString))
            {
                try
                {
                    var player = db.Player.FirstOrDefault(p => p.player_username == username)
                        ?? throw new FaultException("Usuario no encontrado.");
                    player.player_avatar_file = avatarBytes;
                    player.player_avatar_name = string.IsNullOrWhiteSpace(fileName)
                        ? DefaultAvatarName
                        : Path.GetFileName(fileName);

                    db.SaveChanges();
                    return true;
                }
                catch (SqlException ex) when (ex.Number == 2 || ex.Number == 53)
                {
                    ExceptionHandler.HandleDBException(ex, classMethod, ExceptionHandler.SqlMessage);
                }
                catch (EntityException ex)
                {
                    ExceptionHandler.HandleEntityException(ex, classMethod, ExceptionHandler.PushingError);
                }
                catch (Exception ex)
                {
                    Log.Error("ProfileManager.UploadAvatar", ex);
                    throw new FaultException("Server couldn't save the avatar.");
                }
            }

            return false;
        }

        public byte[] GetAvatar(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return new List<byte>().ToArray();
            }

            if (!Regex.IsMatch(fileName, @"^[a-f0-9]{32}\.(jpg|png|jpeg)$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(200)))
            {
                return new List<byte>().ToArray();
            }

            var avatarsDir = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "avatars"
            );

            var fullPath = Path.Combine(avatarsDir, fileName);

            if (!File.Exists(fullPath))
            {
                return new List<byte>().ToArray();
            }

            return File.ReadAllBytes(fullPath);
        }

        private bool SaveUpdateChanges(Forbidden_FEIEntities db)
        {
            string classMethod = "ProfileManager.SaveUpdateChanges";
            bool success = false;
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    db.SaveChanges();
                    transaction.Commit();
                    success = true;
                }
                catch (SqlException ex) when (ex.Number == 2 || ex.Number == 53)
                {
                    ExceptionHandler.HandleDBException(ex, classMethod, ExceptionHandler.SqlMessage);
                }
                catch (DbUpdateException ex)
                {
                    transaction.Rollback();
                    ExceptionHandler.HandleDbUpdateException(ex, classMethod, ExceptionHandler.PushingError);
                }
            }
            return success;
        }

        public bool UpdatePlayer(Contracts.Player updatedPlayer)
        {
            string classMethod = "ProfileManager.UpdatePlayer";
            bool success = false;
            using (var db = new Forbidden_FEIEntities(ConnectionString))
            {
                try
                {
                    var formerPlayer = db.Player.Find(updatedPlayer.PlayerId);
                    if (formerPlayer == null) return false;

                    formerPlayer.player_name = updatedPlayer.PlayerName;
                    formerPlayer.player_username = updatedPlayer.PlayerUsername;
                    formerPlayer.player_password = updatedPlayer.PlayerPassword;
                    formerPlayer.player_email = updatedPlayer.PlayerEmail;

                    if (updatedPlayer.PlayerAvatarBytes != null && updatedPlayer.PlayerAvatarBytes.Length > 0)
                    {
                        formerPlayer.player_avatar_file = updatedPlayer.PlayerAvatarBytes;
                        formerPlayer.player_avatar_name = string.IsNullOrWhiteSpace(updatedPlayer.PlayerAvatarName)
                            ? DefaultAvatarName
                            : updatedPlayer.PlayerAvatarName;
                    }

                    formerPlayer.is_verified = updatedPlayer.IsVerified;

                    if (ClearSocials(formerPlayer, db))
                    {
                        db.PlayerSocialmedia.AddRange(
                            updatedPlayer.SocialMedia
                                .Where(s => !string.IsNullOrWhiteSpace(s.SocialLink))
                                .Select(s => new PlayerSocialmedia
                                {
                                    social_media_name = s.SocialMediaName,
                                    social_link = s.SocialLink,
                                    player_id = formerPlayer.player_id
                                }));
                    }

                    success = SaveUpdateChanges(db);
                }
                catch (SqlException ex) when (ex.Number == 2 || ex.Number == 53)
                {
                    ExceptionHandler.HandleDBException(ex, classMethod, ExceptionHandler.SqlMessage);
                }
                catch (EntityException ex)
                {
                    ExceptionHandler.HandleEntityException(ex, classMethod, ExceptionHandler.PushingError);
                }
            }
            return success;
        }

        public bool ClearSocials(Model.Player player, Forbidden_FEIEntities db)
        {
            string classMethod = "ProfileManager.ClearSocials ";
            bool success = false;
            try
            {
                var socials = db.PlayerSocialmedia.Where(s => s.player_id == player.player_id).ToList();
                foreach (var social in socials)
                {
                    db.PlayerSocialmedia.Remove(social);
                }
                db.SaveChanges();
                success = true;
            }
            catch (SqlException ex) when (ex.Number == 2 || ex.Number == 53)
            {
                ExceptionHandler.HandleDBException(ex, classMethod, ExceptionHandler.SqlMessage);
            }
            catch (EntityException ex)
            {
                ExceptionHandler.HandleEntityException(ex, classMethod, ExceptionHandler.PushingError);
            }

            return success;
        }

        public bool DeletePlayerByUsername(string username)
        {
            Log.Info("Deleting player by username");

            bool success = false;
            string classMethod = "ProfileManager.DeletePlayerByUsername ";

            using (var db = new Forbidden_FEIEntities(ConnectionString))
            {
                try
                {
                    var playerToDelete = db.Player.FirstOrDefault(dp => dp.player_username == username);
                    if (playerToDelete != null)
                    {
                        var tokens = db.Token.Where(t => t.player_id == playerToDelete.player_id).ToList();
                        var friends = db.Friends.Where(f => f.player_id == playerToDelete.player_id).ToList();
                        var socials = db.PlayerSocialmedia.Where(
                            sm => sm.player_id == playerToDelete.player_id).ToList();

                        db.Token.RemoveRange(tokens);
                        db.Friends.RemoveRange(friends);
                        db.PlayerSocialmedia.RemoveRange(socials);
                        db.Player.Remove(playerToDelete);
                        db.SaveChanges();

                        Log.Info("Player deleted");
                        success = true;
                        return success;
                    }
                }
                catch (SqlException ex) when (ex.Number == 2 || ex.Number == 53)
                {
                    ExceptionHandler.HandleDBException(ex, classMethod, ExceptionHandler.SqlMessage);
                }
                catch (EntityException ex)
                {
                    ExceptionHandler.HandleEntityException(ex, classMethod, ExceptionHandler.PushingError);
                }
            }

            return success;
        }

        public bool ConnectPlayerByUsername(string username)
        {
            string classMethod = "ProfileManager.ConnectPlayerByUsername ";
            bool success = false;

            using (var db = new Forbidden_FEIEntities(ConnectionString))
            {
                var player = new Model.Player();
                try
                {
                    player = db.Player.FirstOrDefault(p => p.player_username == username);
                }
                catch (SqlException ex) when (ex.Number == 2 || ex.Number == 53)
                {
                    ExceptionHandler.HandleDBException(ex, classMethod, ExceptionHandler.SqlMessage);
                }
                catch (EntityException ex)
                {
                    ExceptionHandler.HandleEntityException(ex, classMethod, ExceptionHandler.PushingError);
                }

                if (player != null)
                {
                    player.player_status = 1;
                    success = SaveUpdateChanges(db);
                }
            }

            return success;
        }

        public bool DisconnectPlayerByUsername(string username)
        {
            string classMethod = "ProfileManager.ConnectPlayerByUsername ";
            bool success = false;

            using (var db = new Forbidden_FEIEntities(ConnectionString))
            {
                var player = new Model.Player();
                try
                {
                    player = db.Player.FirstOrDefault(p => p.player_username == username);
                }
                catch (SqlException ex) when (ex.Number == 2 || ex.Number == 53)
                {
                    ExceptionHandler.HandleDBException(ex, classMethod, ExceptionHandler.SqlMessage);
                }
                catch (EntityException ex)
                {
                    ExceptionHandler.HandleEntityException(ex, classMethod, ExceptionHandler.PushingError);
                }

                if (player != null)
                {
                    player.player_status = 0;
                    success = SaveUpdateChanges(db);
                }
            }

            return success;
        }
    }
}