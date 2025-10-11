using ForbbidenIslandFEI_Construction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ForbbidenService.logic
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IPlayerManager" in both code and config file together.
    [ServiceContract]
    public interface IProfileManager
    {
        [OperationContract]
        void Login(Player player);

        [OperationContract]
        void Signup(Player player);

        [OperationContract]
        void UpdatePersonalInfo();

        [OperationContract]
        void UpdateSocialLinks();

        [OperationContract]
        void UpdateAvatar();
    }

    [DataContract]

    public class PlayerClient
    {
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public string Avatar { get; set; }
        [DataMember]
        public string SocialLinks { get; set; }
    }
}
