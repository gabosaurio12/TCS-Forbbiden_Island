using System;
using System.ServiceModel;
using Forbbiden.Server.logic;

namespace Forbbiden.Server
{
    class Server
    {
        private Server() { }

        static void Main(string[] args)
        {
            ServiceHost profileHost = null;
            ServiceHost friendsHost = null;
            ServiceHost matchHost = null;

            try
            {
                profileHost = new ServiceHost(typeof(ProfileManager));
                friendsHost = new ServiceHost(typeof(FriendsManager));
                matchHost = new ServiceHost(typeof(MatchManager));

                profileHost.Open();
                friendsHost.Open();
                matchHost.Open();

                Console.WriteLine("=== Forbbiden Server ===");
                Console.WriteLine("ProfileManager - net.tcp://localhost:8081/ProfileManager");
                Console.WriteLine("FriendsManager - net.tcp://localhost:8082/FriendsManager");
                Console.WriteLine("MatchManager  - net.tcp://localhost:8083/MatchManager");
                Console.WriteLine();

                try
                {
                    using (var db = new Forbbiden_FEIEntities())
                    {
                        db.Database.Connection.Open();
                        Console.WriteLine(" Conexión a la base de datos exitosa");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al conectar a la base de datos:");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.InnerException?.Message);
                }

                Console.WriteLine("Servicios en ejecución. Presiona ENTER para detener el servidor...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al iniciar los servicios:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException?.Message);
            }
            finally
            {
                if (profileHost?.State == CommunicationState.Opened)
                    profileHost.Close();
                if (friendsHost?.State == CommunicationState.Opened)
                    friendsHost.Close();
                if (matchHost?.State == CommunicationState.Opened)
                    matchHost.Close();

                Console.WriteLine("Servidor detenido.");
            }
        }
    }
}
