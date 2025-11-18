using System;
using System.ServiceModel;
using Forbbiden.Server.logic;
using dotenv.net;
using Forbbiden.Server.utils;

namespace Forbbiden.Server
{
    class Server
    {
        private Server() {
            DotEnv.Load();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Iniciando el servidor Forbbiden...");
            DotEnv.Load();

            string dbUser = Environment.GetEnvironmentVariable("FORBBIDEN_USER");
            string dbPass = Environment.GetEnvironmentVariable("FORBBIDEN_PASS");
            string dbHost = Environment.GetEnvironmentVariable("FORBBIDEN_HOST");
            string dbName = Environment.GetEnvironmentVariable("FORBBIDEN_DB");

            Console.WriteLine(dbUser + "\n" + dbPass + "\n" + dbHost + "\n" + dbName);

            ServiceHost profileHost = null;
            ServiceHost friendsHost = null;
            ServiceHost matchHost = null;
            ServiceHost boardHost = null;

            try
            {
                profileHost = new ServiceHost(typeof(ProfileManager));
                friendsHost = new ServiceHost(typeof(FriendsManager));
                matchHost = new ServiceHost(typeof(MatchManager));
                boardHost = new ServiceHost(typeof(BoardManager));

                profileHost.Open();
                friendsHost.Open();
                matchHost.Open();
                boardHost.Open();

                Console.WriteLine("=== Forbbiden Server ===");
                Console.WriteLine("ProfileManager - net.tcp://localhost:8081/ProfileManager");
                Console.WriteLine("FriendsManager - net.tcp://localhost:8082/FriendsManager");
                Console.WriteLine("MatchManager  - net.tcp://localhost:8083/MatchManager");
                Console.WriteLine("BoardManager - net.tcp://localhost:8084/BoardManager");
                Console.WriteLine();

                try
                {
                    string connectionString = ConnectionStringSingleton.GetInstance().connectionString;
                    using (var db = new Forbbiden_FEIEntities(connectionString))
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
                if (boardHost?.State == CommunicationState.Opened)
                    matchHost.Close();

                Console.WriteLine("Servidor detenido.");
            }
        }
    }
}
