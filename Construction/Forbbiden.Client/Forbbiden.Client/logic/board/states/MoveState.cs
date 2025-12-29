using Forbbiden.Client.BoardManager;

namespace Forbbiden.Client.logic.board.states
{
    public class MoveState : IBoardState
    {
        private readonly BoardStateContext Context;

        public MoveState(BoardStateContext context)
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
            Context.Board.RefreshAvatarTile(tile);
            Context.Board.EndAction();
            Context.SetState(new NormalState(Context));
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

        public void OnCaptureTreasureClicked()
        {
        }
    }
}
