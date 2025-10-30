using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Forbbiden.Server
{
    class Server
    {
        private Server() { }
        static void Main(string[] args)
        {
            ServiceHost profileHost = new ServiceHost(typeof(logic.ProfileManager));
            profileHost.Open();
            ServiceHost friendsHost = new ServiceHost(typeof(logic.FriendsManager));
            friendsHost.Open();

            try
            {
                using (var db = new Forbbiden_FEIEntities())
                {
                    db.Database.Connection.Open();
                    Console.WriteLine("Conexión exitosa");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al conectar: " + ex.Message);
                Console.WriteLine(ex.InnerException?.Message);
            }

            Console.WriteLine("Service is running...");
            Console.ReadLine();

            profileHost.Close();
            friendsHost.Close();
        }
    }
}
