using Forbbiden.Client.BoardManager;

namespace Forbbiden.Client.Logic.Board
{
    public interface IBoardState
    {
        void Enter();
        void OnTileClicked(TileClickedEventArgs tile);
        void OnCardClicked(Card card);
        void OnMoveClicked();
        void OnShoreClicked();
        void OnEndTurnClicked();
        void OnCaptureTreasureClicked();
        void OnUseTreasureCardClicked();
        void Exit();
    }
}
