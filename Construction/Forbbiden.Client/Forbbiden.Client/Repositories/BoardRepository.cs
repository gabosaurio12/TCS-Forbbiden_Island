using Forbbiden.Client.BoardManager;
using Forbbiden.Client.ErrorCodes;
using Forbbiden.Client.Exceptions;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Forbbiden.Client.Repositories
{
    public class BoardRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BoardRepository));
        private static readonly BoardManagerClient BoardClient = new BoardManagerClient();

        public static async Task<bool> CreateBoard(List<Tile> tiles, int matchId)
        {
            bool created;
            try
            {
                created = await BoardClient.CreateBoardAsync(tiles.ToArray(), matchId);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("BoardRepository.CreateBoard", ex);
                throw new ViewException(ServerErrorCodes.pushingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("BoardRepository.CreateBoard", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }
            return created;
        }

        public static async Task<Board> Getboard(int matchId)
        {
            Board board;
            try
            {
                board = await BoardClient.GetBoardAsync(matchId);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("BoardRepository.Getboard", ex);
                throw new ViewException(ServerErrorCodes.pullingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("BoardRepository.Getboard", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }
            return board;
        }

        public static async Task<List<Tile>> RegisterBoardTiles(List<Tile> boardTiles)
        {
            Tile[] registeredTiles;
            try
            {
                registeredTiles = await BoardClient.RegisterBoardTilesAsync(boardTiles.ToArray());
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("BoardRepository.RegisterBoardTiles", ex);
                throw new ViewException(ServerErrorCodes.pushingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("BoardRepository.RegisterBoardTiles", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }
            return registeredTiles.ToList();
        }

        public static async Task<List<Tile>> GetBoardTiles(int matchId)
        {
            Tile[] tiles;
            try
            {
                tiles = await BoardClient.GetBoardTilesAsync(matchId);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("BoardRepository.GetBoardTiles", ex);
                throw new ViewException(ServerErrorCodes.pullingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("BoardRepository.GetBoardTiles", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }
            return tiles.ToList();
        }

        public static async Task<List<Card>> GetTreasureCards()
        {
            Card[] cards;
            try
            {
                cards = await BoardClient.GetTreasureCardsAsync();
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("BoardRepository.GetTreasureCards", ex);
                throw new ViewException(ServerErrorCodes.pullingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("BoardRepository.GetTreasureCards", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }
            return cards.ToList();
        }

        public static async Task<List<Card>> GetFloodCards()
        {
            Card[] cards;
            try
            {
                cards = await BoardClient.GetFloodCardsAsync();
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("BoardRepository.GetFloodCards", ex);
                throw new ViewException(ServerErrorCodes.pullingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("BoardRepository.GetFloodCards", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }
            return cards.ToList();
        }

        public static async Task<Card> GetCard(string path)
        {
            Card card;
            try
            {
                card = await BoardClient.GetCardAsync(path);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("BoardRepository.GetCard", ex);
                throw new ViewException(ServerErrorCodes.pullingDataError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("BoardRepository.GetCard", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }
            return card;
        }

        public static async void SendOnBoardCreatedCallback(string boardJson, List<string> usernames)
        {
            try
            {
                await BoardClient.SendOnBoardCreatedCallbackAsync(boardJson, usernames.ToArray());
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("BoardRepository.SendOnBoardCreatedCallback", ex);
                throw new ViewException(ServerErrorCodes.sendingCallbackError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("BoardRepository.SendOnBoardCreatedCallback", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }
        }

        public static async void SendOnBoardUpdatedCallback(string boardJson, List<string> usernames)
        {
            try
            {
                await BoardClient.SendOnBoardUpdatedCallbackAsync(boardJson, usernames.ToArray());
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("BoardRepository.SendOnBoardUpdatedCallback", ex);
                throw new ViewException(ServerErrorCodes.sendingCallbackError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("BoardRepository.SendOnBoardUpdatedCallback", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }
        }

        public static async void SendOnPlayersTurnCallback(string username)
        {
            try
            {
                await BoardClient.SendOnPlayersTurnCallbackAsync(username);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("BoardRepository.SendOnPlayersTurnCallback", ex);
                throw new ViewException(ServerErrorCodes.sendingCallbackError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("BoardRepository.SendOnPlayersTurnCallback", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }
        }

        public static async void SendOnTurnFinishedCallback(string boardJson, List<string> usernames)
        {
            try
            {
                await BoardClient.SendOnTurnFinishedCallbackAsync(boardJson, usernames.ToArray());
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error("BoardRepository.SendOnTurnFinishedCallback", ex);
                throw new ViewException(ServerErrorCodes.sendingCallbackError);
            }
            catch (TimeoutException ex)
            {
                Log.Error("BoardRepository.SendOnTurnFinishedCallback", ex);
                throw new ViewException(ServerErrorCodes.timeoutError);
            }
        }
    }
}
