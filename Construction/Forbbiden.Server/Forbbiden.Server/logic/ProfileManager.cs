using Forbbiden.Contracts;
using Forbbiden.Server.utils;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.ServiceModel;

namespace Forbbiden.Server.logic
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ProfileManager : IProfileManager
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(ProfileManager));
        private const string ErrorCode = "[Error] ProfileManager.cs - ";
        private readonly string connectionString;
        private readonly string defaultAvatarPath = "defaultAvatar.png";

        public ProfileManager()
        {
            connectionString = ConnectionStringSingleton.GetInstance().connectionString;
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
                    using (var db = new Forbbiden_FEIEntities(connectionString))
                    {
                        var emailResult = db.Player.FirstOrDefault(p => p.player_email == email);
                        return emailResult == null;
                    }
                }
                catch (EntityException ex)
                {
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
                }
            }

            return false;
        }

        public bool IsUsernameAvailable(string username)
        {
            bool usernameFound = false;

            using (var db = new Forbbiden_FEIEntities(connectionString))
            {
                try
                {
                    var playerResult = db.Player.FirstOrDefault(u => u.player_username == username);
                    usernameFound = playerResult == null;
                }
                catch (EntityException ex)
                {
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
                }
            }
            return usernameFound;
        }

        public bool SendEmail(string email)
        {
            log.Info("Sending email");

            bool success = true;
            string receiver = email;
            string emisor = Properties.email.Default.emailAddress;
            MailMessage message = new MailMessage(emisor, receiver);
            message.Subject = "Register confirmation";
            message.Body = "Your account has been succesfully created, welcome to Forbbiden Island FEI Edition. Enjoy the adventure!";
            SmtpClient client = new SmtpClient(Properties.email.Default.smtp);
            client.Port = 587;
            string emailCode = Properties.email.Default.emailCode;
            client.Credentials = new System.Net.NetworkCredential(emisor, emailCode);
            client.EnableSsl = true;

            try
            {
                client.Send(message);
                log.Info("Email sent");
            }
            catch (SmtpException ex)
            {
                Console.WriteLine(ErrorCode + ex.Message);
                log.Error(ex);
            }

            return success;
        }

        public bool SignUp(Contracts.Player player)
        {
            log.Info("Signing up new player");

            bool success = true;
            using (var db = new Forbbiden_FEIEntities(connectionString))
            {
                string avatar = Path.Combine(defaultAvatarPath);
                Player newPlayer = new Player
                {
                    player_username = player.PlayerUsername,
                    player_password = player.PlayerPassword,
                    player_email = player.PlayerEmail,
                    player_name = "",
                    player_avatar = avatar,
                    player_status = 0
                };
                try
                {
                    db.Player.Add(newPlayer);
                    db.SaveChanges();
                    log.Info("New player signed up");
                    SendEmail(newPlayer.player_email);
                }
                catch (DbEntityValidationException ex)
                {
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
                }
                catch (EntityException ex)
                {
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
                }
            }
            return success;
        }

        public bool Login(Contracts.Player player)
        {
            log.Info("Logging in player");

            bool success = false;
            using (var db = new Forbbiden_FEIEntities(connectionString))
            {
                try
                {
                    db.LoginPlayer.Add(new LoginPlayer
                    {
                        login_player_id = player.PlayerId,
                    });
                
                    db.SaveChanges();
                    success = true;
                    log.Info("Player logged in");
                }
                catch (DbEntityValidationException ex)
                {
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
                }
                catch (EntityException ex)
                {
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
                }
            }

            return success;
        }
    
        private Contracts.Player SetPlayer(Player player)
        {
            List<Friendship> friendsList = GetFriendsByID(player.player_id);

            string avatar = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Images",
                defaultAvatarPath);

            return new Contracts.Player
            {
                PlayerId = player.player_id,
                PlayerUsername = player.player_username,
                PlayerName = player.player_name ?? "",
                PlayerPassword = player.player_password,
                PlayerEmail = player.player_email,
                PlayerAvatarPath = player.player_avatar ?? avatar,
                Status = player.player_status ?? 0,
                Verified = player.is_verified ?? 0,
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

        public Contracts.Player GetPlayerByUsername(string username)
        {
            log.Info("Retrieving player by username");

            Contracts.Player player = null;
            using (var db = new Forbbiden_FEIEntities(connectionString))
            {
                try
                {
                    var playerResult = db.Player.FirstOrDefault(u => u.player_username == username);
                    if (playerResult != null)
                    {
                        log.Info("Player found");

                        player = SetPlayer(playerResult);
                    }
                }
                catch (EntityException ex)
                {
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
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

        public Contracts.Player GetPlayerById(int playerId)
        {
            log.Info("Retrieving player by ID");

            Contracts.Player player = null;
            using (var db = new Forbbiden_FEIEntities(connectionString))
            {
                try
                {
                    var playerResult = db.Player.Find(playerId);
                    if (playerResult != null)
                    {
                        player = SetPlayer(playerResult);
                    }
                }
                catch (EntityException ex)
                {
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
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
            using (var db = new Forbbiden_FEIEntities(connectionString))
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
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
                }
                var friendships = new List<Friendship>();
                foreach (var friend in friends)
                {
                    int friendID = friend.player_id == playerID ?
                        friend.friend_id : friend.player_id;
                    var friendship = new Friendship
                    {
                        PlayerID = playerID,
                        Friend = GetPlayerById(friendID)
                    };
                    friendships.Add(friendship);
                }

                return friendships;
            }
        }

        public Contracts.Player GetCurrentLogin()
        {
            log.Info("Retrieving current logged-in player");

            Contracts.Player player = null;
            using (var db = new Forbbiden_FEIEntities(connectionString))
            {
                try
                {
                    int current_id = db.LoginPlayer.Select(lp => lp.login_player_id).FirstOrDefault();
                    Player playerResult = db.Player.Include("player_socialmedia")
                        .FirstOrDefault(p => p.player_id == current_id);

                    if (playerResult != null)
                    {
                        player = SetPlayer(playerResult);
                    }
                }
                catch (EntityException ex)
                {
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
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

        public bool ClearCurrentLogin()
        {
            log.Info("Clearing current logged-in player");

            bool success = false;
            using (var db = new Forbbiden_FEIEntities(connectionString))
            {
                try
                {
                    var loggedInPlayers = db.LoginPlayer.ToList();
                    db.LoginPlayer.RemoveRange(loggedInPlayers);
                    db.SaveChanges();
                    success = true;
                }
                catch (DbEntityValidationException ex)
                {
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
                }
                catch (EntityException ex)
                {
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
                }
            }

            log.Info("Current logged-in player cleared");
            return success;
        }

        public bool ClearSocials(Player player)
        {
            bool success = false;
            using (var db = new Forbbiden_FEIEntities(connectionString))
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
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
                }
            }

            return success;
        }

        public bool UpdatePlayer(Contracts.Player updatedPlayer)
        {
            log.Info("Updating player");

            bool success = false;
            using (var db = new Forbbiden_FEIEntities(connectionString))
            {
                try
                {
                    Player formerPlayer = db.Player.Find(updatedPlayer.PlayerId);
                    if (formerPlayer == null) return false;
                    
                    formerPlayer.player_name = updatedPlayer.PlayerName;
                    formerPlayer.player_username = updatedPlayer.PlayerUsername;
                    formerPlayer.player_email = updatedPlayer.PlayerEmail;
                    formerPlayer.player_avatar = updatedPlayer.PlayerAvatarPath;
                    Console.WriteLine("Avatar path: " + updatedPlayer.PlayerAvatarPath);
                    Console.WriteLine("Former Avatar path: " + formerPlayer.player_avatar);

                    if (ClearSocials(formerPlayer)) {
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

                    db.SaveChanges();
                    success = true;
                    log.Info("Player updated");
                }
                catch (DbEntityValidationException ex)
                {
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
                }
                catch (EntityException ex)
                {
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
                }
            }
            return success;
        }

        public bool DeletePlayerByUsername(string username)
        {
            log.Info("Deleting player by username");

            bool success = false;

            using (var db = new Forbbiden_FEIEntities(connectionString))
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

                        log.Info("Player deleted");
                        success = true;
                        return success;
                    }
                }
                catch (DbEntityValidationException ex)
                {
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
                }
                catch (EntityException ex)
                {
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
                }
            }

            return success;
        }
    }
}
