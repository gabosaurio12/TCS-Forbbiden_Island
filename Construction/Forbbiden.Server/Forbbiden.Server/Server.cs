using System;
using System.ServiceModel;
using dotenv.net;
using Forbbiden.Server.callbacks;
using Forbbiden.Server.logic;
using Forbbiden.Server.utils;

namespace Forbbiden.Server
{
    public class Server
    {
        private Server()
        {
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Iniciando el servidor Forbbiden...");
            DotEnv.Load();

            var profileHost = new ServiceHost(typeof(ProfileManager));
            var friendsHost = new ServiceHost(typeof(FriendsManager));
            var matchHost = new ServiceHost(typeof(MatchManager));
            var gameHost = new ServiceHost(typeof(GameManager));
            var boardHost = new ServiceHost(typeof(BoardManager));
            var tokenHost = new ServiceHost(typeof(TokenManager));
            var friendsNotificationHost = new ServiceHost(typeof(FriendsNotificationManager));
            var matchNotificationHost = new ServiceHost(typeof(MatchNotificationManager));

            try
            {
                profileHost.Open();
                friendsHost.Open();
                matchHost.Open();
                gameHost.Open();
                boardHost.Open();
                tokenHost.Open();
                friendsNotificationHost.Open();
                matchNotificationHost.Open();

                Console.WriteLine("=== Forbbiden Server ===");
                Console.WriteLine("Servicios cargados desde App.config.");
                Console.WriteLine();

                try
                {
                    string connectionString = ConnectionStringSingleton.GetInstance().ConnectionString;
                    using (var db = new Forbbiden_FEIEntities(connectionString))
                    {
                        db.Database.Connection.Open();
                        Console.WriteLine("Conexión a la base de datos exitosa.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al conectar a la base de datos:");
                    Console.WriteLine(ex.Message);
                }

                Console.WriteLine("Servidor en ejecución. Presiona ENTER para detener...");
                Console.ReadLine();
            }
            finally
            {
                if (profileHost.State == CommunicationState.Opened)
                {
                    profileHost.Close();
                }
                if (friendsHost.State == CommunicationState.Opened)
                {
                    friendsHost.Close();
                }
                if (matchHost.State == CommunicationState.Opened)
                {
                    matchHost.Close();
                }
                if (gameHost.State == CommunicationState.Opened)
                {
                    gameHost.Close();
                }
                if (boardHost.State == CommunicationState.Opened)
                {
                    boardHost.Close();
                }
                if (tokenHost.State == CommunicationState.Opened)
                {
                    tokenHost.Close();
                }
                if (friendsNotificationHost.State == CommunicationState.Opened)
                {
                    friendsNotificationHost.Close();
                }
                if (matchNotificationHost.State == CommunicationState.Opened)
                {
                    matchNotificationHost.Close();
                }

                Console.WriteLine("Servidor detenido.");
            }
        }
    }
}
