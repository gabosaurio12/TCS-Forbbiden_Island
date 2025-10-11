using ForbbidenIslandFEI_Construction;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net.Mail;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ForbbidenService.logic
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    internal class Signup
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Login));

        private void SendEmail(string email)
        {
            string receiver = email;
            string emisor = "forbbidenislandfei@gmail.com";
            MailMessage message = new MailMessage(emisor, receiver);
            message.Subject = "Register confirmation";
            message.Body = "Your account has been succesfully created, welcome to Forbbiden Island FEI Edition. Enjoy the adventure!";
            SmtpClient client = new SmtpClient("smtp.gmail.com");
            client.Port = 587;
            client.Credentials = new System.Net.NetworkCredential(emisor, "uqeosliojdotaitq");
            client.EnableSsl = true;

            try
            {
                client.Send(message);
            }
            catch (Exception ex)
            {
                log.Error("SignupWindow.xaml.cs", ex);
            }
        }

        public void SignUp(Player player)
        {
            using (var db = new Forbbiden_FEIEntities())
            {
                try
                {
                    db.Player.Add(player);
                    db.SaveChanges();
                    SendEmail(player.player_email);
                }
                catch (DbEntityValidationException ex)
                {
                    log.Error("SignupWindow.xaml.cs", ex);
                }
                catch (DbUpdateException ex)
                {
                    log.Error("SignupWindow.xaml.cs", ex);
                }
            }
        }
        public bool ValidateUsername(Player player)
        {
            using (var db = new Forbbiden_FEIEntities())
            {
                var playerSearch = db.Player.FirstOrDefault(u => u.player_username == player.player_username);
                return playerSearch == null;
            }
        }

        public bool ValidateEmail(Player player)
        {
            if (string.IsNullOrWhiteSpace(player.player_email))
            {
                return false;
            }
            if (player.player_email.Contains("@"))
            {
                using (var db = new Forbbiden_FEIEntities())
                {
                    var email = db.Player.FirstOrDefault(p => p.player_email == player.player_email);
                    return email == null;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
