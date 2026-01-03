using Forbbiden.Client.BoardManager;
using Forbbiden.Client.MatchManager;
using Forbbiden.Client.view.games;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Forbbiden.Client.logic
{
    public class HostLogic
    {
        private static BoardPage MatchBoardPage;
        private static List<string> PlayersUsername = new List<string>();

        private static BoardManagerClient BoardClient = new BoardManagerClient();
        private static int PlayersTurnIndex = 0;

        protected HostLogic()
        {
            MatchNotificationsSingleton.Instance.OnTurnFinished += SendTurnNotificationToPlayer;
        }

        public static void SubscribePlayers(List<PlayerInfo> players)
        {
            foreach (var player in players)
            {
                MatchNotificationsSingleton.Instance.Subscribe(player.PlayerUsername);
            }
        }

        public static void SetBoardPage(BoardPage page)
        {
            MatchBoardPage = page;
        }

        public static void SetPlayersTurnOrder(List<PlayerInfo> players)
        {
            PlayersUsername = GetPlayersUsernames(players).ToList();
        }

        public static string[] GetPlayersUsernames(List<PlayerInfo> players)
        {
            string[] usernames = new string[players.Count];
            int usernameIndex = 0;
            foreach (var player in players)
            {
                usernames[usernameIndex++] = player.PlayerUsername;
            }

            return usernames;
        }

        public static string CreateCallbackBoardPageJSON(CallbackBoardPage page)
        {
            return JsonSerializer.Serialize(page);
        }

        public static void SendBoardToPlayers(Match matchInfo)
        {
            string[] usernames = GetPlayersUsernames(matchInfo.Players.ToList());
            CallbackBoardPage callbackPage = new CallbackBoardPage(
                MatchBoardPage, usernames);
            string boardJson = CreateCallbackBoardPageJSON(callbackPage);
            BoardClient.SendOnBoardCreatedCallbackAsync(boardJson, usernames);
        }

        public static void SendTurnNotificationToPlayer(string boardJson)
        {
            if (PlayersTurnIndex < PlayersUsername.Count)
            {
                string playerUsername = PlayersUsername[PlayersTurnIndex++];
                BoardClient.SendOnPlayersTurnCallback(playerUsername);
            }
            else
            {
                PlayersTurnIndex = 0;
            }
        }
    }
}
