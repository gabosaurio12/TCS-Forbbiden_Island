using Forbbiden.Client.BoardManager;
using Forbbiden.Client.Controls;
using Forbbiden.Client.Model;
using Forbbiden.Client.View.Games;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace Forbbiden.Client.Logic
{
    public class PlayerLogic
    {
        public static BoardPage MatchBoardPage { get; set; }
        private static List<string> PlayersUsername = new List<string>();
        private static int MatchId;

        protected PlayerLogic()
        {
            MatchNotificationsSingleton.Instance.OnBoardCreated += CreateBoardPageFromJSON;
            MatchNotificationsSingleton.Instance.OnBoardUpdated += RefreshBoardFromJSON;
            MatchNotificationsSingleton.Instance.OnPlayersTurn += OnTurnStarted;
            MatchNotificationsSingleton.Instance.OnTurnFinished += OnTurnFinishedCallbackReceived;
        }

        public static void CreateBoardPageFromJSON(string boardJson)
        {
            var auxBoardDto = JsonSerializer.Deserialize<BoardPageCallbackDto>(boardJson);
            MatchId = auxBoardDto.MatchId;

            var boardPage = BoardDtoToBoardPage(auxBoardDto.Board);
            MatchBoardPage = boardPage;
            PlayersUsername = auxBoardDto.PlayersUsernames.ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                MatchBoardPage.ReloadPage(boardPage);
            });
        }

        public static void RefreshBoardFromJSON(string boardJson)
        {
            var auxBoard = JsonSerializer.Deserialize<BoardPage>(boardJson);

            Application.Current.Dispatcher.Invoke(() =>
            {
                MatchBoardPage.ReloadPage(auxBoard);
            });
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
            BoardPageCallbackDto page = new BoardPageCallbackDto(
                boardDto, PlayersUsername.ToArray(), );
            string pageJson = HostLogic.CreateCallbackBoardPageJSON(page);
            client.SendOnTurnFinishedCallback(pageJson, PlayersUsername.ToArray());
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

        private static BoardPage BoardDtoToBoardPage(BoardPageDto board)
        {
            var boardControl = new UserControlBoard();
            boardControl.SetAllTiles(board.Tiles);

            return new BoardPage
            {
                ActionsRemain = board.ActionsRemain,
                TreasuresCaptured = board.TreasureCaptured,
                WaterLevelCount = board.WaterLevelCount,

                TreasureStack = board.TreasureStack,
                TreasureDiscardStack = board.TreasureDiscardStack,
                FloodStack = board.FloodStack,
                FloodDiscardStack = board.FloodDiscardStack,

                BoardControl = boardControl
            };
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
