using Forbbiden.Client.view.games;

namespace Forbbiden.Client.logic
{
    public class CallbackBoardPage
    {
        public BoardPage BOARD_PAGE { get; }
        public string[] PLAYERS_USERNAME { get; }

        public CallbackBoardPage(BoardPage boardPage, string[] usernames)
        {
            BOARD_PAGE = boardPage;
            PLAYERS_USERNAME = usernames;
        }
    }
}
