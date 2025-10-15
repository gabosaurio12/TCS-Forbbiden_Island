using ProfileManager;

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
        public async Task TestValidateEmailSuccess()
        {
            var client = new ProfileManagerClient();
            string email = "randomEmail@email.net";

            var result = await client.ValidateEmailAsync(email);
            Assert.That(result, Is.True, "result should be true");
        }

        [Test]
        public async Task TestValidateEmailInvalidEmail()
        {
            var client = new ProfileManagerClient();
            string email = "randomEmail.com";

            var result = await client.ValidateEmailAsync(email);
            Assert.That(result, Is.False, "result should be false");
        }

        [Test]
        public async Task TestValidateEmailEmptyEmail()
        {
            var client = new ProfileManagerClient();
            string email = "";

            var result = await client.ValidateEmailAsync(email);
            Assert.That(result, Is.False, "result should be false");
        }

        [Test]
        public async Task TestValidateEmailTakenEmail()
        {
            var client = new ProfileManagerClient();
            string email = "testuser@email.net";

            var result = await client.ValidateEmailAsync(email);
            Assert.That(result, Is.False, "result should be false");
        }

        [Test]
        public async Task TestIsUsernameAvailableSucces()
        {
            var client = new ProfileManagerClient();
            string username = "testUser098";

            var result = await client.IsUsernameAvailableAsync(username);
            Assert.That(result, Is.True, "result should be true");
        }

        [Test]
        public async Task TestIsUsernameAvailableTakenUsername()
        {
            var client = new ProfileManagerClient();
            string username = "testUser";

            var result = await client.IsUsernameAvailableAsync(username);
            Assert.That(result, Is.False, "result should be false");
        }
    }
}