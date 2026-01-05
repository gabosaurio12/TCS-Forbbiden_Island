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
        private ProfileManagerClient ProfileClient;
        private List<string> UsernamesToDelete;

        [OneTimeSetUp]
        public async Task Setup()
        {
            ProfileClient = new ProfileManagerClient();
            UsernamesToDelete = [];

            testPlayer = new Player
            {
                PlayerUsername = "testUser",
                PlayerPassword = "T3st_pass",
                PlayerEmail = "testuser@email.net"
            };

            try
            {
                testPlayer.PlayerId = await ProfileClient.SignUpAsync(testPlayer);
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [OneTimeTearDown]
        public async Task TearDown()
        {
            string username = "testUser";
            try
            {
                await ProfileClient.DeletePlayerByUsernameAsync(username);
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
            ProfileClient.Close();
        }

        [Test]
        public async Task TestValidateEmailSuccess()
        {
            string email = "randomEmail@email.net";

            try
            {
                var result = await ProfileClient.ValidateEmailAsync(email);
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
            string email = "randomEmail.com";

            try
            {
                var result = await ProfileClient.ValidateEmailAsync(email);
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
            string email = "";

            try
            {
                var result = await ProfileClient.ValidateEmailAsync(email);
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
            string email = testPlayer.PlayerEmail;

            try
            {
                var result = await ProfileClient.ValidateEmailAsync(email);
                Assert.That(result, Is.False, "result should be false");
            } catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestIsUsernameAvailableSucces()
        {

            string username = "testUser098";

            try
            {
                var result = await ProfileClient.IsUsernameAvailableAsync(username);
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

            string username = testPlayer.PlayerUsername;

            try
            {
                var result = await ProfileClient.IsUsernameAvailableAsync(username);
                Assert.That(result, Is.False, "result should be false");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestSendEmailSuccess()
        {

            string email = "mazinger.gl@gmail.com";

            try
            {
                var result = await ProfileClient.SendEmailAsync(email, testPlayer.PlayerId);
                Assert.That(result, Is.True, "result should be true");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestSendEmailNotExist()
        {

            string email = "falseEmailTest@email.net";

            try
            {
                var result = await ProfileClient.SendEmailAsync(email, 0);
                Assert.That(result, Is.False, "result should be false");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestSignUpSuccess()
        {

            var player = new Player
            {
                PlayerUsername = "testPlayer",
                PlayerPassword = "T3st_player",
                PlayerEmail = "testplayer@email.com"
            };

            try
            {
                var result = await ProfileClient.SignUpAsync(player);
                UsernamesToDelete.Add(player.PlayerUsername);
                Assert.That(result, Is.True, "result should be true");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestLoginSuccess()
        {


            try
            {
                var result = await ProfileClient.LoginAsync(testPlayer);
                Assert.That(result, Is.True, "result should be true");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestLoginNotRegisteredPlayer()
        {
            var fakePlayer = new Player
            {
                PlayerUsername = "fakePlayer",
                PlayerPassword = "F4ke_pass"
            };

            try
            {
                var result = await ProfileClient.LoginAsync(fakePlayer);
                Assert.That(result, Is.False, "result should be false");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestGetPlayerByUsernameSuccess()
        {
            string username = testPlayer.PlayerUsername;
            try
            {
                var player = await ProfileClient.GetPlayerByUsernameAsync(username, false);
                bool result = player.PlayerId != -1;
                Assert.That(result, Is.True, "result should true");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestGetPlayerByUsernameNonExist()
        {
            string username = "fakePlayer";
            try
            {
                var player = await ProfileClient.GetPlayerByUsernameAsync(username, false);
                bool result = player.PlayerId == -1;
                Assert.That(result, Is.True, "result.PlayerId should be true");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }
    }
}