using Forbbiden.Client.Model;

namespace Forbbiden.Client.Model
{
    public class BoardPageCallbackDto
    {
        public BoardPageDto Board { get; set; }

        public int MatchId { get; set; }

        public string[] PlayersUsernames { get; set; }

        public BoardPageCallbackDto(BoardPageDto boardPage, int matchId, string[] usernames)
        {
            Board = boardPage;
            MatchId = matchId;
            PlayersUsernames = usernames;
        }
    }
}