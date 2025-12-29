using Forbbiden.Client.BoardManager;
using System;
using System.Collections.Generic;

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
        }

        public void OnMoveClicked()
        {
            Context.SetState(new MoveState(Context));
        }

        public void OnShoreClicked()
        {
            Context.SetState(new ShoreState(Context));
        }

        public void OnCaptureTreasureClicked()
        {
            var board = Context.Board;
            var treasureName = board.CurrentTile.Name;

            var counters = new Dictionary<string, Func<int>>
            {
                ["clean-code-name"] = () => board.CleanCodeCounter,
                ["cubicle-keys-name"] = () => board.CubicleKeysCounter,
                ["lucio-name"] = () => board.LucioCounter,
                ["parking-card-name"] = () => board.ParkingCardCounter
            };

            if (counters.TryGetValue(treasureName, out var counter)
                && counter() == 2)
            {
                board.CaptureTreasure(board.CurrentTile.TreasureCard);
            }
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

        public void OnTileClicked(TileClickedEventArgs tile)
        {
        }
    }
}
