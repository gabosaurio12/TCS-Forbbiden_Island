using Forbbiden.Client.BoardManager;
using Forbbiden.Client.MatchManager;
using Forbbiden.Client.model;
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

        private static readonly BoardManagerClient BoardClient = new BoardManagerClient();
        private static int PlayersTurnIndex = 0;

        protected HostLogic()
        {
            MatchNotificationsSingleton.Instance.OnTurnFinished += SendTurnNotificationToPlayer;
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

        public static string CreateCallbackBoardPageJSON(BoardPageCallbackDto page)
        {
            var json = JsonSerializer.Serialize(page);
            return json;
        }

        public static void SendBoardToPlayers(Match matchInfo)
        {
            string[] usernames = GetPlayersUsernames(matchInfo.Players.ToList());
            var boardDto = BoardPageToDto();
            BoardPageCallbackDto callbackPage = new BoardPageCallbackDto(
                boardDto, usernames);

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

        private static BoardPageDto BoardPageToDto()
        {
            return new BoardPageDto
            {
                ActionsRemain = MatchBoardPage.ActionsRemain,
                TreasureCaptured = MatchBoardPage.TreasuresCaptured,
                WaterLevelCount = MatchBoardPage.WaterLevelCount,

                TreasureStack = MatchBoardPage.TreasureStack,
                TreasureDiscardStack = MatchBoardPage.TreasureDiscardStack,
                FloodStack = MatchBoardPage.FloodStack,
                FloodDiscardStack = MatchBoardPage.FloodDiscardStack,

                Tiles = MatchBoardPage.BoardControl.GetAllTilesFromGrid()
                    .Select(t => new TileDto
                    {
                        Row = t.Row,
                        Column = t.Col,
                        IsFlood = t.IsFlood,
                        IsLost = t.IsLost,
                        IsTreasure = t.IsTreasure,
                        IsEscapeTile = t.IsEscapeTile,
                        ImageFileName = t.ImageFileName,
                        TreasureCard = t.TreasureCard
                    })
                    .ToList()
            };
        }
    }
}
