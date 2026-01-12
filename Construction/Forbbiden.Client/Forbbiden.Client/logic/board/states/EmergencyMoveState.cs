using Forbbiden.Client.BoardManager;
using System.Windows;

namespace Forbbiden.Client.Logic.Board.States
{
    internal class EmergencyMoveState : IBoardState
    {
        private readonly BoardStateContext Context;

        public EmergencyMoveState(BoardStateContext context)
        {
            Context = context;
        }

        public void Enter()
        {
            var tiles = MatchLogic.GetPossibleTilesToMove(
                    Context.Board.CurrentTile,
                    Context.Board.BoardControl);

            if (tiles.Count == 0)
            {
                string title = Properties.Resources.game_over;
                string message = Properties.Resources.you_drowned_message;
                ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(Context.Board));
                Exit();
            }
            else
            {
                Context.Board.ShowInteractiveTiles(tiles);
            }
        }

        public void OnTileClicked(TileClickedEventArgs tile)
        {
            Context.Board.RefreshAvatarTile(tile);
            Context.Board.EndAction();

            Exit();
        }

        public void Exit()
        {
            Context.Board.ResetTiles();
            Context.SetState(new NormalState(Context));
        }

        public void OnCaptureTreasureClicked()
        {
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

        public void OnUseTreasureCardClicked()
        {
        }
    }
}
