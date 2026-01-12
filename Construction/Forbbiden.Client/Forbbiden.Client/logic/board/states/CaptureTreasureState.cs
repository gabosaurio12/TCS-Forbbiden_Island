using Forbbiden.Client.BoardManager;
using System;
using System.Collections.Generic;

namespace Forbbiden.Client.Logic.Board.States
{
    internal class CaptureTreasureState : IBoardState
    {
        private readonly BoardStateContext Context;

        public CaptureTreasureState(BoardStateContext context)
        {
            Context = context;
        }

        public void Enter()
        {
            if (Context.Board.ActionsRemain == 0)
            {
                Context.Board.NotifyNoActionsRemain();
                Context.SetState(new NormalState(Context));
            }
        }

        public void Exit()
        {
            Context.SetState(new NormalState(Context));
        }

        public void OnCaptureTreasureClicked()
        {
            var board = Context.Board;
            var treasureName = board.CurrentTile.TreasureCard.Name;

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

            Exit();
        }

        public void OnEndTurnClicked()
        {
            Context.Board.EndTurn();
            PlayerLogic.SendTurnFinishedCallback();
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

        public void OnTileClicked(TileClickedEventArgs tile)
        {
        }

        public void OnCardClicked(Card card)
        {
        }
    }
}
