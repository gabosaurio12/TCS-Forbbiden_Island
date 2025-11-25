using Forbbiden.Client.ProfileManager;

namespace Forbbiden.Client.logic
{
    public class ClientSession
    {
        public static int CurrentPlayerId { get; private set; }
        public static string Name { get; set; }
        public static string Username { get; set; }
        public static string Password { get; set; }
        public static string Email { get; set; }
        public static string AvatarPath { get; set; }
        public static int Status { get; set; }
        public static int IsVerified { get; set; }

        public static void SetPlayer(Player player) {
            CurrentPlayerId = player.PlayerId;
            Username = player.PlayerUsername;
            Name = player.PlayerName ?? "";
            Password = player.PlayerPassword;
            Email = player.PlayerEmail;
            AvatarPath = player.PlayerAvatarPath;
            Status = player.Status;
            IsVerified = player.IsVerified;
        }

        public static Player GetPlayer()
        {
            Player player = new Player
            {
                PlayerId = CurrentPlayerId,
                PlayerName = Name,
                PlayerUsername = Username,
                PlayerPassword = Password,
                PlayerEmail = Email,
                PlayerAvatarPath = AvatarPath,
                Status = Status,
                IsVerified = IsVerified
            };

            return player;
        }

        public static void LogOut()
        {
            CurrentPlayerId = -1;
        }
    }
}
