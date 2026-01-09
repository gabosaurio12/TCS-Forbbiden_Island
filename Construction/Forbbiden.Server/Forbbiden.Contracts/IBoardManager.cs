using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Forbbiden.Contracts
{
    [ServiceContract]
    public interface IBoardManager
    {
        [OperationContract]
        [FaultContract(typeof(Fault))]
        bool CreateBoard(int matchId);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        List<Tile> RegisterBoardTiles(List<Tile> boardTiles);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        List<Card> GetTreasureCards();

        [OperationContract]
        [FaultContract(typeof(Fault))]
        List<Card> GetFloodCards();

        [OperationContract]
        [FaultContract(typeof(Fault))]
        Card GetCard(string path);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        void SendOnBoardCreatedCallback(string boardJson, List<string> usernames);
        
        [OperationContract]
        [FaultContract(typeof(Fault))]
        void SendOnBoardUpdatedCallback(string boardJson, List<string> usernames);
        
        [OperationContract]
        [FaultContract(typeof(Fault))]
        void SendOnPlayersTurnCallback(string username);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        void SendOnTurnFinishedCallback(string boardJson, List<string> usernames);

        
    }

    [DataContract]
    public class Treasure
    {
        [DataMember]
        public int TreasureId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }
    }

    [DataContract]
    public class Card
    {
        [DataMember]
        public int CardId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string ImagePath { get; set; }
    }

    [DataContract]
    public class Board
    {
        [DataMember]
        public int BoardId { get; set; }

        [DataMember]
        public int MatchId { get; set; }

        [DataMember]
        public List<Tile> Tiles { get; set; }
    }

    [DataContract]
    public class Tile
    {
        [DataMember]
        public int TileId { get; set; }

        [DataMember]
        public int Column { get; set; }

        [DataMember]
        public int Row { get; set; }

        [DataMember]
        public bool IsFlood { get; set; }

        [DataMember]
        public bool IsTreasure { get; set; }

        [DataMember]
        public bool IsEscape { get; set; }
    }
}
