using Forbbiden.Contracts;
using Forbbiden.Server.utils;
using log4net;
using System;
using System.Collections.Generic;
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
                    HandleEntityException(ex);
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
                    HandleEntityException(ex);
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
                    var fault = new EmailFault
                    {
                        Error = "SMTP Error",
                        Details = ex.Message
                    };
                    Log.Error(ex);

                    throw new FaultException<EmailFault>(fault,
                        new FaultReason("SmtpException"));
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
                    HandleEntityException(ex);
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
                    HandleEntityException(ex);
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
                    HandleEntityException(ex);
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
                    HandleEntityException(ex);
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
                    HandleEntityException(ex);
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

        public Contracts.Player GetCurrentLogin()
        {
            Log.Info("Retrieving current logged-in player");

            Contracts.Player player = null;
            using (var db = new Forbbiden_FEIEntities(ConnectionString))
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
                    HandleEntityException(ex);
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
            Log.Info("Clearing current logged-in player");

            bool success = false;
            using (var db = new Forbbiden_FEIEntities(ConnectionString))
            {
                try
                {
                    var loggedInPlayers = db.LoginPlayer.ToList();
                    db.LoginPlayer.RemoveRange(loggedInPlayers);
                    db.SaveChanges();
                    success = true;
                }
                catch (EntityException ex)
                {
                    HandleEntityException(ex);
                }
            }

            Log.Info("Current logged-in player cleared");
            return success;
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
                    HandleEntityException(ex);
                }
            }

            return success;
        }

        public bool UpdatePlayer(Contracts.Player updatedPlayer)
        {
            Log.Info("Updating player");

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

                    using (var transaction = db.Database.BeginTransaction())
                    {
                        try
                        {
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

                            db.SaveChanges();
                            transaction.Commit();
                            success = true;
                            Log.Info("Player updated");
                        }
                        catch (DbUpdateException ex)
                        {
                            transaction.Rollback();
                            Log.Error(ex);

                            var fault = new DBFault
                            {
                                Error = "Database Error",
                                Details = ex.Message
                            };

                            throw new FaultException<DBFault>(fault,
                                new FaultReason("EntityException"));
                        }                        
                    }                    
                }
                catch (EntityException ex)
                {
                    HandleEntityException(ex);
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
                    HandleEntityException(ex);
                }
            }

            return success;
        }

        private void HandleEntityException(EntityException ex)
        {
            Log.Error(ex);

            var fault = new DBFault
            {
                Error = "Database Error",
                Details = ex.Message
            };

            throw new FaultException<DBFault>(fault,
                new FaultReason("EntityException"));
        }
    }
}