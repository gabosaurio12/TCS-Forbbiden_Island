using Forbbiden.Client.model;

namespace Forbbiden.Client.logic
{
    public class BoardPageCallbackDto
    {
        public BoardPageDto Board { get; set; }
        public string[] PlayersUsernames { get; set; }

        public BoardPageCallbackDto(BoardPageDto boardPage, string[] usernames)
        {
            Board = boardPage;
            PlayersUsernames = usernames;
        }
    }
}