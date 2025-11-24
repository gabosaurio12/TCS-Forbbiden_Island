using Forbbiden.Contracts;
using log4net;
using ProfileManager;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forbbiden.Test
{
    public class TestTokenManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TestTokenManager));
        private const string ClassName = "TestTokenManager - ";
        private Player player;

        [OneTimeSetUp]
        public async Task Setup()
        {
            var client = new ProfileManagerClient();

            player = new Player
            {
                PlayerUsername = "testUser",
                PlayerPassword = "T3st_pass",
                PlayerEmail = "testuser@email.net"
            };

            try
            {
                await client.SignUpAsync(player);
                player = await client.GetPlayerByUsernameAsync(player.PlayerUsername, false);
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }
    }
}
