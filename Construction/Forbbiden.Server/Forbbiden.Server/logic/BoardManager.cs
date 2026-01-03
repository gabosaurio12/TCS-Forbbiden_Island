using Forbbiden.Contracts;
using Forbbiden.Server.callbacks;
using Forbbiden.Server.exceptionHandlers;
using Forbbiden.Server.utils;
using log4net;
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
        private const string CLASS_METHOD = "BoardManger.SendOnPlayersTurnCallback";

        public BoardManager()
        {
            ConnectionString = ConnectionStringSingleton.GetInstance().connectionString;
        }

        public Contracts.Card GetCard(string path)
        {
            using (var db = new Forbbiden_FEIEntities(ConnectionString))
            {
                Card card = null;
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
            using (var db = new Forbbiden_FEIEntities(ConnectionString))
            {
                List<Card> cards = null;
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

        public List<string> GetTileImages()
        {
            return new List<string>();
        }

        public List<Contracts.Card> GetTreasureCards()
        {
            using (var db = new Forbbiden_FEIEntities(ConnectionString))
            {
                List<Card> cards = null;
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

        public void SendOnBoardCreatedCallback(string boardJson, List<string> usernames)
        {
            try
            {
                MatchNotificationManager.SendOnBoardCreatedCallback(boardJson, usernames);
            }
            catch (CommunicationException ex)
            {
                ExceptionHandler.HandleCommunicationException(ex, CLASS_METHOD);
            }
            catch (TimeoutException ex)
            {
                ExceptionHandler.HandleTimeoutException(ex, CLASS_METHOD);
            }
        }

        public void SendOnBoardUpdatedCallback(string boardJson, List<string> usernames)
        {
            try
            {
                MatchNotificationManager.SendOnBoardUpdatedCallback(boardJson, usernames);
            }
            catch (CommunicationException ex)
            {
                ExceptionHandler.HandleCommunicationException(ex, CLASS_METHOD);
            }
            catch (TimeoutException ex)
            {
                ExceptionHandler.HandleTimeoutException(ex, CLASS_METHOD);
            }
        }

        public void SendOnPlayersTurnCallback(string username)
        {
            try
            {
                MatchNotificationManager.SendOnPlayersTurnCallback(username);
            }
            catch (CommunicationException ex)
            {
                ExceptionHandler.HandleCommunicationException(ex, CLASS_METHOD);
            }
            catch (TimeoutException ex)
            {
                string classMethod = "BoardManger.SendOnPlayersTurnCallback";
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
