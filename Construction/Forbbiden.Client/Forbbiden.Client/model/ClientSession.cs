using Forbbiden.Client.ProfileManager;

namespace Forbbiden.Client.logic
{
    public static class ClientSession
    {
        public static int CurrentPlayerId { get; private set; }
        public static string Name { get; set; }
        public static string Username { get; set; }
        public static string Password { get; set; }
        public static string Email { get; set; }
        public static byte[] AvatarBytes { get; set; }
        public static string AvatarName { get; set; }
        public static int Status { get; set; }
        public static int IsVerified { get; set; }

        public static void SetPlayer(Player player)
        {
            CurrentPlayerId = player.PlayerId;
            Username = player.PlayerUsername;
            Name = player.PlayerName ?? "";
            Password = player.PlayerPassword;
            Email = player.PlayerEmail;
            AvatarBytes = player.PlayerAvatarBytes;
            AvatarName = player.PlayerAvatarName;
            Status = player.Status;
            IsVerified = player.IsVerified;
        }

        public static Player GetPlayer()
        {
            return new Player
            {
                PlayerId = CurrentPlayerId,
                PlayerName = Name,
                PlayerUsername = Username,
                PlayerPassword = Password,
                PlayerEmail = Email,
                PlayerAvatarBytes = AvatarBytes,
                PlayerAvatarName = AvatarName,
                Status = Status,
                IsVerified = IsVerified
            };
        }

        public static void SetGuestSession()
        {
            var player = GetPlayer() ?? new Player();
            player.PlayerId = 0;
            player.PlayerUsername = "Guest";
            player.PlayerPassword = "";
            player.PlayerEmail = "";
            player.PlayerAvatarBytes = null;
            player.PlayerAvatarName = "defaultAvatar.png";
            player.Status = 1;
            player.IsVerified = 1;
            SetPlayer(player);
        }

        public static void LogOut()
        {
            CurrentPlayerId = -1;
        }
    }
}