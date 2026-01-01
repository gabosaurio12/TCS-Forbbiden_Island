using Forbbiden.Contracts;
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

        private static readonly ILog Log = LogManager.GetLogger(typeof(BoardManager));
        private readonly string ConnectionString;

        public BoardManager()
        {
            ConnectionString = ConnectionStringSingleton.GetInstance().connectionString;
        }

        private void HandleEntityException(EntityException ex, string classMethod)
        {
            Log.Error(classMethod, ex);

            var fault = new DBFault
            {
                Error = "Database Error",
                Details = ex.Message
            };

            string entityError = "EntityException";

            throw new FaultException<DBFault>(fault,
                new FaultReason(entityError));
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
                    HandleEntityException(ex, classMethod);
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
                    HandleEntityException(ex, classMethod);
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
                    HandleEntityException(ex, classMethod);
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
    }
}
