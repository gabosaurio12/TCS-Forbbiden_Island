using Forbbiden.Client.logic.board.states;
using Forbbiden.Client.view.games;

namespace Forbbiden.Client.logic.board
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
    }
}
