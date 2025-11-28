using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Forbbiden.Contracts
{
    [ServiceContract]
    public interface IProfileManager
    {
        [OperationContract]
        [FaultContract(typeof(DBFault))]
        bool ValidateEmail(string email);

        [OperationContract]
        [FaultContract(typeof(DBFault))]
        bool IsUsernameAvailable(string username);

        [OperationContract]
        [FaultContract(typeof(EmailFault))]
        bool SendEmail(string email, int playerId);

        [OperationContract]
        [FaultContract(typeof(DBFault))]
        int SignUp(Player player);

        [OperationContract]
        [FaultContract(typeof(DBFault))]
        Player Login(string username, string password);

        [OperationContract]
        [FaultContract(typeof(DBFault))]
        Player GetPlayerByUsername(string username, bool includeFriends = true);

        [OperationContract]
        [FaultContract(typeof(DBFault))]
        Player GetPlayerById(int playerId, bool includeFriends = true);

        [OperationContract]
        bool ConnectPlayerByUsername(string username);

        [OperationContract]
        bool DisconnectPlayerByUsername(string username);

        [OperationContract]
        [FaultContract(typeof(DBFault))]
        bool UpdatePlayer(Player updatedPlayer);

        [OperationContract]
        [FaultContract(typeof(DBFault))]
        bool DeletePlayerByUsername(string username);
    }

    [DataContract]
    public class Player
    {
        [DataMember]
        public int PlayerId { get; set; }
        [DataMember]
        public string PlayerName { get; set; }
        [DataMember]
        public string PlayerUsername { get; set; }
        [DataMember]
        public string PlayerPassword { get; set; }
        [DataMember]
        public string PlayerEmail { get; set; }
        [DataMember]
        public string PlayerAvatarPath { get; set; }
        [DataMember]
        public int Status { get; set; }
        [DataMember]
        public int IsVerified { get; set; }
        [DataMember]
        public List<SocialMedia> SocialMedia { get; set; }
        [DataMember]
        public List<Friendship> Friends { get; set; }
    }

    [DataContract]
    public class SocialMedia
    {
        [DataMember]
        public int SocialMediaId { get; set; }
        [DataMember]
        public int PlayerId { get; set; }
        [DataMember]
        public string SocialLink { get; set; }
        [DataMember]
        public string SocialMediaName { get; set; }
    }

    [DataContract]
    public class DBFault
    {
        [DataMember]
        public string Error { get; set; }
        [DataMember]
        public string Details { get; set; }
    }

    [DataContract]
    public class EmailFault
    {
        [DataMember]
        public string Error { get; set; }

        [DataMember]
        public string Details { get; set; }
    }
}