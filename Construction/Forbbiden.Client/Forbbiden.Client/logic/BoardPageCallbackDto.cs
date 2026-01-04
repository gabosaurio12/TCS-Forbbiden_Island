using Forbbiden.Client.view.games;

namespace Forbbiden.Client.logic
{
    public class BoardPageCallbackDto
    {
        public BoardPage Board { get; }
        public string[] PlayersUsernames { get; }

        public BoardPageCallbackDto(BoardPage boardPage, string[] usernames)
        {
            Board = boardPage;
            PlayersUsernames = usernames;
        }
    }
}
