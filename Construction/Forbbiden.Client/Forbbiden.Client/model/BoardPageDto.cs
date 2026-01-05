using Forbbiden.Client.BoardManager;
using System.Collections.Generic;

namespace Forbbiden.Client.model
{
    public class BoardPageDto
    {
        public int ActionsRemain { get; set; }
        public int TreasureCaptured { get; set; }
        public int WaterLevelCount { get; set; }

        public List<Card> TreasureStack { get; set; }
        public List<Card> TreasureDiscardStack { get; set; }
        public List<Card> FloodStack { get; set; }
        public List<Card> FloodDiscardStack { get; set; }

        public List<TileDto> Tiles { get; set; }
    }
}
