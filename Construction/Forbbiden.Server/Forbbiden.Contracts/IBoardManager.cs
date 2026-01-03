using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Forbbiden.Contracts
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IBoardManager" in both code and config file together.
    [ServiceContract]
    public interface IBoardManager
    {
        [OperationContract]
        List<String> GetTileImages();

        [OperationContract]
        List<Card> GetTreasureCards();

        [OperationContract]
        List<Card> GetFloodCards();

        [OperationContract]
        Card GetCard(string path);

        [OperationContract]
        void SendOnBoardCreatedCallback(string boardJson, List<string> usernames);
        [OperationContract]
        void SendOnBoardUpdatedCallback(string boardJson, List<string> usernames);
        [OperationContract]
        void SendOnPlayersTurnCallback(string username);

        [OperationContract]
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
    public class CallbackFault
    {
        [DataMember]
        public string Error { get; set; }
        [DataMember]
        public string Details { get; set; }
    }

    [DataContract]
    public class TimeoutFault
    {
        [DataMember]
        public string Error { get; set; }
        [DataMember]
        public string Details { get; set; }
    }
}
