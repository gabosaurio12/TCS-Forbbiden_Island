using Forbbiden.Client.BoardManager;

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
                var tiles = MatchLogic.GetPossibleTilesToShore(
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
            var shoreTile = Context.Board.BoardControl.GetTile(tile.Row, tile.Column);
            shoreTile.IsFlood = false;
            shoreTile.ResetBorder();

            Context.Board.EndAction();

            Exit();
        }

        public void OnCaptureTreasureClicked()
        {
            Context.SetState(new CaptureTreasureState(Context));
        }

        public void OnEndTurnClicked()
        {
            Context.Board.ResetTiles();
            Context.Board.EndTurn();
            PlayerLogic.SendTurnFinishedCallback(Context.Board);
            Context.SetState(new StandByState(Context));
        }

        public void OnMoveClicked()
        {
            Context.SetState(new MoveState(Context));
        }

        public void OnShoreClicked()
        {
            Context.SetState(new ShoreState(Context));
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

        public void OnCardClicked(Card card)
        {
        }
    }
}
