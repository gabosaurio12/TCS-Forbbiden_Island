using log4net;
using ProfileManager;
using System.Data.Entity.Core;
using TokenManager;

namespace Forbbiden.Test
{
    public class TestTokenManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TestTokenManager));
        private const string ClassName = "TestTokenManager - ";
        private ProfileManager.Player player;

        [OneTimeSetUp]
        public async Task Setup()
        {
            var client = new ProfileManagerClient();

            player = new ProfileManager.Player
            {
                PlayerUsername = "testUser",
                PlayerPassword = "T3st_pass",
                PlayerEmail = "testuser@email.net"
            };

            try
            {
                player.PlayerId = await client.SignUpAsync(player);
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [OneTimeTearDown]
        public async Task TearDown()
        {
            var client = new ProfileManagerClient();
            string username = "testUser";
            try
            {
                await client.DeletePlayerByUsernameAsync(username);
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestCreateRandomTokenSuccess()
        {
            var client = new TokenManagerClient();
            string token = await client.CreateRandomTokenAsync();
            Assert.That(token, Has.Length.EqualTo(6), "Should be 6");
        }

        [Test]
        public async Task TestGenerateTokenSuccess()
        {
            var client = new TokenManagerClient();
            TokenManager.Token token = await client.GenerateTokenAsync(player.PlayerId);
            await client.VerifyTokenAsync(token.TokenString, token.PlayerId);
            Assert.That(token.PlayerId, Is.EqualTo(player.PlayerId), "Should be the same");
        }

        /*[Test]
        public async Task TestGenerateTokenFaultException()
        {
            var client = new TokenManagerClient();
            int fakePlayerId = -1;

            await Assert.ThrowsAsync<FaultException<DBFault>(() =>
                client.GenerateTokenAsync(fakePlayerId)
            );
        }*/
    }
}
