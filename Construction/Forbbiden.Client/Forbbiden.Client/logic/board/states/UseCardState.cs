using Forbbiden.Client.BoardManager;
using System;
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
            throw new NotImplementedException();
        }

        public void OnCardClicked(Card card)
        {
            switch (card.Name)
            {
                case "mitigation-name":
                    Context.SetState(new MitigationState(Context));
                    break;
                case "escape-q-name":
                    if (Context.Board.TreasuresCaptured == 4)
                    {
                        if (Context.Board.CurrentTile.IsEscapeTile)
                        {
                            Context.Board.NotifyWin();
                        }
                        else
                        {
                            string title = Properties.Langs.Resources.not_escape_tile_title;
                            string message = Properties.Langs.Resources.not_escape_tile;
                            ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(Context.Board));
                        }
                    }
                    else
                    {
                        string title = Properties.Langs.Resources.missing_treasures_title;
                        string message = Properties.Langs.Resources.missing_treasures;
                        ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(Context.Board));
                    }
                    break;
            }
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
