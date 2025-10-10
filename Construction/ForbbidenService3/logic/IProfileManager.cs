using ForbbidenIslandFEI_Construction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ForbbidenServer.logic
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
}
