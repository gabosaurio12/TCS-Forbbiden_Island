using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForbbidenIslandFEI_Construction.model
{
    public class PlayerClient
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PlayerClient()
        {
            this.match_players = new HashSet<match_players>();
            this.player_socialmedia = new HashSet<player_socialmedia>();
        }

        public int player_id { get; set; }
        public string player_name { get; set; }
        public string player_username { get; set; }
        public string player_password { get; set; }
        public string player_email { get; set; }
        public string player_avatar { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<match_players> match_players { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<player_socialmedia> player_socialmedia { get; set; }
        public virtual LoginPlayer LoginPlayer { get; set; }

        public bool ValidatePassword(string password)
        {
            if (!string.IsNullOrWhiteSpace(password) && password.Length > 7)
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]")) return false;
                if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-z]")) return false;
                if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[0-9]")) return false;
                if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[\W_]")) return false;
            }
            else
            {
                return false;
            }
            return true;
        }

        public void HashPassword(PlayerClient player)
        {
            player.player_password = BCrypt.Net.BCrypt.HashPassword(player.player_password);
        }
    }
}
