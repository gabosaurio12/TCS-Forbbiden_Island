using Forbbiden.Client.BoardManager;
using System.Collections.Generic;

namespace Forbbiden.Client.Model
{
    public class BoardPageDto
    {
        public int TreasureCaptured { get; set; }
        public int WaterLevelCount { get; set; }

        public List<Card> TreasureStack { get; set; }
        public List<Card> TreasureDiscardStack { get; set; }
        public List<Card> FloodStack { get; set; }
        public List<Card> FloodDiscardStack { get; set; }
    }
}
