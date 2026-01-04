using Forbbiden.Client.BoardManager;

namespace Forbbiden.Client.logic.board.states
{
    public class DiscardCardState : IBoardState
    {
        private readonly BoardStateContext Context;
        private readonly Card PendingCard;

        public DiscardCardState(BoardStateContext context, Card pendingCard)
        {
            Context = context;
            PendingCard = pendingCard;
        }

        public void Enter()
        {
        }

        public void OnCardClicked(Card card)
        {
            Context.Board.DiscardCardFromHand(card);
            Context.Board.AddCardToHand(PendingCard);
            Context.SetState(new NormalState(Context));
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
            Context.SetState(new NormalState(Context));
        }

        public void OnEndTurnClicked()
        {
            Context.Board.EndTurn();
            PlayerLogic.SendTurnFinishedCallback(Context.Board);
            Context.SetState(new StandByState(Context));
        }

        public void OnCaptureTreasureClicked()
        {
        }

        public void OnTileClicked(TileClickedEventArgs tile)
        {
        }
    }
}
