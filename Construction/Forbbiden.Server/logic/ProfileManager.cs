using Forbbiden.Contracts;
using log4net;
using System;
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

        public bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }
            if (email.Contains("@"))
            {
                using (var db = new Forbbiden_FEIEntities())
                {
                    var emailResult = db.Player.FirstOrDefault(p => p.player_email == email);
                    return emailResult == null;
                }
            }

            return false;
        }

        public bool IsUsernameAvailable(string username)
        {
            using (var db = new Forbbiden_FEIEntities())
            {
                try
                {
                    var playerResult = db.Player.FirstOrDefault(u => u.player_username == username);
                    return playerResult == null;
                }
                catch (Exception ex)
                {
                    log.Error("ProfileManager.cs", ex);
                    return false;
                }
            }
        }

        public bool SendEmail(string email)
        {
            bool success = true;
            string receiver = email;
            string emisor = "forbbidenislandfei@gmail.com";
            MailMessage message = new MailMessage(emisor, receiver);
            message.Subject = "Register confirmation";
            message.Body = "Your account has been succesfully created, welcome to Forbbiden Island FEI Edition. Enjoy the adventure!";
            SmtpClient client = new SmtpClient("smtp.gmail.com");
            client.Port = 587;
            string emailCode = Properties.Settings.Default.emailCode;
            client.Credentials = new System.Net.NetworkCredential(emisor, emailCode);
            client.EnableSsl = true;

            try
            {
                client.Send(message);
            }
            catch (Exception ex)
            {
                log.Error("ProfileManager.cs", ex);
                success = false;
            }

            return success;
        }

        public bool SignUp(Contracts.Player player)
        {
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
                    SendEmail(newPlayer.player_email);
                }
                catch (DbEntityValidationException ex)
                {
                    log.Error("ProfileManager.cs", ex);
                    success = false;
                }
                catch (DbUpdateException ex)
                {
                    log.Error("ProfileManager.cs", ex);
                    success = false;
                }
            }
            return success;
        }

        public bool Login(Contracts.Player player)
        {
            bool success = true;
            using (var db = new Forbbiden_FEIEntities())
            {
                db.LoginPlayer.Add(new LoginPlayer
                {
                    login_player_id = player.PlayerId,
                });
                try
                {
                    db.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    success = false;
                    log.Error("ProfileManager.cs", ex);
                }
                catch (DbUpdateException ex)
                {
                    success = false;
                    log.Error("ProfileManager.cs", ex);
                }
                catch (Exception ex)
                {
                    success = false;
                    log.Error("ProfileManager.cs", ex);
                }
            }

            return success;
        }
    
        public Contracts.Player GetPlayerByUsername(string username)
        {
            using (var db = new Forbbiden_FEIEntities())
            {
                var playerResult = db.Player.FirstOrDefault(u => u.player_username == username);
                if (playerResult != null)
                {
                    return new Contracts.Player
                    {
                        PlayerId = playerResult.player_id,
                        PlayerUsername = playerResult.player_username,
                        PlayerPassword = playerResult.player_password,
                        PlayerEmail = playerResult.player_email
                    };
                }
                return null;
            }
        }

        public Contracts.Player GetCurrentLogin()
        {
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
                catch (InvalidOperationException ex)
                {
                    log.Error("ProfileManager.cs", ex);
                }
                catch (ArgumentException ex)
                {
                    log.Error("ProfileManager.cs", ex);
                }
                catch (Exception ex)
                {
                    log.Error("ProfileManager.cs", ex);
                }
            }
            return player;
        }

        public bool ClearCurrentLogin()
        {
            bool success = true;
            using (var db = new Forbbiden_FEIEntities())
            {
                try
                {
                    var loggedInPlayers = db.LoginPlayer.ToList();
                    db.LoginPlayer.RemoveRange(loggedInPlayers);
                    db.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    success = false;
                    log.Error("ProfileManager.cs", ex);
                }
                catch (DbUpdateException ex)
                {
                    success = false;
                    log.Error("ProfileManager.cs", ex);
                }
            }

            return success;
        }

        public Contracts.Player GetPlayerById(int playerId)
        {
            Contracts.Player player = null;
            using (var db = new Forbbiden_FEIEntities())
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
            return player;
        }

        public bool UpdatePlayer(Contracts.Player updatedPlayer)
        {
            bool success = true;
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
                }
                catch (DbEntityValidationException ex)
                {
                    success = false;
                    log.Error("ProfileManager.cs", ex);
                }
                catch (DbUpdateException ex)
                {
                    success = false;
                    log.Error("ProfileManager.cs", ex);
                }
                catch (Exception ex)
                {
                    success = false;
                    log.Error("ProfileManager.cs", ex);
                }
            }

            return success;
        }

        public bool DeletePlayerByUsername(string username)
        {
            using (var db = new Forbbiden_FEIEntities())
            {
                try
                {
                    var playerToDelete = db.Player.FirstOrDefault(dp => dp.player_username == username);
                    if (playerToDelete != null)
                    {
                        db.Player.Remove(playerToDelete);
                        db.SaveChanges();
                        return true;
                    }
                }
                catch (DbEntityValidationException ex)
                {
                    log.Error("ProfileManager.cs", ex);
                }
                catch (DbUpdateException ex)
                {
                    log.Error("ProfileManager.cs", ex);
                }
                catch (Exception ex)
                {
                    log.Error("ProfileManager.cs", ex);
                }
            }

            return false;
        }
    }
}
