using Forbbiden.Contracts;
using Forbbiden.Server.utils;
using log4net;
using System;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Text;

namespace Forbbiden.Server.logic
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "TokenManager" in both code and config file together.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]

    public class TokenManager : ITokenManager
    {
        private readonly string ConnectionString;
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProfileManager));

        public TokenManager()
        {
            ConnectionString = ConnectionStringSingleton.GetInstance().connectionString;
        }

        private void HandleEntityException(Exception ex)
        {
            Log.Error(ex);

            var fault = new DBFault
            {
                Error = "Database Error",
                Details = ex.Message
            };

            throw new FaultException<DBFault>(fault,
                new FaultReason(fault.Error));
        }

        public string CreateRandomToken()
        {
            int tokenLength = 6;
            Random random = new Random();
            int minRandom = 0;
            int maxRandom = 9;
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < tokenLength; i++)
            {
                int randomToken = random.Next(minRandom, maxRandom);
                builder.Append(randomToken);
            }

            string randomTokenString = builder.ToString();

            return randomTokenString;
        }

        private bool RemoveExistingToken(int playerId)
        {
            bool removed = false;
            using (var db = new Forbbiden_FEIEntities(ConnectionString))
            {
                try
                {
                    var existingToken = db.Token.FirstOrDefault(t => t.player_id == playerId);
                    if (existingToken != null)
                    {
                        db.Token.Remove(existingToken);
                        db.SaveChanges();
                        removed = true;
                    }
                }
                catch (EntityException ex)
                {
                    HandleEntityException(ex);
                }
            }

            return removed;
        }

        public Contracts.Token GenerateToken(int playerId)
        {
            string randomTokenString = CreateRandomToken();
            bool success = false;
            RemoveExistingToken(playerId);
            using (var db = new Forbbiden_FEIEntities(ConnectionString))
            {
                do
                {
                    try
                    {
                        var searchToken = db.Token.FirstOrDefault(t => t.token1 == randomTokenString);
                        if (searchToken == null)
                        {
                            searchToken = new Token
                            {
                                token1 = randomTokenString,
                                player_id = playerId
                            };
                            db.Token.Add(searchToken);
                            db.SaveChanges();
                            success = true;

                            return new Contracts.Token
                            {
                                Id = searchToken.token_id,
                                TokenString = searchToken.token1,
                                PlayerId = (int)searchToken.player_id
                            };
                        }
                    }
                    catch (Exception ex) when (ex is DbUpdateException || ex is EntityException)
                    {
                        HandleEntityException(ex);
                    }
                } while (!success);
            }

            return new Contracts.Token
            {
                Id = -1
            };
        }

        public Contracts.Token GetToken(int playerId)
        {
            using (var db = new Forbbiden_FEIEntities(ConnectionString))
            {
                var searchToken = db.Token.FirstOrDefault(t => t.player_id == playerId);
                if (searchToken != null)
                {
                    return new Contracts.Token
                    {
                        Id = searchToken.token_id,
                        TokenString = searchToken.token1,
                        PlayerId = (int)searchToken.player_id
                    };
                }
            }

            return new Contracts.Token
            {
                Id = -1
            };
        }

        public bool VerifyToken(string token, int playerId)
        {
            bool isTokenCorrect = false;
            using (var db = new Forbbiden_FEIEntities(ConnectionString))
            {
                try
                {
                    var searchToken = db.Token.FirstOrDefault(t => t.token1 == token && t.player_id == playerId);
                    if (searchToken != null)
                    {
                        isTokenCorrect = true;
                        db.Token.Remove(searchToken);
                        db.SaveChanges();
                    }
                }
                catch (EntityException ex)
                {
                    HandleEntityException(ex);
                }
            }

            return isTokenCorrect;
        }
    }
}
