using Forbbiden.Client.BoardManager;
using Forbbiden.Client.view.games;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace Forbbiden.Client.logic
{
    public class PlayerLogic
    {
        private static BoardPage MatchBoardPage;
        private static List<string> PlayersUsername = new List<string>();

        protected PlayerLogic()
        {
            MatchNotificationsSingleton.Instance.OnBoardCreated += CreateBoardPageFromJSON;
            MatchNotificationsSingleton.Instance.OnBoardUpdated += RefreshBoardFromJSON;
            MatchNotificationsSingleton.Instance.OnPlayersTurn += OnTurnStarted;
            MatchNotificationsSingleton.Instance.OnTurnFinished += OnTurnFinishedCallbackReceived;
        }

        public static void CreateBoardPageFromJSON(string boardJson)
        {
            var auxBoard = JsonSerializer.Deserialize<BoardPageCallbackDto>(boardJson);
            MatchBoardPage = auxBoard.Board;
            PlayersUsername = auxBoard.PlayersUsernames.ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                MatchBoardPage.ReloadPage(auxBoard.Board);
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
            string title = Properties.Langs.Resources.your_turn_title;
            string message = Properties.Langs.Resources.your_turn_message;
            ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(MatchBoardPage));
        }

        public static void SendTurnFinishedCallback(BoardPage boardPage)
        {
            var client = new BoardManagerClient();
            BoardPageCallbackDto page = new BoardPageCallbackDto(boardPage, PlayersUsername.ToArray());
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
    }
}
