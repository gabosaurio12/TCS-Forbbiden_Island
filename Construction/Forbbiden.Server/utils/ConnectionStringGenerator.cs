using System;
using dotenv.net;

namespace Forbbiden.Server.utils
{
    internal class ConnectionStringGenerator
    {
        private ConnectionStringGenerator() 
        {
        }

        public static string Generate()
        {
            DotEnv.Load();

            string forbbidenUser = Environment.GetEnvironmentVariable("FORBBIDEN_USER");
            string forbbidenPass = Environment.GetEnvironmentVariable("FORBBIDEN_PASS");
            string forbbidenHost = Environment.GetEnvironmentVariable("FORBBIDEN_HOST");
            string forbbidenDB = Environment.GetEnvironmentVariable("FORBBIDEN_DB");

            string sqlConnectionString = 
                $"data source={forbbidenHost};initial catalog={forbbidenDB};user id={forbbidenUser};password={forbbidenPass};" +
                "trustservercertificate=True;MultipleActiveResultSets=True;App=EntityFramework";

            string entityConnectionString = 
                $"metadata=res://*/ForbbidenModel.csdl|res://*/ForbbidenModel.ssdl|res://*/ForbbidenModel.msl;" +
                $"provider=System.Data.SqlClient;" +
                $"provider connection string=\"{sqlConnectionString}\"";

            return entityConnectionString;
        }
    }
}
