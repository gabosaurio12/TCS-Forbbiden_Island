using log4net;
using ProfileManager;
using System.Data.Entity.Core;

namespace Forbbiden.Test
{
    [TestFixture]
    public class TestProfileManager
    {

        private static readonly ILog Log = LogManager.GetLogger(typeof(TestProfileManager));
        private const string ClassName = "TestProfileManager - ";
        private Player testPlayer;

        [OneTimeSetUp]
        public async Task Setup()
        {
            var client = new ProfileManagerClient();

            testPlayer = new Player
            {
                PlayerUsername = "testUser",
                PlayerPassword = "T3st_pass",
                PlayerEmail = "testuser@email.net"
            };

            try
            {
                testPlayer.PlayerId = await client.SignUpAsync(testPlayer);
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
        public async Task TestValidateEmailSuccess()
        {
            var client = new ProfileManagerClient();
            string email = "randomEmail@email.net";

            try
            {
                var result = await client.ValidateEmailAsync(email);
                Assert.That(result, Is.True, "result should be true");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestValidateEmailInvalidEmail()
        {
            var client = new ProfileManagerClient();
            string email = "randomEmail.com";

            try
            {
                var result = await client.ValidateEmailAsync(email);
                Assert.That(result, Is.False, "result should be false");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestValidateEmailEmptyEmail()
        {
            var client = new ProfileManagerClient();
            string email = "";

            try
            {
                var result = await client.ValidateEmailAsync(email);
                Assert.That(result, Is.False, "result should be false");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestValidateEmailTakenEmail()
        {
            var client = new ProfileManagerClient();
            string email = "testuser@email.net";

            try
            {
                var result = await client.ValidateEmailAsync(email);
                Assert.That(result, Is.False, "result should be false");
            } catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestIsUsernameAvailableSucces()
        {
            var client = new ProfileManagerClient();
            string username = "testUser098";

            try
            {
                var result = await client.IsUsernameAvailableAsync(username);
                Assert.That(result, Is.True, "result should be true");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestIsUsernameAvailableTakenUsername()
        {
            var client = new ProfileManagerClient();
            string username = "testUser";

            try
            {
                var result = await client.IsUsernameAvailableAsync(username);
                Assert.That(result, Is.False, "result should be false");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }
    }
}