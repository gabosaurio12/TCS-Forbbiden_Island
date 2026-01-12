using Forbbiden.Client.BoardManager;
using Forbbiden.Client.Controls;
using Forbbiden.Client.Exceptions;
using Forbbiden.Client.MatchManager;
using Forbbiden.Client.Model;
using Forbbiden.Client.Repositories;
using Forbbiden.Client.View.Games;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Forbbiden.Client.Logic
{
    public class HostLogic
    {
        private static BoardPage MatchBoardPage;
        private static List<string> PlayersUsername = new List<string>();

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
            List<string> usernames = new List<string>();
            foreach (var player in players)
            {
                usernames.Add(player.PlayerUsername);
            }
            usernames.Remove(ClientSession.Username);

            return usernames.ToArray();
        }

        public static async void SendBoardToPlayers(Match matchInfo)
        {
            try
            {
                if (await RegisterBoardInDB(matchInfo.MatchId))
                {
                    string[] usernames = GetPlayersUsernames(matchInfo.Players.ToList());

                    var boardDto = GetBoardPageToDto();
                    BoardPageCallbackDto callbackPage = new BoardPageCallbackDto()
                    {
                        Board = boardDto,
                        MatchId = matchInfo.MatchId,
                        PlayersUsernames = usernames
                    };

                    string boardJson = CreateCallbackBoardPageJSON(callbackPage);
                    BoardRepository.SendOnBoardCreatedCallback(boardJson, usernames.ToList());
                }
            }
            catch (ViewException ex)
            {
                ExceptionViewManager.ShowViewExceptionNotification(
                    ex, Window.GetWindow(MatchBoardPage));
            }
        }

        private static async Task<bool> RegisterBoardInDB(int matchId)
        {
            var boardControlTiles = MatchBoardPage.BoardControl.GetAllTilesFromGrid();
            var boardTiles = ConvertControlTilesToTiles(boardControlTiles);
            bool result = false;

            try
            {
                var updatedBoardTiles = await BoardRepository.RegisterBoardTiles(boardTiles);
                if (!updatedBoardTiles.Equals(boardTiles))
                    result = await BoardRepository.CreateBoard(updatedBoardTiles, matchId);
            }
            catch (ViewException ex)
            {
                throw ex;
            }

            return result;
        }

        private static List<Tile> ConvertControlTilesToTiles(List<UserControlTile> controlTiles)
        {
            var tiles = controlTiles.Select(t => new Tile
            {
                Row = t.Row,
                Column = t.Col,
                IsFlood = t.IsFlood,
                IsTreasure = t.IsTreasure,
                IsEscape = t.IsEscapeTile,
                IsLost = t.IsLost,
                ImageFileName = t.ImageFileName,
                TreasureCard = t.TreasureCard,
            })
            .ToList();
            return tiles;
        }

        private static BoardPageDto GetBoardPageToDto()
        {
            return new BoardPageDto
            {
                TreasureCaptured = MatchBoardPage.TreasuresCaptured,
                WaterLevelCount = MatchBoardPage.WaterLevelCount,

                TreasureStack = ConvertCardToCardDtoList(MatchBoardPage.TreasureStack),
                TreasureDiscardStack = ConvertCardToCardDtoList(MatchBoardPage.TreasureDiscardStack),
                FloodStack = ConvertCardToCardDtoList(MatchBoardPage.FloodStack),
                FloodDiscardStack = ConvertCardToCardDtoList(MatchBoardPage.FloodDiscardStack)
            };
        }

        private static List<CardDto> ConvertCardToCardDtoList(List<Card> cards)
        {
            List<CardDto> cardsDto = new List<CardDto>();
            foreach (Card card in cards)
            {
                cardsDto.Add(new CardDto()
                {
                    CardId = card.CardId,
                    Description = card.Description,
                    ImagePath = card.ImagePath,
                    Name = card.Name,
                    Type = card.Type,
                });
            }

            return cardsDto;
        }

        public static string CreateCallbackBoardPageJSON(BoardPageCallbackDto page)
        {
            var json = JsonSerializer.Serialize(page);
            return json;
        }

        public static void SendTurnNotificationToPlayer(string boardJson)
        {
            if (PlayersTurnIndex < PlayersUsername.Count)
            {
                string playerUsername = PlayersUsername[PlayersTurnIndex++];
                BoardRepository.SendOnPlayersTurnCallback(playerUsername);
            }
            else
            {
                PlayersTurnIndex = 0;
            }
        }
    }
}
