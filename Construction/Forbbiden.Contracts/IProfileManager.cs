using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Forbbiden.Contracts
{
    [ServiceContract]
    public interface IProfileManager
    {
        [OperationContract]
        bool ValidateEmail(string email);

        [OperationContract]
        bool IsUsernameAvailable(string username);

        [OperationContract]
        bool SendEmail(string email);

        [OperationContract]
        bool SignUp(Player player);

        [OperationContract]
        bool Login(Player player);

        [OperationContract]
        Player GetPlayerByUsername(string username);

        [OperationContract]
        Player GetCurrentLogin();

        [OperationContract]
        bool ClearCurrentLogin();

        [OperationContract]
        Player GetPlayerById(int playerId);

        [OperationContract]
        bool UpdatePlayer(Player updatedPlayer);

        [OperationContract]
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
        public List<SocialMedia> SocialMedia { get; set; }
        [DataMember]
        public List<Player> Friends { get; set; }
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
}
