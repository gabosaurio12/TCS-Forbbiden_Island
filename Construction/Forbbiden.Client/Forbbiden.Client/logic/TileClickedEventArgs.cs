using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forbbiden.Client.logic
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
