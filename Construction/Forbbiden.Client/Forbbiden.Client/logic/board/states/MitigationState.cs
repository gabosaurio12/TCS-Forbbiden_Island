using Forbbiden.Client.BoardManager;
using Forbbiden.Client.Exceptions;
using Forbbiden.Client.ProfileManager;
using Forbbiden.Client.Repositories;
using log4net;
using System.ServiceModel;
using System.Windows;

namespace Forbbiden.Client.Logic.Board.States
{
    public class MitigationState : IBoardState
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MitigationState));
        private readonly BoardStateContext Context;

        public MitigationState(BoardStateContext context)
        {
            Context = context;
        }

        public void Enter()
        {
            var tiles = MatchLogic.GetPossibleTilesToMitigate(
                Context.Board.BoardControl);

            Context.Board.ShowInteractiveTiles(tiles);
        }

        private async void DiscardMitigationCard()
        {
            string path = "mitigation.png";
            Card card;
            try
            {
                card = await BoardRepository.GetCard(path);
            }
            catch (ViewException ex)
            {
                ExceptionViewManager.ShowViewExceptionNotification(
                    ex, Window.GetWindow(Context.Board));
                return;
            }

            Context.Board.DiscardCardFromHand(card);

        }

        public void OnTileClicked(TileClickedEventArgs tile)
        {
            ShoreState.ShoreTile(tile, Context);

            DiscardMitigationCard();
            Context.Board.EndAction();
            Exit();
        }

        public void OnCaptureTreasureClicked()
        {
            Context.SetState(new CaptureTreasureState(Context));
        }

        public void OnEndTurnClicked()
        {
            Context.EndTurnAndResetTiles();
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

        public void Exit()
        {
            Context.Board.ResetTiles();
            Context.SetState(new NormalState(Context));
        }

        public void OnCardClicked(Card card)
        {
        }
    }
}
