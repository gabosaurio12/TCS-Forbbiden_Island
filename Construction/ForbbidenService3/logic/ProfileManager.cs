using ForbbidenIslandFEI_Construction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ForbbidenServer.logic
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "PlayerManager" in both code and config file together.
    public class ProfileManager : IProfileManager
    {

        public void Login(Player player)
        {
            throw new NotImplementedException();
        }

        public void Signup(Player player)
        {
            Signup signup = new Signup();
            signup.SignUp(player);
        }

        public void UpdateAvatar()
        {
            throw new NotImplementedException();
        }

        public void UpdatePersonalInfo()
        {
            throw new NotImplementedException();
        }

        public void UpdateSocialLinks()
        {
            throw new NotImplementedException();
        }
    }
}
