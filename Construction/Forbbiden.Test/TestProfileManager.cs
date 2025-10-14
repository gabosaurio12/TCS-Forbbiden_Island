using ProfileManager;
using NUnit.Framework;

namespace Forbbiden.Test
{
    [TestFixture]
    public class TestProfileManager
    {

        [OneTimeSetUp]
        public void Setup()
        {
            var client = new ProfileManagerClient();

            Player player = new Player
            {
                PlayerUsername = "testUser",
                PlayerPassword = "T3st_pass",
                PlayerEmail = "testuser@email.net"
            };

            client.SignUpAsync(player);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            var client = new ProfileManagerClient();
            string username = "testUser";
            client.DeletePlayerByUsernameAsync(username);
        }

        [Test]
        public async Task TestIsEmailAvailableSuccess()
        {
            var client = new ProfileManagerClient();
            string email = "randomEmail@email.net";

            var result = await client.IsEmailAvailableAsync(email);
            Assert.That(result, Is.True, "result should be true");
        }

        [Test]
        public async Task TestIsEmailAvailableInvalidEmail()
        {
            var client = new ProfileManagerClient();
            string email = "randomEmail.com";

            var result = await client.IsEmailAvailableAsync(email);
            Assert.That(result, Is.False, "result should be false");
        }

        [Test]
        public async Task TestIsEmailAvailableEmptyEmail()
        {
            var client = new ProfileManagerClient();
            string email = "";

            var result = await client.IsEmailAvailableAsync(email);
            Assert.That(result, Is.False, "result should be false");
        }

        [Test]
        public async Task TestIsEmailAvailableTakenEmail()
        {
            var client = new ProfileManagerClient();
            string email = "testuser@email.net";

            var result = await client.IsEmailAvailableAsync(email);
            Assert.That(result, Is.False, "result should be false");
        }

    }
}