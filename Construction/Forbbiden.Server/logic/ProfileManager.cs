using Forbbiden.Contracts;
using log4net;
using System;
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
        private const string CLASS_NAME = "ProfileManager.cs";

        public bool ValidateEmail(string email)
        {
            log.Info($"Validating email: {email}");

            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }
            if (email.Contains("@"))
            {
                try
                {
                    using (var db = new Forbbiden_FEIEntities())
                    {
                        var emailResult = db.Player.FirstOrDefault(p => p.player_email == email);
                        return emailResult == null;
                    }
                }
                catch (EntityException ex)
                {
                    Console.WriteLine("[ERROR] ProfileManager.cs - ".Concat(ex.Message));
                    log.Error(CLASS_NAME, ex);
                    throw new EntityException(ex.Message);
                }

            }

            return false;
        }

        public bool IsUsernameAvailable(string username)
        {
            log.Info($"Checking username availability: {username}"));

            using (var db = new Forbbiden_FEIEntities())
            {
                try
                {
                    var playerResult = db.Player.FirstOrDefault(u => u.player_username == username);
                    return playerResult == null;
                }
                catch (EntityException ex)
                {
                    Console.WriteLine("[ERROR] ProfileManager.cs - ".Concat(ex.Message));
                    log.Error(CLASS_NAME, ex);
                    throw new EntityException(ex.Message);
                }
            }
        }

        public bool SendEmail(string email)
        {
            log.Info($"Sending email to: {email}");

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
                log.Info($"Email sent to: {email}");
            }
            catch (SmtpException ex)
            {
                Console.WriteLine("[ERROR] ProfileManager.cs - ".Concat(ex.Message));
                log.Error(CLASS_NAME, ex);
                throw new Exception(ex.Message);
            }

            return success;
        }

        public bool SignUp(Contracts.Player player)
        {
            log.Info($"Signing up new player: {player.PlayerUsername}");

            bool success = true;
            using (var db = new Forbbiden_FEIEntities())
            {
                Player newPlayer = new Player
                {
                    player_username = player.PlayerUsername,
                    player_password = player.PlayerPassword,
                    player_email = player.PlayerEmail
                };
                try
                {
                    db.Player.Add(newPlayer);
                    db.SaveChanges();
                    log.Info($"New player signed up: {player.PlayerUsername}");
                    SendEmail(newPlayer.player_email);
                }
                catch (DbEntityValidationException ex)
                {
                    Console.WriteLine("[ERROR] ProfileManager.cs - ".Concat(ex.Message));
                    log.Error(CLASS_NAME, ex);
                    throw new DbEntityValidationException(ex.Message);
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine("[ERROR] ProfileManager.cs - ".Concat(ex.Message));
                    log.Error(CLASS_NAME, ex);
                    throw new DbEntityValidationException(ex.Message);
                }
            }
            return success;
        }

        public bool Login(Contracts.Player player)
        {
            log.Info($"Logging in player: {player.PlayerUsername}");

            bool success = false;
            using (var db = new Forbbiden_FEIEntities())
            {
                db.LoginPlayer.Add(new LoginPlayer
                {
                    login_player_id = player.PlayerId,
                });
                try
                {
                    db.SaveChanges();
                    success = true;
                    log.Info($"Player logged in: {player.PlayerUsername}");
                }
                catch (DbEntityValidationException ex)
                {
                    Console.WriteLine("[ERROR] ProfileManager.cs - ".Concat(ex.Message));
                    log.Error(CLASS_NAME, ex);
                    throw new DbEntityValidationException(ex.Message);
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine("[ERROR] ProfileManager.cs - ".Concat(ex.Message));
                    log.Error(CLASS_NAME, ex);
                    throw new DbUpdateException(ex.Message);
                }
            }

            return success;
        }
    
        public Contracts.Player GetPlayerByUsername(string username)
        {
            log.Info($"Retrieving player by username: {username}");

            using (var db = new Forbbiden_FEIEntities())
            {
                try
                {
                    var playerResult = db.Player.FirstOrDefault(u => u.player_username == username);
                    if (playerResult != null)
                    {
                        log.Info(String.Format("Player found: {0}", username));
                        return new Contracts.Player
                        {
                            PlayerId = playerResult.player_id,
                            PlayerUsername = playerResult.player_username,
                            PlayerPassword = playerResult.player_password,
                            PlayerEmail = playerResult.player_email
                        };
                    }
                }
                catch (EntityException ex)
                {
                    Console.WriteLine("[ERROR] ProfileManager.cs - ".Concat(ex.Message));
                    log.Error(CLASS_NAME, ex);
                    throw new EntityException(ex.Message);
                }

                return null;
            }
        }

        public Contracts.Player GetCurrentLogin()
        {
            log.Info("Retrieving current logged-in player");

            Contracts.Player player = null;
            using (var db = new Forbbiden_FEIEntities())
            {
                try
                {
                    int current_id = db.LoginPlayer.Select(lp => lp.login_player_id).FirstOrDefault();
                    Player searchPlayer = db.Player.Include("player_socialmedia").FirstOrDefault(p => p.player_id == current_id);

                    if (searchPlayer != null)
                    {
                        player = new Contracts.Player
                        {
                            PlayerId = searchPlayer.player_id,
                            PlayerUsername = searchPlayer.player_username,
                            PlayerName = searchPlayer.player_name,
                            PlayerPassword = searchPlayer.player_password,
                            PlayerEmail = searchPlayer.player_email,
                            PlayerAvatarPath = searchPlayer.player_avatar,
                            SocialMedia = searchPlayer.player_socialmedia.Select(sm => new SocialMedia
                            {
                                PlayerId = sm.player_id ?? 0,
                                SocialMediaId = sm.social_media,
                                SocialMediaName = sm.social_media_name.Trim(),
                                SocialLink = sm.social_link
                            }).ToList()
                        };
                    }
                }
                catch (EntityException ex)
                {
                    Console.WriteLine("[ERROR] ProfileManager.cs - ".Concat(ex.Message));
                    log.Error(CLASS_NAME, ex);
                    throw new EntityException(ex.Message);
                }
            }
            string playerUsername = player != null ? player.PlayerUsername : "None";
            log.Info($"Current logged-in player retrieved: {playerUsername}");
            return player;
        }

        public bool ClearCurrentLogin()
        {
            log.Info("Clearing current logged-in player");

            bool success = false;
            using (var db = new Forbbiden_FEIEntities())
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
                    Console.WriteLine("[ERROR] ProfileManager.cs - ".Concat(ex.Message));
                    log.Error(CLASS_NAME, ex);
                    throw new DbEntityValidationException(ex.Message);
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine("[ERROR] ProfileManager.cs - ".Concat(ex.Message));
                    log.Error(CLASS_NAME, ex);
                    throw new DbUpdateException(ex.Message);
                }
            }

            log.Info("Current logged-in player cleared");
            return success;
        }

        public Contracts.Player GetPlayerById(int playerId)
        {
            log.Info($"Retrieving player by ID: {playerId}"));

            Contracts.Player player = null;
            using (var db = new Forbbiden_FEIEntities())
            {
                try
                {
                    var playerResult = db.Player.Find(playerId);
                    if (playerResult != null)
                    {
                        player = new Contracts.Player
                        {
                            PlayerId = playerResult.player_id,
                            PlayerName = playerResult.player_name,
                            PlayerUsername = playerResult.player_username,
                            PlayerPassword = playerResult.player_password,
                            PlayerEmail = playerResult.player_email,
                            PlayerAvatarPath = playerResult.player_avatar
                        };
                    }
                }
                catch (EntityException ex)
                {
                    Console.WriteLine("[ERROR] ProfileManager.cs - ".Concat(ex.Message));
                    log.Error(CLASS_NAME, ex);
                    throw new EntityException(ex.Message);
                }
            }

            string playerUsername = player != null ? player.PlayerUsername : "None";
            log.Info($"Player retrieved by ID: {playerUsername}");
            return player;
        }

        public bool UpdatePlayer(Contracts.Player updatedPlayer)
        {
            log.Info($"Updating player: {updatedPlayer.PlayerUsername}");

            bool success = false;
            using (var db = new Forbbiden_FEIEntities())
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

                    foreach(var social in updatedPlayer.SocialMedia)
                    {
                        if (!string.IsNullOrWhiteSpace(social.SocialLink))
                        {
                            db.player_socialmedia.Add(new player_socialmedia
                            {
                                social_media_name = social.SocialMediaName,
                                social_link = social.SocialLink,
                                player_id = formerPlayer.player_id
                            });
                        }
                    }

                    db.SaveChanges();
                    success = true;
                    log.Info($"Player updated: {updatedPlayer.PlayerUsername}");
                }
                catch (DbEntityValidationException ex)
                {
                    Console.WriteLine("[ERROR] ProfileManager.cs - ".Concat(ex.Message));
                    log.Error(CLASS_NAME, ex);
                    throw new DbEntityValidationException(ex.Message);
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine("[ERROR] ProfileManager.cs - ".Concat(ex.Message));
                    log.Error(CLASS_NAME, ex);
                    throw new DbUpdateException(ex.Message);
                }
            }
            return success;
        }

        public bool DeletePlayerByUsername(string username)
        {
            log.Info($"Deleting player by username: {username}");

            bool success = false;

            using (var db = new Forbbiden_FEIEntities())
            {
                try
                {
                    var playerToDelete = db.Player.FirstOrDefault(dp => dp.player_username == username);
                    if (playerToDelete != null)
                    {
                        db.Player.Remove(playerToDelete);
                        db.SaveChanges();

                        log.Info("Player deleted: " + username);
                        success = true;
                        return success;
                    }
                }
                catch (DbEntityValidationException ex)
                {
                    Console.WriteLine("[ERROR] ProfileManager.cs - ".Concat(ex.Message));
                    log.Error(CLASS_NAME, ex);
                    throw new DbEntityValidationException(ex.Message);
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine("[ERROR] ProfileManager.cs - ".Concat(ex.Message));
                    log.Error(CLASS_NAME, ex);
                    throw new DbUpdateException(ex.Message);
                }
            }

            return success;
        }
    }
}
