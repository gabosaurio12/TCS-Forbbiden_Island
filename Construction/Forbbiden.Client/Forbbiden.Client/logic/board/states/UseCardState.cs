using Forbbiden.Client.BoardManager;
using System.Windows;

namespace Forbbiden.Client.logic.board.states
{
    internal class UseCardState : IBoardState
    {
        private readonly BoardStateContext Context;

        public UseCardState(BoardStateContext context)
        {
            Context = context;
        }

        public void Enter()
        {
        }

        public void OnCardClicked(Card card)
        {
            switch (card.Name)
            {
                case "mitigation-name":
                    Context.SetState(new MitigationState(Context));
                    break;
                case "escape-q-name":
                    if (Context.Board.TreasuresCaptured == Context.Board.BoardControl.NumberOfTreasures)
                    {
                        if (Context.Board.CurrentTile.IsEscapeTile)
                        {
                            Context.Board.NotifyWin();
                        }
                        else
                        {
                            string title = Properties.Resources.not_escape_tile_title;
                            string message = Properties.Resources.not_escape_tile;
                            ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(Context.Board));
                        }
                    }
                    else
                    {
                        string title = Properties.Resources.missing_treasures_title;
                        string message = Properties.Resources.missing_treasures;
                        ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(Context.Board));
                    }

                    Exit();
                    break;
            }
        }

        public void OnCaptureTreasureClicked()
        {
            Context.SetState(new CaptureTreasureState(Context));
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

        public void Exit()
        {
            Context.SetState(new NormalState(Context));
        }

        public void OnUseTreasureCardClicked()
        {
            Context.SetState(new CaptureTreasureState(Context));
        }

        public void OnTileClicked(TileClickedEventArgs tile)
        {
        }
    }
}
