using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using ForbbidenService.logic;

namespace ForbbidenHost
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (ServiceHost host = new ServiceHost(typeof(ProfileManager)))
            {
                host.Open();
                Console.WriteLine("Service running");
                Console.ReadLine();
            }
        }
    }
}
