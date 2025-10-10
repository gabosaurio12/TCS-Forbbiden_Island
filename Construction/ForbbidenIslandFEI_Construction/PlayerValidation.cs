using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForbbidenIslandFEI_Construction
{
    internal class PlayerValidation
    {
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

        public bool ValidatePassword(Player player)
        {
            if (!string.IsNullOrWhiteSpace(player.player_password) && player.player_password.Length > 7)
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(player.player_password, @"[A-Z]")) return false;
                if (!System.Text.RegularExpressions.Regex.IsMatch(player.player_password, @"[a-z]")) return false;
                if (!System.Text.RegularExpressions.Regex.IsMatch(player.player_password, @"[0-9]")) return false;
                if (!System.Text.RegularExpressions.Regex.IsMatch(player.player_password, @"[\W_]")) return false;
            }
            else
            {
                return false;
            }
            return true;
        }

        public void HashPassword(Player player)
        {
            player.player_password = BCrypt.Net.BCrypt.HashPassword(player.player_password);
        }
    }
}
