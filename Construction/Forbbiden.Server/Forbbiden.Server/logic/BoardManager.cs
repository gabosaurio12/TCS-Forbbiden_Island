using Forbbiden.Contracts;
using Forbbiden.Server.callbacks;
using Forbbiden.Server.exceptionHandlers;
using Forbbiden.Server.Model;
using Forbbiden.Server.utils;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.ServiceModel;

namespace Forbbiden.Server.logic
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class BoardManager : IBoardManager
    {
        private readonly string ConnectionString;

        public BoardManager()
        {
            ConnectionString = ConnectionStringSingleton.GetInstance().ConnectionString;
        }

        public bool CreateBoard(int matchId)
        {
            bool success = false;
            if (matchId < 1)
                return success;

            Contracts.Board contractBoard = new Contracts.Board();
            using (var db = new Forbidden_FEIEntities(ConnectionString))
            {
                Model.Board modelBoard = new Model.Board()
                {
                    match_id = matchId
                };

                try
                {
                    db.Board.Add(modelBoard);
                    db.SaveChanges();
                    success = true;
                }
                catch (EntityException ex)
                {
                    string classMethod = "BoardManager.CreateBoard";
                    ExceptionHandler.HandleEntityException(ex, classMethod);
                }
            }

            return success;
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

                        AddTilesToDatabase(db, modelTiles, boardTiles);

                        db.SaveChanges();

                        contractTiles = AssignTilesIDs(modelTiles, boardTiles);
                    }
                }
                catch (EntityException ex)
                {
                    string classMethod = "BoardManager.RegisterTiles";
                    ExceptionHandler.HandleEntityException(ex, classMethod);
                }
            }          

            return contractTiles;
        }

        private void AddTilesToDatabase(Forbidden_FEIEntities db, List<Model.Tile> modelTiles, List<Contracts.Tile> boardTiles)
        {
            foreach (var boardTile in boardTiles)
            {
                Model.Tile tile = GetModelTile(boardTile);
                modelTiles.Add(tile);
                db.Tile.Add(tile);
            }
        }

        private Model.Tile GetModelTile(Contracts.Tile contractsTile)
        {
            return new Model.Tile()
            {
                col = contractsTile.Column,
                row = contractsTile.Row,
                isTreasure = contractsTile.IsTreasure ? 1 : 0,
                isEscape = contractsTile.IsEscape ? 1 : 0,
                isFlood = contractsTile.IsFlood ? 1 : 0
            };
        }

        private List<Contracts.Tile> AssignTilesIDs(List<Model.Tile> modelTiles, List<Contracts.Tile> boardTiles)
        {
            List<Contracts.Tile> contractTiles = boardTiles.ToList();

            for (int i = 0; i < modelTiles.Count; i++)
            {
                contractTiles[i].TileId = modelTiles[i].tile_id;
            }
            return contractTiles;
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
                ExceptionHandler.HandleCommunicationException(ex, classMethod);
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
                ExceptionHandler.HandleCommunicationException(ex, classMethod);
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
                ExceptionHandler.HandleCommunicationException(ex, classMethod);
            }
            catch (TimeoutException ex)
            {
                ExceptionHandler.HandleTimeoutException(ex, classMethod);
            }
        }

        public void SendOnTurnFinishedCallback(string boardJson, List<string> usernames)
        {
            try
            {
                MatchNotificationManager.SendOnTurnFinishedCallback(boardJson, usernames);
            }
            catch (CommunicationException ex)
            {
                string classMethod = "BoardManger.SendOnBoardCreatedCallback";
                ExceptionHandler.HandleCommunicationException(ex, classMethod);
            }
            catch (TimeoutException ex)
            {
                string classMethod = "BoardManger.SendOnBoardCreatedCallback";
                ExceptionHandler.HandleTimeoutException(ex, classMethod);
            }
        }
    }
}
