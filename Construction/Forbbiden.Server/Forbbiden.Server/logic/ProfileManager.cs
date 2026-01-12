using Forbbiden.Contracts;
using Forbbiden.Server.exceptionHandlers;
using Forbbiden.Server.Model;
using Forbbiden.Server.utils;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
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
        public string DefaultAvatarName = "defaultAvatar.png";
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProfileManager));
        private readonly string ConnectionString;

        public ProfileManager()
        {
            ConnectionString = ConnectionStringSingleton.GetInstance().ConnectionString;
        }

        public bool IsEmailAvailable(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                using (var db = new Forbidden_FEIEntities(ConnectionString))
                {
                    var emailResult = db.Player.FirstOrDefault(p => p.player_email == email);
                    return emailResult == null;
                }
            }
            catch (EntityException ex)
            {
                string classMethod = "ProfileManager.ValidateEmail ";
                ExceptionHandler.HandleEntityException(ex, classMethod);
            }

            return false;
        }

        public bool IsUsernameAvailable(string username)
        {
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
                catch (EntityException ex)
                {
                    string classMethod = "ProfileManager.IsUsernameAvailable ";
                    ExceptionHandler.HandleEntityException(ex, classMethod);
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
            message.To.Add(new MailAddress(email));
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
                    ExceptionHandler.HandleSmtpException(ex, classMethod);
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

            message.To.Add(new MailAddress(email));

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
                    ExceptionHandler.HandleSmtpException(ex, "ProfileManager.SendVerificationEmail");
                }
            }

            return success;
        }

<<<<<<< HEAD
=======
        public int SignUp(Contracts.Player player)
        {
            int playerId = -1;
            using (var db = new Forbidden_FEIEntities(ConnectionString))
            {
                bool exists = db.Player.Any(p =>
                    p.player_username == player.PlayerUsername ||
                    p.player_email == player.PlayerEmail);

                if (exists)
                {
                    playerId = -2;
                    return playerId;
                }

                string avatar = Path.Combine(DefaultAvatarPath);
                string normalizedPassword = player.PlayerPassword.Normalize(NormalizationForm.FormC);
                string password = BCrypt.Net.BCrypt.HashPassword(normalizedPassword);
                Model.Player newPlayer = new Model.Player
                {
                    player_username = player.PlayerUsername,
                    player_password = password,
                    player_email = player.PlayerEmail,
                    player_name = "",
                    player_avatar = avatar,
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
                catch (EntityException ex)
                {
                    string classMethod = "ProfileManager.SignUp ";
                    ExceptionHandler.HandleEntityException(ex, classMethod);
                }
            }

            return playerId;
        }

>>>>>>> fd07bbe994b3fa18d6f9a6a6e6e4a3f26fd5f8f2
        public Contracts.Player Login(string username, string password)
        {
            Log.Info("Logging in player");

            var player = new Contracts.Player { PlayerId = -1 };

            using (var db = new Forbidden_FEIEntities(ConnectionString))
            {
                try
                {
                    var searchPlayer = db.Player.FirstOrDefault(p => p.player_username == username);

                    if (searchPlayer != null)
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
                catch (EntityException ex)
                {
                    ExceptionHandler.HandleEntityException(ex, "ProfileManager.Login");
                }
            }

            return player;
        }

<<<<<<< HEAD
        public int SignUp(Contracts.Player player)
=======
        private Contracts.Player SetPlayer(Model.Player player, bool includeFriends = true)
>>>>>>> fd07bbe994b3fa18d6f9a6a6e6e4a3f26fd5f8f2
        {
            int playerId = -1;

            using (var db = new Forbbiden_FEIEntities(ConnectionString))
            {
                bool exists = db.Player.Any(p =>
                    p.player_username == player.PlayerUsername ||
                    p.player_email == player.PlayerEmail);

                if (exists)
                {
                    return playerId;
                }

                var newPlayer = new Model.Player
                {
                    player_username = player.PlayerUsername,
                    player_password = player.PlayerPassword,
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
                catch (EntityException ex)
                {
                    ExceptionHandler.HandleEntityException(ex, "ProfileManager.SignUp");
                }
            }

            return playerId;
        }

        public bool UploadAvatar(string username, byte[] avatarBytes, string fileName)
        {
            if (avatarBytes == null || avatarBytes.Length == 0)
            {
                throw new FaultException("Avatar vacío o nulo.");
            }

            using (var db = new Forbbiden_FEIEntities(ConnectionString))
            {
                try
                {
                    var player = db.Player.FirstOrDefault(p => p.player_username == username);
                    if (player == null)
                    {
                        throw new FaultException("Usuario no encontrado.");
                    }

                    player.player_avatar_file = avatarBytes;
                    player.player_avatar_name = string.IsNullOrWhiteSpace(fileName)
                        ? DefaultAvatarName
                        : Path.GetFileName(fileName);

                    db.SaveChanges();
                    return true;
                }
                catch (EntityException ex)
                {
                    ExceptionHandler.HandleEntityException(ex, "ProfileManager.UploadAvatar");
                    throw new FaultException("Error de base de datos al guardar avatar.");
                }
                catch (Exception ex)
                {
                    Log.Error("ProfileManager.UploadAvatar", ex);
                    throw new FaultException("Server couldn't save the avatar.");
                }
            }
        }

        public AvatarResponse GetAvatarByUsername(string username)
        {
            using (var db = new Forbbiden_FEIEntities(ConnectionString))
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
                catch (EntityException ex)
                {
                    ExceptionHandler.HandleEntityException(ex, "ProfileManager.GetAvatarByUsername");
                    throw new FaultException("Error de base de datos al obtener avatar.");
                }
            }
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
                catch (EntityException ex)
                {
                    string classMethod = "ProfileManager.GetPlayerByUsername ";
                    ExceptionHandler.HandleEntityException(ex, classMethod);
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
                    catch (EntityException ex)
                    {
                        string classMethod = "ProfileManager.GetPlayerById ";
                        ExceptionHandler.HandleEntityException(ex, classMethod);
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
                catch (EntityException ex)
                {
                    string classMethod = "ProfileManager.GetFriendsByID ";
                    ExceptionHandler.HandleEntityException(ex, classMethod);
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

<<<<<<< HEAD
=======
        private static string BuildAvatarFilePath(string username, string fileName)
        {
            Directory.CreateDirectory(AvatarsDir);

            string extension = ".jpg";

            var maybeExt = Path.GetExtension(fileName);
            if (!string.IsNullOrEmpty(maybeExt))
            {
                extension = maybeExt;
            }

            var safeFileName = $"{SanitizeFileName(username)}_{Guid.NewGuid():N}{extension}";
            var avatarPath = Path.Combine(AvatarsDir, safeFileName);

            return avatarPath;
        }

        public string UploadAvatar(string username, byte[] avatarBytes, string fileName)
        {
            if (avatarBytes == null || avatarBytes.Length == 0)
            {
                throw new FaultException("Avatar vacío o nulo.");
            }
            try
            {
                var fullPath = BuildAvatarFilePath(username, fileName);

                string avatarsRoot = String.Concat(
                    Path.GetFullPath(AvatarsDir),
                    Path.DirectorySeparatorChar);

                var normalizedFullPath = Path.GetFullPath(fullPath);

                if (!normalizedFullPath.StartsWith(avatarsRoot, StringComparison.OrdinalIgnoreCase))
                {
                    throw new FaultException("Invalid avatar path");
                }

                File.WriteAllBytes(normalizedFullPath, avatarBytes);
                return Path.GetFileName(normalizedFullPath);
            }
            catch (Exception ex)
            {
                Log.Error("UploadAvatar error", ex);
                throw new FaultException("Server couldn't save the avatar.");
            }
        }

>>>>>>> fd07bbe994b3fa18d6f9a6a6e6e4a3f26fd5f8f2
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


        private static string SanitizeFileName(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return "user";
            }
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                input = input.Replace(c, '_');
            }
            return input;
        }

<<<<<<< HEAD
        private static bool SaveUpdateChanges(Forbbiden_FEIEntities db)
=======
        private static bool SaveUpdateChanges(Forbidden_FEIEntities db)
>>>>>>> fd07bbe994b3fa18d6f9a6a6e6e4a3f26fd5f8f2
        {
            bool success = false;
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    db.SaveChanges();
                    transaction.Commit();
                    success = true;
                }
                catch (DbUpdateException ex)
                {
                    transaction.Rollback();
                    string classMethod = "ProfileManager.SaveUpdateChanges";
                    ExceptionHandler.HandleDbUpdateException(ex, classMethod);
                }
            }
            return success;
        }

        public bool UpdatePlayer(Contracts.Player updatedPlayer)
        {
            bool success = false;
            using (var db = new Forbidden_FEIEntities(ConnectionString))
            {
                try
                {
<<<<<<< HEAD
                    var formerPlayer = db.Player.Find(updatedPlayer.PlayerId);
=======
                    Model.Player formerPlayer = db.Player.Find(updatedPlayer.PlayerId);
>>>>>>> fd07bbe994b3fa18d6f9a6a6e6e4a3f26fd5f8f2
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
<<<<<<< HEAD
                            updatedPlayer.SocialMedia
                                .Where(s => !string.IsNullOrWhiteSpace(s.SocialLink))
                                .Select(s => new PlayerSocialmedia
                                {
                                    social_media_name = s.SocialMediaName,
                                    social_link = s.SocialLink,
                                    player_id = formerPlayer.player_id
                                }));
=======
                        updatedPlayer.SocialMedia
                        .Where(social => !string.IsNullOrWhiteSpace(social.SocialLink))
                        .Select(social => new PlayerSocialmedia
                        {
                            social_media_name = social.SocialMediaName,
                            social_link = social.SocialLink,
                            player_id = formerPlayer.player_id
                        }));
>>>>>>> fd07bbe994b3fa18d6f9a6a6e6e4a3f26fd5f8f2
                    }

                    success = SaveUpdateChanges(db);
                }
                catch (EntityException ex)
                {
                    ExceptionHandler.HandleEntityException(ex, "ProfileManager.UpdatePlayer");
                }
            }
            return success;
        }

<<<<<<< HEAD
        public bool ClearSocials(Model.Player player)
=======
        public bool ClearSocials(Model.Player player, Forbidden_FEIEntities db)
>>>>>>> fd07bbe994b3fa18d6f9a6a6e6e4a3f26fd5f8f2
        {
            bool success = false;
            try
            {
                var socials = db.PlayerSocialmedia.Where(s => s.player_id == player.player_id).ToList();
                foreach (var social in socials)
                {
<<<<<<< HEAD
                    var socials = db.PlayerSocialmedia.Where(s => s.player_id == player.player_id).ToList();
                    foreach (var social in socials)
                    {
                        db.PlayerSocialmedia.Remove(social);
                    }
                    db.SaveChanges();
                    success = true;
                }
                catch (EntityException ex)
                {
                    string classMethod = "ProfileManager.ClearSocials ";
                    ExceptionHandler.HandleEntityException(ex, classMethod);
=======
                    db.PlayerSocialmedia.Remove(social);
>>>>>>> fd07bbe994b3fa18d6f9a6a6e6e4a3f26fd5f8f2
                }
                db.SaveChanges();
                success = true;
            }
            catch (EntityException ex)
            {
                string classMethod = "ProfileManager.ClearSocials ";
                ExceptionHandler.HandleEntityException(ex, classMethod);
            }

            return success;
        }

        public bool DeletePlayerByUsername(string username)
        {
            Log.Info("Deleting player by username");

            bool success = false;

            using (var db = new Forbidden_FEIEntities(ConnectionString))
            {
                try
                {
                    var playerToDelete = db.Player.FirstOrDefault(dp => dp.player_username == username);
                    if (playerToDelete != null)
                    {
                        var tokens = db.Token.Where(t => t.player_id == playerToDelete.player_id).ToList();
                        var friends = db.Friends.Where(f => f.player_id == playerToDelete.player_id).ToList();

                        db.Token.RemoveRange(tokens);
                        db.Friends.RemoveRange(friends);
                        db.Player.Remove(playerToDelete);
                        db.SaveChanges();

                        Log.Info("Player deleted");
                        success = true;
                        return success;
                    }
                }
                catch (EntityException ex)
                {
                    string classMethod = "ProfileManager.DeletePlayerByUsername ";
                    ExceptionHandler.HandleEntityException(ex, classMethod);
                }
            }

            return success;
        }

        public bool ConnectPlayerByUsername(string username)
        {
            bool success = false;

            using (var db = new Forbidden_FEIEntities(ConnectionString))
            {
                var player = new Model.Player();
                try
                {
                    player = db.Player.FirstOrDefault(p => p.player_username == username);
                }
                catch (EntityException ex)
                {
                    string classMethod = "ProfileManager.ConnectPlayerByUsername ";
                    ExceptionHandler.HandleEntityException(ex, classMethod);
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
            bool success = false;

            using (var db = new Forbidden_FEIEntities(ConnectionString))
            {
                var player = new Model.Player();
                try
                {
                    player = db.Player.FirstOrDefault(p => p.player_username == username);
                }
                catch (EntityException ex)
                {
                    string classMethod = "ProfileManager.ConnectPlayerByUsername ";
                    ExceptionHandler.HandleEntityException(ex, classMethod);
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