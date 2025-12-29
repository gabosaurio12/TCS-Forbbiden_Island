using Forbbiden.Contracts;
using Forbbiden.Server.utils;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace Forbbiden.Server.logic
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class BoardManager : IBoardManager
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(BoardManager));
        private readonly string connectionString;

        public BoardManager()
        {
            connectionString = ConnectionStringSingleton.GetInstance().connectionString;
        }

        public List<Contracts.Card> GetFloodCards()
        {
            using (var db = new Forbbiden_FEIEntities(connectionString))
            {
                List<Card> cards = db.Card.Where(c => c.type == "flood").ToList();
                List<Contracts.Card> floodCards = new List<Contracts.Card>();
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

                return floodCards;
            }
        }

        public List<string> GetTileImages()
        {
            return new List<string>();
        }

        public List<Contracts.Card> GetTreasureCards()
        {
            using (var db = new Forbbiden_FEIEntities(connectionString))
            {
                List<Card> cards = db.Card.Where(c => c.type == "treasure").ToList();
                
                List<Contracts.Card> treasureCards = new List<Contracts.Card>();
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

                return treasureCards;
            }
        }
    }
}
