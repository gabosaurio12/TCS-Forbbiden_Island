using Forbbiden.Client.Logic.Board.States;
using Forbbiden.Client.View.Games;

namespace Forbbiden.Client.Logic.Board
{
    public class BoardStateContext
    {
        public IBoardState CurrentState { get; private set; }
        public BoardPage Board { get;}

        public BoardStateContext(BoardPage boardPage)
        {
            Board = boardPage;
            SetState(new NormalState(this));
        }

        public void SetState(IBoardState newState)
        {
            CurrentState = newState;
            newState.Enter();
        }

        public void OnCaptureTreasureClicked()
        {
            SetState(new CaptureTreasureState(this));
            CurrentState.OnCaptureTreasureClicked();
        }

        public void EnterEmergencyMoveState()
        {
            SetState(new EmergencyMoveState(this));
        }

        public void EnterNormalState()
        {
            SetState(new NormalState(this));
        }

        public void EndTurnAndResetTiles()
        {
            Board.ResetTiles();
            Board.EndTurn();
            PlayerLogic.SendTurnFinishedCallback();
            SetState(new StandByState(this));
        }
    }
}
