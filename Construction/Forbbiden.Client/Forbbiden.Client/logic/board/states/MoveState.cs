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
                    Context.Board.BoardControl);

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

            Exit();
        }

        public void OnShoreClicked()
        {
            Context.SetState(new ShoreState(Context));
        }

        public void OnCaptureTreasureClicked()
        {
            Context.SetState(new CaptureTreasureState(Context));
        }

        public void OnUseTreasureCardClicked()
        {
            Context.SetState(new UseCardState(Context));
        }

        public void Exit()
        {
            Context.Board.ResetTiles();
            Context.SetState(new NormalState(Context));
        }

        public void OnEndTurnClicked()
        {
            Context.Board.EndTurn();
            Exit();
        }

        public void OnCardClicked(Card card)
        {
        }

        public void OnMoveClicked()
        {
        }
    }
}
