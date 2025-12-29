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
            // TO-DO Resaltar cartas
        }

        public void OnCardClicked(Card card)
        {
            Context.Board.DiscardCardFromHand(card);
            Context.Board.AddCardToHand(PendingCard);
            Context.SetState(new NormalState(Context));
            Context.Board.ContinueTreasureDraw();
        }

        public void OnCaptureTreasureClicked()
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

        public void OnTileClicked(TileClickedEventArgs tile)
        {
        }
    }
}
