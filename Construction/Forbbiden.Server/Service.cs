using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Forbbiden.Server
{
    class Service
    {
        private Service() { }
        static void Main(string[] args)
        {
            ServiceHost profileHost = new ServiceHost(typeof(logic.ProfileManager));
            profileHost.Open();
            ServiceHost friendsHost = new ServiceHost(typeof(logic.FriendsManager));
            friendsHost.Open();

            Console.WriteLine("Service is running...");
            Console.ReadLine();

            profileHost.Close();
            friendsHost.Close();
        }
    }
}
