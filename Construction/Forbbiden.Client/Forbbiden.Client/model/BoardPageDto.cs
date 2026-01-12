using System.Collections.Generic;

namespace Forbbiden.Client.Model
{
    public class BoardPageDto
    {
        public int TreasureCaptured { get; set; }
        public int WaterLevelCount { get; set; }

        public List<CardDto> TreasureStack { get; set; }
        public List<CardDto> TreasureDiscardStack { get; set; }
        public List<CardDto> FloodStack { get; set; }
        public List<CardDto> FloodDiscardStack { get; set; }
    }
}
