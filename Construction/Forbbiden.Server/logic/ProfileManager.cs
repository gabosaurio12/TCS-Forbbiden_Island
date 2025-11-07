using Forbbiden.Contracts;
using Forbbiden.Server.utils;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
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
                string avatar = "D:\\mazin\\Documents\\Codigos\\Proyectos\\TCS-Forbbiden_Island\\Construction\\Forbbiden.Client\\Images\\defaultAvatar.png";
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
    
        public Contracts.Player GetPlayerByUsername(string username)
        {
            log.Info("Retrieving player by username");

            using (var db = new Forbbiden_FEIEntities(connectionString))
            {
                try
                {
                    var playerResult = db.Player.FirstOrDefault(u => u.player_username == username);
                    if (playerResult != null)
                    {
                        log.Info("Player found");

                        var friendsClient = new FriendsManager();

                        List<Friendship> friendsList =
                            friendsClient.getFriendsByID(playerResult.player_id);
                        
                        string avatar = "D:\\mazin\\Documents\\Codigos\\Proyectos\\TCS-Forbbiden_Island\\Construction\\Forbbiden.Client\\Images\\defaultAvatar.png";

                        return new Contracts.Player
                        {
                            PlayerId = playerResult.player_id,
                            PlayerUsername = playerResult.player_username,
                            PlayerName = playerResult.player_name ?? "",
                            PlayerPassword = playerResult.player_password,
                            PlayerEmail = playerResult.player_email,
                            PlayerAvatarPath = playerResult.player_avatar ?? avatar,
                            Status = playerResult.player_status ?? 0,
                            Verified = playerResult.is_verified ?? 0,
                            SocialMedia = playerResult.player_socialmedia.Select(sm => new SocialMedia
                            {
                                PlayerId = playerResult.player_id,
                                SocialMediaId = sm.social_media,
                                SocialMediaName = sm.social_media_name.Trim(),
                                SocialLink = sm.social_link
                            }).ToList(),
                            Friends = friendsList                                
                        };
                    }
                }
                catch (EntityException ex)
                {
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
                }

                return new Contracts.Player()
                {
                    PlayerId = -1
                };
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
                        var friendsClient = new FriendsManager();

                        List<Friendship> friendsList = 
                            friendsClient.getFriendsByID(playerResult.player_id);

                        string avatar = "D:\\mazin\\Documents\\Codigos\\Proyectos\\TCS-Forbbiden_Island\\Construction\\Forbbiden.Client\\Images\\defaultAvatar.png";

                        player = new Contracts.Player
                        {
                            PlayerId = playerResult.player_id,
                            PlayerUsername = playerResult.player_username,
                            PlayerName = playerResult.player_name ?? "",
                            PlayerPassword = playerResult.player_password,
                            PlayerEmail = playerResult.player_email,
                            PlayerAvatarPath = playerResult.player_avatar ?? avatar,
                            Status = playerResult.player_status ?? 0,
                            Verified = playerResult.is_verified ?? 0,
                            SocialMedia = playerResult.player_socialmedia.Select(sm => new SocialMedia
                            {
                                PlayerId = playerResult.player_id,
                                SocialMediaId = sm.social_media,
                                SocialMediaName = sm.social_media_name.Trim(),
                                SocialLink = sm.social_link
                            }).ToList(),
                            Friends = friendsList
                        };
                    }
                }
                catch (EntityException ex)
                {
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
                }
            }

            log.Info("Current logged-in player retrieved");
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
                        string avatar = "D:\\mazin\\Documents\\Codigos\\Proyectos\\TCS-Forbbiden_Island\\Construction\\Forbbiden.Client\\Images\\defaultAvatar.png";

                        player = new Contracts.Player
                        {
                            PlayerId = playerResult.player_id,
                            PlayerUsername = playerResult.player_username,
                            PlayerName = playerResult.player_name ?? "",
                            PlayerPassword = playerResult.player_password,
                            PlayerEmail = playerResult.player_email,
                            PlayerAvatarPath = playerResult.player_avatar ?? avatar,
                            Status = playerResult.player_status ?? 0,
                            Verified = playerResult.is_verified ?? 0,
                            SocialMedia = playerResult.player_socialmedia.Select(sm => new SocialMedia
                            {
                                PlayerId = playerResult.player_id,
                                SocialMediaId = sm.social_media,
                                SocialMediaName = sm.social_media_name.Trim(),
                                SocialLink = sm.social_link
                            }).ToList(),
                        };
                    }
                }
                catch (EntityException ex)
                {
                    Console.WriteLine(ErrorCode + ex.Message);
                    log.Error(ex);
                }
            }

            log.Info("Retrieving player by ID");
            return player;
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

                    formerPlayer.player_socialmedia.Clear();

                    db.player_socialmedia.AddRange(
                        updatedPlayer.SocialMedia
                        .Where(social => !string.IsNullOrWhiteSpace(social.SocialLink))
                        .Select(social => new player_socialmedia
                        {
                            social_media_name = social.SocialMediaName,
                            social_link = social.SocialLink,
                            player_id = formerPlayer.player_id
                        })
                    );

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
