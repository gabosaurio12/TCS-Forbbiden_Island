using Forbbiden.Contracts;
using Forbbiden.Server.callbacks;
using Forbbiden.Server.exceptionHandlers;
using Forbbiden.Server.Model;
using Forbbiden.Server.utils;
using Forbbiden.Server.Utils;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.ServiceModel;

namespace Forbbiden.Server.logic
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class BoardManager : IBoardManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BoardManager));
        private readonly string ConnectionString;

        public BoardManager()
        {
            ConnectionString = ConnectionStringSingleton.GetInstance().ConnectionString;
        }

        public bool CreateBoard(List<Contracts.Tile> tiles, int matchId)
        {
            bool success = false;
            if (tiles?.Count > 0 && matchId > 0)
            {
                try
                {
                    using (var db = new Forbidden_FEIEntities(ConnectionString))
                    {
                        foreach (var tile in tiles)
                        {
                            Model.Board modelBoard = new Model.Board()
                            {
                                match_id = matchId,
                                tile_id = tile.TileId
                            };

                            db.Board.Add(modelBoard);
                        }
                        
                        db.SaveChanges();
                        success = true;
                    }
                }
                catch (EntityException ex)
                {
                    string classMethod = "BoardManager.CreateBoard";
                    ExceptionHandler.HandleEntityException(ex, classMethod);
                }
            }

            return success;
        }

        public Contracts.Board GetBoard(int matchId)
        {
            Contracts.Board board = new Contracts.Board();
            if (matchId > 0)
            {
                try
                {
                    using (var db = new Forbidden_FEIEntities(ConnectionString))
                    {
                        var boardTiles = db.Board.Where(b => b.match_id == matchId).ToList();
                        board.Tiles = BoardUtils.GetContractsTilesFromBoard(boardTiles, db);
                    }
                }
                catch (EntityException ex)
                {
                    string classMethod = "BoardManager.GetBoard";
                    ExceptionHandler.HandleEntityException(ex, classMethod);
                }
            }
            return board;
        }

        public List<Contracts.Tile> RegisterBoardTiles(List<Contracts.Tile> boardTiles)
        {
            List<Contracts.Tile> contractTiles = new List<Contracts.Tile>();
            if (boardTiles != null && boardTiles.Any())
            {
                try
                {
                    using (var db = new Forbidden_FEIEntities(ConnectionString))
                    {
                        var modelTiles = new List<Model.Tile>();

                        BoardUtils.AddTilesToDatabase(db, modelTiles, boardTiles);

                        db.SaveChanges();

                        contractTiles = BoardUtils.AssignTilesIDs(modelTiles, boardTiles);
                    }
                }
                catch (EntityException ex)
                {
                    string classMethod = "BoardManager.RegisterBoardTiles";
                    ExceptionHandler.HandleEntityException(ex, classMethod);
                }
            }

            return contractTiles;
        }

        public List<Contracts.Tile> GetBoardTiles(int matchId)
        {
            using (var db = new Forbidden_FEIEntities(ConnectionString))
            {
                List<Contracts.Tile> tiles = new List<Contracts.Tile>();
                List<Model.Board> board = null;
                try
                {
                    board = db.Board.Where(b => b.match_id == matchId).ToList();
                    tiles = BoardUtils.GetContractsTilesFromBoard(board, db);
                }
                catch (EntityException ex)
                {
                    string classMethod = "BoardManager.GetBoardTiles";
                    ExceptionHandler.HandleEntityException(ex, classMethod);
                }

                return tiles;
            }
        }

        public Contracts.Card GetCard(string path)
        {
            using (var db = new Forbidden_FEIEntities(ConnectionString))
            {
                Model.Card card = null;
                try
                {
                    card = db.Card.FirstOrDefault(c => c.card_image_path == path);
                }
                catch (EntityException ex)
                {
                    string classMethod = "BoardManager.GetCard";
                    ExceptionHandler.HandleEntityException(ex, classMethod);
                }

                Contracts.Card contractCard;

                if (card != null)
                {
                    contractCard = new Contracts.Card
                    {
                        CardId = card.card_id,
                        Name = card.card_name,
                        Description = card.description,
                        Type = card.type,
                        ImagePath = card.card_image_path
                    };
                }
                else
                {
                    contractCard = new Contracts.Card
                    {
                        CardId = -1
                    };
                }

                return contractCard;
            }
        }

        public Contracts.Card GetCardById(int cardId)
        {
            using (var db = new Forbidden_FEIEntities(ConnectionString))
            {
                Model.Card modelCard = null;
                try
                {
                    modelCard = db.Card.FirstOrDefault(c => c.card_id == cardId);
                }
                catch (EntityException ex)
                {
                    string classMethod = "BoardManager.GetCard";
                    ExceptionHandler.HandleEntityException(ex, classMethod);
                }

                Contracts.Card contractCard;

                if (modelCard != null)
                {
                    contractCard = new Contracts.Card
                    {
                        CardId = modelCard.card_id,
                        Name = modelCard.card_name,
                        Description = modelCard.description,
                        Type = modelCard.type,
                        ImagePath = modelCard.card_image_path
                    };
                }
                else
                {
                    contractCard = new Contracts.Card
                    {
                        CardId = -1
                    };
                }

                return contractCard;
            }
        }

        public List<Contracts.Card> GetFloodCards()
        {
            using (var db = new Forbidden_FEIEntities(ConnectionString))
            {
                List<Model.Card> cards = null;
                try
                {
                    cards = db.Card.Where(c => c.type == "flood").ToList();
                }
                catch (EntityException ex)
                {
                    string classMethod = "BoardManager.GetFloodCards";
                    ExceptionHandler.HandleEntityException(ex, classMethod);
                }

                List<Contracts.Card> floodCards = null;
                if (cards != null)
                {
                    floodCards = new List<Contracts.Card>();
                    foreach (var card in cards)
                    {
                        floodCards.Add(new Contracts.Card
                        {
                            CardId = card.card_id,
                            Name = card.card_name,
                            Description = card.description,
                            Type = card.type,
                            ImagePath = card.card_image_path
                        });
                    }
                }

                return floodCards;
            }
        }

        public List<Contracts.Card> GetTreasureCards()
        {
            using (var db = new Forbidden_FEIEntities(ConnectionString))
            {
                List<Model.Card> cards = null;
                try
                {
                    cards = db.Card.Where(c => c.type == "treasure").ToList();
                }
                catch (EntityException ex)
                {
                    string classMethod = "BoardManager.GetTreasureCards";
                    ExceptionHandler.HandleEntityException(ex, classMethod);
                }

                List<Contracts.Card> treasureCards = null;
                if (cards != null)
                {
                    treasureCards = new List<Contracts.Card>();
                    foreach (var card in cards)
                    {
                        treasureCards.Add(new Contracts.Card
                        {
                            CardId = card.card_id,
                            Name = card.card_name,
                            Description = card.description,
                            Type = card.type,
                            ImagePath = card.card_image_path
                        });
                    }
                }


                return treasureCards;
            }
        }

        public PlayerCoordinates GetPlayerCoordinates(int matchId, string username)
        {
            PlayerCoordinates playerCoordinates = new PlayerCoordinates()
            {
                PlayerId = -1
            };

            try
            {
                using (var db = new Forbidden_FEIEntities(ConnectionString))
                {
                    var coordinates = db.MatchPlayers
                        .Where(mp => mp.match_id == matchId
                            && mp.Player.player_username == username)
                        .Select(mp => new PlayerCoordinates
                        {
                            PlayerId = mp.player_id,
                            MatchId = mp.match_id,
                            Username = username,
                            Col = mp.col ?? -1,
                            Row = mp.row ?? -1
                        })
                        .FirstOrDefault();

                    if (coordinates != null)
                    {
                        playerCoordinates = coordinates;
                    }
                }
            }
            catch (EntityException ex)
            {
                string classMethod = "BoardManager.GetPlayerCoordinates";
                ExceptionHandler.HandleEntityException(ex, classMethod);
            }
            return playerCoordinates;
        }

        public bool UpdatePlayerCoordinates(PlayerCoordinates playerCoordinates)
        {
            string classMethod = "BoardManager.UpdatePlayerCoordinates";
            bool coordinatesUpdated = false;
            try
            {
                using (var db = new Forbidden_FEIEntities(ConnectionString))
                {
                    var currentCoordinates = db.MatchPlayers.FirstOrDefault(
                        mp => mp.match_id == playerCoordinates.MatchId
                        && mp.player_id == playerCoordinates.PlayerId);

                    if (currentCoordinates != null)
                    {
                        currentCoordinates.row = playerCoordinates.Row;
                        currentCoordinates.col = playerCoordinates.Col;

                        db.SaveChanges();
                        coordinatesUpdated = true;
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                ExceptionHandler.HandleDbUpdateException(ex, classMethod);
            }
            catch (EntityException ex)
            {
                ExceptionHandler.HandleEntityException(ex, classMethod);
            }
            return coordinatesUpdated;
        }

        public void SendOnBoardCreatedCallback(string boardJson, List<string> usernames)
        {
            string classMethod = "BoardManger.SendOnBoardCreatedCallback";
            try
            {
                MatchNotificationManager.SendOnBoardCreatedCallback(boardJson, usernames);
            }
            catch (CommunicationException ex)
            {
                Log.Warn(classMethod, ex);
            }
            catch (TimeoutException ex)
            {
                ExceptionHandler.HandleTimeoutException(ex, classMethod);
            }
        }

        public void SendOnBoardUpdatedCallback(string boardJson, List<string> usernames)
        {
            string classMethod = "BoardManger.SendOnBoardUpdatedCallback";
            try
            {
                MatchNotificationManager.SendOnBoardUpdatedCallback(boardJson, usernames);
            }
            catch (CommunicationException ex)
            {
                Log.Warn(classMethod, ex);
            }
            catch (TimeoutException ex)
            {
                ExceptionHandler.HandleTimeoutException(ex, classMethod);
            }
        }

        public void SendOnPlayersTurnCallback(string username)
        {
            string classMethod = "BoardManger.SendOnPlayersTurnCallback";
            try
            {
                MatchNotificationManager.SendOnPlayersTurnCallback(username);
            }
            catch (CommunicationException ex)
            {
                Log.Warn(classMethod, ex);
            }
            catch (TimeoutException ex)
            {
                ExceptionHandler.HandleTimeoutException(ex, classMethod);
            }
        }

        public void SendOnTurnFinishedCallback(string boardJson, List<string> usernames)
        {
            string classMethod = "BoardManger.SendOnBoardCreatedCallback";
            try
            {
                MatchNotificationManager.SendOnTurnFinishedCallback(boardJson, usernames);
            }
            catch (CommunicationException ex)
            {
                Log.Warn(classMethod, ex);
            }
            catch (TimeoutException ex)
            {
                ExceptionHandler.HandleTimeoutException(ex, classMethod);
            }
        }
    }
}
