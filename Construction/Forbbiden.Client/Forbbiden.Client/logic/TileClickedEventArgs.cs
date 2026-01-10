using System;

namespace Forbbiden.Client.Logic
{
    public class TileClickedEventArgs : EventArgs
    {
        public int Row { get; }
        public int Column { get; }

        public TileClickedEventArgs(int row, int column)
        {
            Row = row;
            Column = column;
        }
    }
}
