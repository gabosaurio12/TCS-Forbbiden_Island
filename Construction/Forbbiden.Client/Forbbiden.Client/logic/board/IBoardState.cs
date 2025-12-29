using Forbbiden.Client.BoardManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forbbiden.Client.logic.board
{
    public interface IBoardState
    {
        void Enter();
        void OnTileClicked(TileClickedEventArgs tile);
        void OnCardClicked(Card card);
        void OnMoveClicked();
        void OnShoreClicked();
        void OnEndTurnClicked();
        void OnCaptureTreasureClicked();
    }
}
