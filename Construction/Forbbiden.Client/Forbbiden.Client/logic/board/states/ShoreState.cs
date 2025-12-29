using Forbbiden.Client.BoardManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forbbiden.Client.logic.board.states
{
    public class ShoreState : IBoardState
    {
        private readonly BoardStateContext Context;

        public ShoreState(BoardStateContext context)
        {
            Context = context;
        }

        public void Enter()
        {
            if (Context.Board.ActionsRemain > 0)
            {
                var tiles = MatchLogic.GetPossibleTilesToMove(
                    Context.Board.CurrentTile,
                    Context.Board.Board);

                Context.Board.ShowInteractiveTiles(tiles);
            }
            else
            {
                Context.Board.NotifyNoActionsRemain();
            }
        }

        public void OnTileClicked(TileClickedEventArgs tile)
        {
            Context.Board.ResetTiles();

            var shoreTile = Context.Board.Board.GetTile(tile.Row, tile.Column);
            shoreTile.IsFlood = false;
            shoreTile.ResetBorderToDefault();

            Context.Board.EndAction();
            Context.SetState(new NormalState(Context));
        }

        public void OnCaptureTreasureClicked()
        {
        }

        public void OnCardClicked(Card card)
        {
        }

        public void OnEndTurnClicked()
        {
        }

        public void OnMoveClicked()
        {
        }

        public void OnShoreClicked()
        {
        }
    }
}
