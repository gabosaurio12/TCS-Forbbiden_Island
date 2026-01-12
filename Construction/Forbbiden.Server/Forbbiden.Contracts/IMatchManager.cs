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
        [FaultContract(typeof(Fault))]
        int CreateMatch(CreateMatchRequest request);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        bool JoinMatch(JoinMatchRequest request);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        List<Match> ListMatches();

        [OperationContract]
        [FaultContract(typeof(Fault))]
        Match GetMatchById(int matchId);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        bool DeleteMatch(int matchId);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        string GetInviteCode(int matchId);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        bool ValidateInvite(int matchId, string inviteCode);
    }

    [DataContract]
    public class Match
    {
        [DataMember]
        public int MatchId { get; set; }

        [DataMember]
        public string MatchName { get; set; }

        [DataMember]
        public int Capacity { get; set; } = 4;

        [DataMember]
        public string Difficulty { get; set; }

        [DataMember]
        public string Visibility { get; set; }

        [DataMember]
        public string HostUsername { get; set; }

        [DataMember]
        public DateTime CreatedAt { get; set; }

        [DataMember]
        public List<PlayerInfo> Players { get; set; } = new List<PlayerInfo>();

        [DataMember]
        public int BoardId { get; set; }
    }

    [DataContract]
    public class PlayerInfo
    {
        [DataMember]
        public int PlayerId { get; set; }

        [DataMember]
        public string PlayerUsername { get; set; }

        [DataMember]
        public string PlayerName { get; set; }

        [DataMember]
        public bool IsHost { get; set; }

        [DataMember]
        public int Position { get; set; }

        [DataMember]
        public byte[] AvatarBytes { get; set; }

        [DataMember]
        public string AvatarFileName { get; set; }
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

        [DataMember(IsRequired = false)]
        public string MatchName { get; set; }

        [DataMember(IsRequired = false)]
        public int Capacity { get; set; } = 4;
    }

    [DataContract]
    public class JoinMatchRequest
    {

        [DataMember(IsRequired = false)]
        public int MatchId { get; set; }

        [DataMember(IsRequired = true)]
        public string Username { get; set; }

        [DataMember(IsRequired = false)]
        public string MatchName { get; set; }

        [DataMember(IsRequired = false)]
        public string HostUsername { get; set; }
    }

    [DataContract]
    public class MatchCard
    {
        [DataMember]
        public int MatchCardID { get; set; }
        [DataMember]
        public int MatchID { get; set; }

        [DataMember]
        public int CardID { get; set; }
    }
}