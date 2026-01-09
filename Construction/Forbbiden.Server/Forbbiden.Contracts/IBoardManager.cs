using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Forbbiden.Contracts
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IBoardManager" in both code and config file together.
    [ServiceContract]
    public interface IBoardManager
    {
        [OperationContract]
        [FaultContract(typeof(Fault))]
        List<String> GetTileImages();

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
}
