using Forbbiden.Client.BoardManager;

namespace Forbbiden.Client.logic.board.states
{
    public class NormalState : IBoardState
    {
        private readonly BoardStateContext Context;

        public NormalState(BoardStateContext context)
        {
            Context = context;
        }

        public void Enter()
        {
        }

        public void OnEndTurnClicked()
        {
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

        public void OnCardClicked(Card card)
        {
            switch (card.Name)
            {
                case "mitigation-name":
                    break;
                case "escape-q-name":
                    break;
            }
        }

        public void OnCaptureTreasureClicked()
        {
        }

        public void OnTileClicked(TileClickedEventArgs tile)
        {
        }

        public void Exit()
        {
        }
    }
}
