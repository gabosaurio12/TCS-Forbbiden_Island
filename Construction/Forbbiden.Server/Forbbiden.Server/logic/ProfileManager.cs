using Forbbiden.Contracts;
using Forbbiden.Server.exceptionHandlers;
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

namespace Forbbiden.Server.logic
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ProfileManager : IProfileManager
    {

        private static readonly ILog Log = LogManager.GetLogger(typeof(ProfileManager));
        private readonly string ConnectionString;
        private readonly string DefaultAvatarPath = "defaultAvatar.png";
        private readonly string DatabaseError = "Database Error";
        private readonly string EntityError = "EntityException";

        public ProfileManager()
        {
            ConnectionString = ConnectionStringSingleton.GetInstance().connectionString;
        }

        public bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }
            if (email.Contains("@"))
            {
                try
                {
                    using (var db = new Forbbiden_FEIEntities(ConnectionString))
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
            }

            return false;
        }

        public bool IsUsernameAvailable(string username)
        {
            bool usernameFound = false;

            using (var db = new Forbbiden_FEIEntities(ConnectionString))
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

        public bool SendEmail(string email, int playerId)
        {
            Log.Info("Sending email");

            bool success = false;
            string receiver = email;
            string emisor = Properties.email.Default.emailAddress;

            var token = new TokenManager().GenerateToken(playerId);

            using (var message = new MailMessage(emisor, receiver))
            using (var client = new SmtpClient(Properties.email.Default.smtp))
            {
                message.Subject = "Register confirmation";
                message.Body = "Your account has been succesfully created. \n" +
                    "This is your token: " + token.TokenString + "\n" +
                    "Welcome to Forbbiden Island FEI Edition. Enjoy the adventure!";
                client.Port = 587;
                string emailCode = Properties.email.Default.emailCode;
                client.Credentials = new System.Net.NetworkCredential(emisor, emailCode);
                client.EnableSsl = true;

                try
                {
                    client.Send(message);
                    Log.Info("Email sent");
                    success = true;
                }
                catch (SmtpException ex)
                {
                    string classMethod = "ProfileManager.SendEmail";
                    ExceptionHandler.HandleSmtpException(ex, classMethod);
                }
            }

            return success;
        }

        public int SignUp(Contracts.Player player)
        {
            Log.Info("Signing up new player");

            int playerId = -1;
            using (var db = new Forbbiden_FEIEntities(ConnectionString))
            {
                string avatar = Path.Combine(DefaultAvatarPath);
                Player newPlayer = new Player
                {
                    player_username = player.PlayerUsername,
                    player_password = player.PlayerPassword,
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

        public Contracts.Player Login(string username, string password)
        {
            Log.Info("Logging in player");

            Contracts.Player player = new Contracts.Player
            {
                PlayerId = -1
            };

            using (var db = new Forbbiden_FEIEntities(ConnectionString))
            {
                try
                {
                    var searchPlayer = db.Player.First(p => p.player_username == username);

                    if (searchPlayer != null)
                    {
                        if (BCrypt.Net.BCrypt.Verify(password, searchPlayer.player_password))
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
                    string classMethod = "ProfileManager.Login ";
                    ExceptionHandler.HandleEntityException(ex, classMethod);
                }
            }

            return player;
        }

        private Contracts.Player SetPlayer(Player player, bool includeFriends = true)
        {
            List<Friendship> friendsList = new List<Friendship>();
            if (includeFriends)
                friendsList = GetFriendsByID(player.player_id);

            string avatar = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Images",
                DefaultAvatarPath);

            return new Contracts.Player
            {
                PlayerId = player.player_id,
                PlayerUsername = player.player_username,
                PlayerName = player.player_name ?? "",
                PlayerPassword = player.player_password,
                PlayerEmail = player.player_email,
                PlayerAvatarPath = player.player_avatar ?? avatar,
                Status = (int)player.player_status,
                IsVerified = (int)player.is_verified,
                SocialMedia = player.player_socialmedia.Select(sm => new SocialMedia
                {
                    PlayerId = player.player_id,
                    SocialMediaId = sm.social_media,
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
            using (var db = new Forbbiden_FEIEntities(ConnectionString))
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
            using (var db = new Forbbiden_FEIEntities(ConnectionString))
            {
                try
                {
                    var playerResult = db.Player.Find(playerId);
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
            using (var db = new Forbbiden_FEIEntities(ConnectionString))
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

        public bool ClearSocials(Player player)
        {
            bool success = false;
            using (var db = new Forbbiden_FEIEntities(ConnectionString))
            {
                try
                {
                    var socials = db.player_socialmedia.Where(s => s.player_id == player.player_id).ToList();
                    foreach (var social in socials)
                    {
                        db.player_socialmedia.Remove(social);
                    }
                    db.SaveChanges();
                    success = true;
                }
                catch (EntityException ex)
                {
                    string classMethod = "ProfileManager.ClearSocials ";
                    ExceptionHandler.HandleEntityException(ex, classMethod);
                }
            }

            return success;
        }

        public string UploadAvatar(string username, byte[] avatarBytes, string fileName)
        {
            if (avatarBytes == null || avatarBytes.Length == 0)
                throw new FaultException("Avatar vacío o nulo.");

            try
            {
                var avatarsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "avatars");
                if (!Directory.Exists(avatarsDir)) Directory.CreateDirectory(avatarsDir);

                // Obtener extensión segura
                string ext = ".jpg";
                try
                {
                    var maybeExt = Path.GetExtension(fileName);
                    if (!string.IsNullOrEmpty(maybeExt))
                        ext = maybeExt;
                }
                catch { /* ignore and fallback */ }

                // Construir nombre único
                var safeFileName = $"{SanitizeFileName(username)}_{Guid.NewGuid():N}{ext}";
                var fullPath = Path.Combine(avatarsDir, safeFileName);

                File.WriteAllBytes(fullPath, avatarBytes);

                Log.Info($"Avatar uploaded for {username}, saved as {safeFileName}");
                return safeFileName;
            }
            catch (Exception ex)
            {
                Log.Error("UploadAvatar error", ex);
                throw new FaultException("No se pudo guardar el avatar en el servidor.");
            }
        }

        public byte[] GetAvatar(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;

            try
            {
                var avatarsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "avatars");
                var fullPath = Path.Combine(avatarsDir, fileName);
                if (!File.Exists(fullPath)) return null;

                return File.ReadAllBytes(fullPath);
            }
            catch (Exception ex)
            {
                Log.Warn($"GetAvatar failed for {fileName}", ex);
                return null;
            }
        }

        // Helper para sanitizar nombres simples (quitar path separators)
        private string SanitizeFileName(string input)
        {
            if (string.IsNullOrEmpty(input)) return "user";
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                input = input.Replace(c, '_');
            }
            return input;
        }
        
        private bool SaveUpdateChanges()
        {
            bool success = false;
            using (var db = new Forbbiden_FEIEntities(ConnectionString))
            {
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
            }
            return success;
        }

        public bool UpdatePlayer(Contracts.Player updatedPlayer)
        {
            bool success = false;
            using (var db = new Forbbiden_FEIEntities(ConnectionString))
            {
                try
                {
                    Player formerPlayer = db.Player.Find(updatedPlayer.PlayerId);
                    if (formerPlayer == null) return false;

                    formerPlayer.player_name = updatedPlayer.PlayerName;
                    formerPlayer.player_username = updatedPlayer.PlayerUsername;
                    formerPlayer.player_email = updatedPlayer.PlayerEmail;
                    formerPlayer.player_avatar = updatedPlayer.PlayerAvatarPath;
                    formerPlayer.is_verified = updatedPlayer.IsVerified;

                    if (ClearSocials(formerPlayer))
                    {
                        db.player_socialmedia.AddRange(
                        updatedPlayer.SocialMedia
                        .Where(social => !string.IsNullOrWhiteSpace(social.SocialLink))
                        .Select(social => new player_socialmedia
                        {
                            social_media_name = social.SocialMediaName,
                            social_link = social.SocialLink,
                            player_id = formerPlayer.player_id
                        }));
                    }

                    success = SaveUpdateChanges();                                        
                }
                catch (EntityException ex)
                {
                    string classMethod = "ProfileManager.UpdatePlayer ";
                    ExceptionHandler.HandleEntityException(ex, classMethod);
                }
            }
            return success;
        }

        public bool DeletePlayerByUsername(string username)
        {
            Log.Info("Deleting player by username");

            bool success = false;

            using (var db = new Forbbiden_FEIEntities(ConnectionString))
            {
                try
                {
                    var playerToDelete = db.Player.FirstOrDefault(dp => dp.player_username == username);
                    if (playerToDelete != null)
                    {
                        var friends = db.Friends.Where(f => f.player_id == playerToDelete.player_id).ToList();

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

        private void HandleEntityException(EntityException ex, string classMethod)
        {
            Log.Error("ERROR" + classMethod, ex);

            var fault = new DBFault
            {
                Error = DatabaseError,
                Details = ex.Message
            };

            throw new FaultException<DBFault>(fault,
                new FaultReason(EntityError));
        }

        public bool ConnectPlayerByUsername(string username)
        {
            bool success = false;

            using (var db = new Forbbiden_FEIEntities(ConnectionString))
            {
                var player = new Player();
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
                    success = SaveUpdateChanges();
                }
            }

            return success;
        }

        public bool DisconnectPlayerByUsername(string username)
        {
            bool success = false;

            using (var db = new Forbbiden_FEIEntities(ConnectionString))
            {
                var player = new Player();
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
                    success = SaveUpdateChanges();
                }
            }

            return success;
        }
    }
}