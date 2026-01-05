using Forbbiden.Client.BoardManager;

namespace Forbbiden.Client.model
{
    public class TileDto
    {
        public int Row { get; set; }
        public int Column { get; set; }

        public bool IsFlood { get; set; }
        public bool IsLost { get; set; }
        public bool IsTreasure { get; set; }
        public bool IsEscapeTile { get; set; }

        public string ImageFileName { get; set; }
        public Card TreasureCard { get; set; }
    }
}
