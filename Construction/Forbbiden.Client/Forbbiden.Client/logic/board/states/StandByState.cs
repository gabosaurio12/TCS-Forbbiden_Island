using Forbbiden.Client.BoardManager;
using System.Windows;

namespace Forbbiden.Client.Logic.Board.States
{
    public class StandByState : IBoardState
    {
        private readonly BoardStateContext Context;

        public StandByState(BoardStateContext context)
        {
            Context = context;
        }

        public void Enter()
        {
        }

        public void Exit()
        {
        }

        private void OpenNotYourTurnNotification()
        {
            string title = Properties.Resources.not_your_turn_title;
            string message = Properties.Resources.not_your_turn_message;
            ViewUtils.OpenNotificationWindow(title, message, Window.GetWindow(Context.Board));
        }
        public void OnCaptureTreasureClicked()
        {
            OpenNotYourTurnNotification();
        }

        public void OnCardClicked(Card card)
        {
            OpenNotYourTurnNotification();
        }

        public void OnEndTurnClicked()
        {
            OpenNotYourTurnNotification();
        }

        public void OnMoveClicked()
        {
            OpenNotYourTurnNotification();
        }

        public void OnShoreClicked()
        {
            OpenNotYourTurnNotification();
        }

        public void OnTileClicked(TileClickedEventArgs tile)
        {
            OpenNotYourTurnNotification();
        }

        public void OnUseTreasureCardClicked()
        {
            OpenNotYourTurnNotification();
        }
    }
}
