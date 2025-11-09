using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Forbbiden.Contracts
{
    [ServiceContract]
    public interface IMatchManager
    {
        [OperationContract]
        int CreateMatch(CreateMatchRequest request);

        [OperationContract]
        bool JoinMatch(JoinMatchRequest request);

        [OperationContract]
        List<Match> ListMatches();

        [OperationContract]
        Match GetMatchById(int matchId);
    }

    [DataContract]
    public class Match
    {
        [DataMember]
        public int MatchId { get; set; }

        [DataMember]
        public string Difficulty { get; set; }

        [DataMember]
        public string Visibility { get; set; }

        [DataMember]
        public List<PlayerInfo> Players { get; set; } = new List<PlayerInfo>();
    }

    [DataContract]
    public class PlayerInfo
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public bool IsHost { get; set; }
    }

    [DataContract]
    public class CreateMatchRequest
    {
        [DataMember(IsRequired = true)]
        public string HostUsername { get; set; }

        [DataMember(IsRequired = true)]
        public string Difficulty { get; set; }

        [DataMember(IsRequired = true)]
        public string Visibility { get; set; }
    }

    [DataContract]
    public class JoinMatchRequest
    {
        [DataMember(IsRequired = true)]
        public int MatchId { get; set; }

        [DataMember(IsRequired = true)]
        public string Username { get; set; }
    }
}
