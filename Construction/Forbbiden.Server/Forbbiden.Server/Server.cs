using System;
using System.ServiceModel;
using dotenv.net;
using Forbbiden.Server.logic;
using Forbbiden.Server.utils;

namespace Forbbiden.Server
{
    class Server
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Iniciando el servidor Forbbiden...");
            DotEnv.Load();

            Console.WriteLine(Environment.GetEnvironmentVariable("FORBBIDEN_USER"));
            Console.WriteLine(Environment.GetEnvironmentVariable("FORBBIDEN_PASS"));
            Console.WriteLine(Environment.GetEnvironmentVariable("FORBBIDEN_HOST"));
            Console.WriteLine(Environment.GetEnvironmentVariable("FORBBIDEN_DB"));

            // SOLO SE INICIALIZAN LOS HOSTS
            var profileHost = new ServiceHost(typeof(ProfileManager));
            var friendsHost = new ServiceHost(typeof(FriendsManager));
            var matchHost = new ServiceHost(typeof(MatchManager));
            var gameHost = new ServiceHost(typeof(GameService));

            try
            {
                // NO SE CREAN ENDPOINTS AQUÍ
                profileHost.Open();
                friendsHost.Open();
                matchHost.Open();
                gameHost.Open();

                Console.WriteLine("=== Forbbiden Server ===");
                Console.WriteLine("Servicios cargados desde App.config.");
                Console.WriteLine();

                try
                {
                    string connectionString = ConnectionStringSingleton.GetInstance().connectionString;
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
                if (profileHost.State == CommunicationState.Opened) profileHost.Close();
                if (friendsHost.State == CommunicationState.Opened) friendsHost.Close();
                if (matchHost.State == CommunicationState.Opened) matchHost.Close();
                if (gameHost.State == CommunicationState.Opened) gameHost.Close();

                Console.WriteLine("Servidor detenido.");
            }
        }
    }
}
