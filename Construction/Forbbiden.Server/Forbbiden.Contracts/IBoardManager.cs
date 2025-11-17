using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Forbbiden.Contracts
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IIBoardManager" in both code and config file together.
    [ServiceContract]
    public interface IBoardManager
    {
        [OperationContract]
        List<String> GetTileImages();

        [OperationContract]
        List<Card> GetCards();

        [OperationContract]
        List<Treasure> GetTreasures();

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
    }
}
