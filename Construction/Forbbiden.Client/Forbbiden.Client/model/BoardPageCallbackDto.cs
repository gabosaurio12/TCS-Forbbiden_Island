
namespace Forbbiden.Client.Model
{
    public class BoardPageCallbackDto
    {
        public BoardPageDto Board { get; set; }

        public int MatchId { get; set; }

        public string[] PlayersUsernames { get; set; }
    }
}