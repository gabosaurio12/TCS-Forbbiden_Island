using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Forbbiden.Contracts
{
    [ServiceContract]
    public interface IProfileManager
    {
        [OperationContract]
        [FaultContract(typeof(Fault))]
        bool IsEmailAvailable(string email);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        bool IsUsernameAvailable(string username);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        bool SendSignupEmail(string email, string token);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        bool SendVerificationEmail(string email, string token);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        int SignUp(Player player);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        Player Login(string username, string password);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        Player GetPlayerByUsername(string username, bool includeFriends = true);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        Player GetPlayerById(int playerId, bool includeFriends = true);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        bool UpdatePlayer(Player updatedPlayer);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        bool DeletePlayerByUsername(string username);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        bool ConnectPlayerByUsername(string username);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        bool DisconnectPlayerByUsername(string username);

        [OperationContract]
        byte[] GetAvatar(string fileName);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        bool UploadAvatar(string username, byte[] avatarBytes, string fileName);

        [OperationContract]
        [FaultContract(typeof(Fault))]
        AvatarResponse GetAvatarByUsername(string username);
    }

    [DataContract]
    public class AvatarResponse
    {
        [DataMember]
        public byte[] AvatarBytes { get; set; }
        [DataMember]
        public string FileName { get; set; }
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
        public byte[] PlayerAvatarBytes { get; set; }
        [DataMember]
        public string PlayerAvatarName { get; set; }

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
    public class Fault
    {
        [DataMember]
        public string Error { get; set; }
        [DataMember]
        public string Details { get; set; }
    }
}