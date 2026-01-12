using System;
using System.ComponentModel;
using System.Data.Entity.Infrastructure;
using System.Threading;
using dotenv.net;

namespace Forbbiden.Server.utils
{
    sealed class ConnectionStringSingleton
    {

        private static ConnectionStringSingleton instance;
        private static readonly object lockObject = new Object();
        public string ConnectionString;

        private ConnectionStringSingleton() 
        {
        }

        public static ConnectionStringSingleton GetInstance()
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new ConnectionStringSingleton();
                        DotEnv.Load();

                        string forbbidenUser = Environment.GetEnvironmentVariable("FORBBIDEN_USER");
                        string forbbidenPass = Environment.GetEnvironmentVariable("FORBBIDEN_PASS");
                        string forbbidenHost = Environment.GetEnvironmentVariable("FORBBIDEN_HOST");
                        string forbbidenDB = Environment.GetEnvironmentVariable("FORBBIDEN_DB");

                        string sqlConnectionString =
                            $"data source={forbbidenHost};initial catalog={forbbidenDB};user id={forbbidenUser};password={forbbidenPass};" +
                            "trustservercertificate=True;MultipleActiveResultSets=True;App=EntityFramework";

                        instance.ConnectionString =
                            $"metadata=res://*/Model.ForbiddenModel.csdl|res://*/Model.ForbiddenModel.ssdl|res://*/Model.ForbiddenModel.msl;" +
                            $"provider=System.Data.SqlClient;" +
                            $"provider connection string=\"{sqlConnectionString}\"";
                    }
                }
            }

            return instance;
        }
    }
}
