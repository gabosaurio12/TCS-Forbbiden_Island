using Forbbiden.Client.BoardManager;
using Forbbiden.Client.Exceptions;
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
    public class PlayerLogic
    {
        public static BoardPage MatchBoardPage { get; set; }
        private static readonly List<string> PlayersUsernames = new List<string>();
        private static int MatchId;

        private static async Task<List<Tile>> GetBoardTilesFromRepo()
        {
            List<Tile> tiles = new List<Tile>();
            try
            {
                tiles = await BoardRepository.GetBoardTiles(MatchId);
            }
            catch (ViewException ex)
            {
                ExceptionViewManager.ShowViewExceptionNotification(
                    ex, Window.GetWindow(MatchBoardPage));
            }
            return tiles;
        }

        private static void SetPlayersUsername(string[] usernames)
        {
            for (int i = 0; i < usernames.Length; i++)
            {
                if (usernames[i] != ClientSession.Username)
                {
                    PlayersUsernames.Add(usernames[i]);
                }
            }
        }

        public static async void RefreshBoardFromJSON(string boardJson)
        {
            var auxBoardDto = JsonSerializer.Deserialize<BoardPageCallbackDto>(boardJson);
            MatchId = auxBoardDto.MatchId;
            SetPlayersUsername(auxBoardDto.PlayersUsernames);

            var boardTiles = await GetBoardTilesFromRepo();

            await Application.Current.Dispatcher.Invoke(async () =>
            {
                MatchBoardPage.TreasuresCaptured = auxBoardDto.Board.TreasureCaptured;
                MatchBoardPage.WaterLevelCount = auxBoardDto.Board.WaterLevelCount;

                MatchBoardPage.TreasureStack = ConvertCardDtoToCardList(
                    auxBoardDto.Board.TreasureStack);
                MatchBoardPage.TreasureDiscardStack = ConvertCardDtoToCardList(
                    auxBoardDto.Board.TreasureDiscardStack);
                MatchBoardPage.FloodStack = ConvertCardDtoToCardList(
                    auxBoardDto.Board.FloodStack);
                MatchBoardPage.FloodDiscardStack = ConvertCardDtoToCardList(
                    auxBoardDto.Board.FloodDiscardStack);

                MatchBoardPage.BoardControl.RefreshBoardTiles(boardTiles);
                await RefreshAvatars();
            });
        }

        private static async Task RefreshAvatars()
        {
            foreach (var playerUsername in PlayersUsernames)
            {
                try
                {
                    var coordinates = await BoardRepository.GetPlayerCoordinates(
                        MatchId, playerUsername);

                    if (coordinates.PlayerId != -1)
                    {
                        var tile = MatchBoardPage.BoardControl.GetTile(
                            coordinates.Row, coordinates.Col);
                        var player = await ProfileRepository.GetPlayerById(
                            coordinates.PlayerId, false);
                        MatchBoardPage.BoardControl.AddPlayerAvatar(player, tile);
                    }
                }
                catch (ViewException ex)
                {
                    ExceptionViewManager.ShowViewExceptionNotification(
                        ex, Window.GetWindow(MatchBoardPage));
                }

                
            }
        }

        private static List<Card> ConvertCardDtoToCardList(List<CardDto> cardsDto)
        {
            List<Card> cards = new List<Card>();
            foreach (CardDto cardDto in cardsDto)
            {
                cards.Add(new Card()
                {
                    CardId = cardDto.CardId,
                    Description = cardDto.Description,
                    ImagePath = cardDto.ImagePath,
                    Name = cardDto.Name,
                    Type = cardDto.Type,
                });
            }

            return cards;
        }

        public static void OnTurnStarted()
        {
            MatchBoardPage.StateContext.EnterNormalState();
            string title = Properties.Resources.your_turn_title;
            string message = Properties.Resources.your_turn_message;
            ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(MatchBoardPage));
        }

        public static void SendTurnFinishedCallback(BoardPage boardPage)
        {
            var client = new BoardManagerClient();
            var boardDto = BoardPageToDto();
            BoardPageCallbackDto page = new BoardPageCallbackDto()
            {
                Board = boardDto,
                MatchId = MatchId,
                PlayersUsernames = PlayersUsernames.ToArray()
            };
            string pageJson = HostLogic.CreateCallbackBoardPageJSON(page);
            client.SendOnTurnFinishedCallback(pageJson, PlayersUsernames.ToArray());
        }

        public static void OnTurnFinishedCallbackReceived(string boardJson)
        {
            var auxBoard = JsonSerializer.Deserialize<BoardPage>(boardJson);
            MatchBoardPage.TreasuresCaptured = auxBoard.TreasuresCaptured;
            MatchBoardPage.TreasureStack = auxBoard.TreasureStack.ToList();
            MatchBoardPage.TreasureDiscardStack = auxBoard.TreasureDiscardStack.ToList();
            MatchBoardPage.FloodStack = auxBoard.FloodStack.ToList();
            MatchBoardPage.FloodDiscardStack = auxBoard.FloodDiscardStack.ToList();

            RefreshBoardFromJSON(boardJson);
        }

        private static BoardPageDto BoardPageToDto()
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
    }
}
