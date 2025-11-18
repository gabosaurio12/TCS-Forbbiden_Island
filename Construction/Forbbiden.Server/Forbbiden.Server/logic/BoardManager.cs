using Forbbiden.Contracts;
using Forbbiden.Server.utils;
using log4net;
using System;
using System.Collections.Generic;
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
            return new List<Contracts.Card>();
        }

        public List<string> GetTileImages()
        {
            return new List<string>();
        }

        public List<Contracts.Card> GetTreasureCards()
        {
            return new List<Contracts.Card>();
        }
    }
}
