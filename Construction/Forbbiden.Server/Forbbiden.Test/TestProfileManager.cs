using log4net;
using ProfileManager;
using System.ServiceModel;

namespace Forbbiden.Test
{
    [TestFixture]
    public class TestProfileManager
    {

        private static readonly ILog Log = LogManager.GetLogger(typeof(TestProfileManager));
        private const string ClassName = "TestProfileManager";
        private ProfileManagerClient ProfileClient;
        private List<string> UsernamesToDelete;
        private Player TestPlayer;
        private string TestToken;

        [OneTimeSetUp]
        public async Task Setup()
        {
            ProfileClient = new ProfileManagerClient();
            UsernamesToDelete = [];
            TestToken = "123654";

            TestPlayer = new Player
            {
                PlayerUsername = "testUser",
                PlayerPassword = "T3st_pass",
                PlayerEmail = "testuser@email.net"
            };

            try
            {
                TestPlayer.PlayerId = await ProfileClient.SignUpAsync(TestPlayer);
                UsernamesToDelete.Add(TestPlayer.PlayerUsername);
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [OneTimeTearDown]
        public async Task TearDown()
        {
            foreach (var username in UsernamesToDelete)
            {
                try
                {
                    await ProfileClient.DeletePlayerByUsernameAsync(username);
                }
                catch (FaultException<Fault> ex)
                {
                    Log.Error(ClassName, ex);
                }
            }
            
            ProfileClient.Close();
        }

        [Test]
        public async Task TestIsEmailAvailableSuccess()
        {
            string email = "randomEmail@email.net";

            try
            {
                var result = await ProfileClient.IsEmailAvailableAsync(email);
                Assert.That(result, Is.True, "result should be true because is a valid email");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestIsEmailAvailableEmailBlank()
        {
            string email = "";

            try
            {
                var result = await ProfileClient.IsEmailAvailableAsync(email);
                Assert.That(result, Is.False, "result should be false because an empty email can't be available");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestIsEmailAvailableTakenEmail()
        {
            string email = TestPlayer.PlayerEmail;

            try
            {
                var result = await ProfileClient.IsEmailAvailableAsync(email);
                Assert.That(result, Is.False, "result should be false because the email is taken");
            } catch (FaultException<Fault> ex)
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
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestIsUsernameAvailableTakenUsername()
        {
            string username = TestPlayer.PlayerUsername;

            try
            {
                var result = await ProfileClient.IsUsernameAvailableAsync(username);
                Assert.That(result, Is.False, "result should be false");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestIsUsernameAvailableBlankUsername()
        {
            string username = "";

            try
            {
                var result = await ProfileClient.IsUsernameAvailableAsync(username);
                Assert.That(result, Is.False, "result should be false");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestSendSignupEmailSuccess()
        {
            string email = "mazinger.gl@gmail.com";

            try
            {
                var result = await ProfileClient.SendSignupEmailAsync(email, TestToken);
                Assert.That(result, Is.True, "result should be true");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestSendVerificationEmailSuccess()
        {
            string email = "mazinger.gl@gmail.com";

            try
            {
                var result = await ProfileClient.SendVerificationEmailAsync(email, TestToken);
                Assert.That(result, Is.True, "result should be true");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestSendSignupEmailNotExist()
        {
            string email = "falseEmailTest@@email.net";

            try
            {
                var result = await ProfileClient.SendSignupEmailAsync(email, TestToken);
                Assert.That(result, Is.False, "result should be false");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestSendVerificationEmailNotExist()
        {
            string email = "falseEmailTest@@email.net";

            try
            {
                var result = await ProfileClient.SendVerificationEmailAsync(email, TestToken);
                Assert.That(result, Is.False, "result should be false");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestSendSignupEmailBlankEmail()
        {
            string email = "";

            try
            {
                var result = await ProfileClient.SendSignupEmailAsync(email, TestToken);
                Assert.That(result, Is.False, "result should be false");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestSendVerificationEmailBlankEmail()
        {
            string email = "";

            try
            {
                var result = await ProfileClient.SendVerificationEmailAsync(email, TestToken);
                Assert.That(result, Is.False, "result should be false");
            }
            catch (FaultException<Fault> ex)
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
                PlayerPassword = "T3st_pass",
                PlayerEmail = "testplayer@email.com"
            };

            try
            {
                var playerId = await ProfileClient.SignUpAsync(player);
                UsernamesToDelete.Add(player.PlayerUsername);
                bool result = playerId != -1;
                Assert.That(result, Is.True, "result should be true");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestSignUpPlayerExists()
        {
            try
            {
                int playerId = await ProfileClient.SignUpAsync(TestPlayer);
                bool result = playerId == -2;
                Assert.That(result, Is.True, "result should be true because the player exists");

            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestLoginSuccess()
        {
            try
            {
                var player = await ProfileClient.LoginAsync(TestPlayer.PlayerUsername, TestPlayer.PlayerPassword);
                bool result = player.PlayerId != -1;
                Assert.That(result, Is.True, "result should be true");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestLoginNotRegisteredPlayer()
        {
            string password = BCrypt.Net.BCrypt.HashPassword("F4ke_pass");
            var fakePlayer = new Player
            {
                PlayerUsername = "fakePlayer",
                PlayerPassword = password
            };

            try
            {
                var player = await ProfileClient.LoginAsync(fakePlayer.PlayerUsername, fakePlayer.PlayerPassword);
                bool result = player.PlayerId == -1;
                Assert.That(result, Is.True, "result should be true");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestLoginUsernameEmpty()
        {
            string password = BCrypt.Net.BCrypt.HashPassword("F4ke_pass");
            var fakePlayer = new Player
            {
                PlayerUsername = "",
                PlayerPassword = password
            };

            try
            {
                var player = await ProfileClient.LoginAsync(fakePlayer.PlayerUsername, fakePlayer.PlayerPassword);
                bool result = player.PlayerId == -1;
                Assert.That(result, Is.True, "result should be true");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestLoginPasswordEmpty()
        {
            var fakePlayer = new Player
            {
                PlayerUsername = "testUser",
                PlayerPassword = ""
            };

            try
            {
                var player = await ProfileClient.LoginAsync(fakePlayer.PlayerUsername, fakePlayer.PlayerPassword);
                bool result = player.PlayerId == -2;
                Assert.That(result, Is.True, "result should be true");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestLoginUsernameAndPasswordEmpty()
        {
            var fakePlayer = new Player
            {
                PlayerUsername = "",
                PlayerPassword = ""
            };

            try
            {
                var player = await ProfileClient.LoginAsync(fakePlayer.PlayerUsername, fakePlayer.PlayerPassword);
                bool result = player.PlayerId == -1;
                Assert.That(result, Is.True, "result should be true");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestLoginWrongPassword()
        {
            string password = BCrypt.Net.BCrypt.HashPassword("falsePassword");
            var fakePlayer = new Player
            {
                PlayerUsername = "testUser",
                PlayerPassword = password
            };

            try
            {
                var player = await ProfileClient.LoginAsync(fakePlayer.PlayerUsername, fakePlayer.PlayerPassword);
                bool result = player.PlayerId == -2;
                Assert.That(result, Is.True, "result should be true");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestLoginWrongUsername()
        {
            var fakePlayer = new Player
            {
                PlayerUsername = "falseUser",
                PlayerPassword = "T3st_pass"
            };

            try
            {
                var player = await ProfileClient.LoginAsync(fakePlayer.PlayerUsername, fakePlayer.PlayerPassword);
                bool result = player.PlayerId == -1;
                Assert.That(result, Is.True, "result should be true");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestGetPlayerByUsernameSuccess()
        {
            string username = TestPlayer.PlayerUsername;
            try
            {
                var player = await ProfileClient.GetPlayerByUsernameAsync(username, false);
                bool result = player.PlayerId != -1;
                Assert.That(result, Is.True, "result should true");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestGetPlayerByUsernameNotExist()
        {
            string username = "fakePlayer";
            try
            {
                var player = await ProfileClient.GetPlayerByUsernameAsync(username, false);
                bool result = player.PlayerId == -1;
                Assert.That(result, Is.True, "result should be true");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestGetPlayerByUsernameUsernameBlank()
        {
            string username = "";
            try
            {
                var player = await ProfileClient.GetPlayerByUsernameAsync(username, false);
                bool result = player.PlayerId == -1;
                Assert.That(result, Is.True, "result should be true");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestGetPlayerByUsernameUsernameInvalid()
        {
            string username = "fake player";
            try
            {
                var player = await ProfileClient.GetPlayerByUsernameAsync(username, false);
                bool result = player.PlayerId == -1;
                Assert.That(result, Is.True, "result should be true");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestGetPlayerByIdSuccess()
        {
            int playerId = TestPlayer.PlayerId;
            try
            {
                var player = await ProfileClient.GetPlayerByIdAsync(playerId, false);
                bool result = player.PlayerId != -1;
                Assert.That(result, Is.True, "result should be true");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestGetPlayerByIdPlayerIdNonExists()
        {
            int playerId = 0;
            try
            {
                var player = await ProfileClient.GetPlayerByIdAsync(playerId, false);
                bool result = player.PlayerId == -1;
                Assert.That(result, Is.True, "result should be true");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestGetPlayerByIdPlayerIdInvalid()
        {
            int playerId = -2;
            try
            {
                var player = await ProfileClient.GetPlayerByIdAsync(playerId, false);
                bool result = player.PlayerId == -1;
                Assert.That(result, Is.True, "result should be true");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestUpdatePlayerSuccess()
        {
            TestPlayer.PlayerName = "Player's Name";
            TestPlayer.SocialMedia = [];
            try
            {
                var result = await ProfileClient.UpdatePlayerAsync(TestPlayer);
                Assert.That(result, Is.True, "result should be true");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestUpdatePlayerUsernameExists()
        {
            var player = new Player
            {
                PlayerUsername = "updateTest",
                PlayerPassword = "T3st_pass",
                PlayerEmail = "updateTest@email.net"
            };

            try
            {
                await ProfileClient.SignUpAsync(player);
                UsernamesToDelete.Add(player.PlayerUsername);

                player.PlayerUsername = TestPlayer.PlayerUsername;
                var result = await ProfileClient.UpdatePlayerAsync(player);
                Assert.That(result, Is.False, "result should be false");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestUpdatePlayerEmailExists()
        {
            var player = new Player
            {
                PlayerUsername = "updateTestEmail",
                PlayerPassword = "T3st_pass",
                PlayerEmail = "updateTest@email.net"
            };

            try
            {
                await ProfileClient.SignUpAsync(player);
                UsernamesToDelete.Add(player.PlayerUsername);

                player.PlayerEmail = TestPlayer.PlayerEmail;
                var result = await ProfileClient.UpdatePlayerAsync(player);
                Assert.That(result, Is.False, "result should be false");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestDeletePlayerByUsernameSuccess()
        {
            string password = BCrypt.Net.BCrypt.HashPassword("Dr0p_player");
            var player = new Player
            {
                PlayerUsername = "DropPlayer",
                PlayerPassword = password,
                PlayerEmail = "dropplayer@email.com"
            };

            try
            {
                await ProfileClient.SignUpAsync(player);

                var result = await ProfileClient.DeletePlayerByUsernameAsync(player.PlayerUsername);
                Assert.That(result, Is.True, "result should be true");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestDeletePlayerByUsernameUsernameNonExist()
        {
            string username = "falseDropPlayer";

            try
            {
                var result = await ProfileClient.DeletePlayerByUsernameAsync(username);
                Assert.That(result, Is.False, "result should be false");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestDeletePlayerByUsernameUsernameBlank()
        {
            string username = "";

            try
            {
                var result = await ProfileClient.DeletePlayerByUsernameAsync(username);
                Assert.That(result, Is.False, "result should be false");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestConnectPlayerByUsernameSuccess()
        {
            string password = BCrypt.Net.BCrypt.HashPassword("C0nnect_player");
            var player = new Player
            {
                PlayerUsername = "connectPlayer",
                PlayerPassword = password,
                PlayerEmail = "connectPlayer@email.com"
            };

            try
            {
                await ProfileClient.SignUpAsync(player);
                UsernamesToDelete.Add(player.PlayerUsername);

                var result = await ProfileClient.ConnectPlayerByUsernameAsync(player.PlayerUsername);
                Assert.That(result, Is.True, "result should be true");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestConnectPlayerByUsernameUsernameNotExist()
        {
            string username = "falseUsername";
            try
            {
                var result = await ProfileClient.ConnectPlayerByUsernameAsync(username);
                Assert.That(result, Is.False, "result should be false");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestConnectPlayerByUsernameUsernameBlank()
        {
            string username = "";
            try
            {
                var result = await ProfileClient.ConnectPlayerByUsernameAsync(username);
                Assert.That(result, Is.False, "result should be false");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestDisonnectPlayerByUsernameSuccess()
        {
            string password = BCrypt.Net.BCrypt.HashPassword("C0nnect_player");

            var player = new Player
            {
                PlayerUsername = "disonnectPlayer",
                PlayerPassword = password,
                PlayerEmail = "disconnectPlayer@email.com"
            };

            try
            {
                await ProfileClient.SignUpAsync(player);
                UsernamesToDelete.Add(player.PlayerUsername);

                var result = await ProfileClient.DisconnectPlayerByUsernameAsync(player.PlayerUsername);
                Assert.That(result, Is.True, "result should be true");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestDisonnectPlayerByUsernameUsernameNotExist()
        {
            string username = "falseUsername";
            try
            {
                var result = await ProfileClient.DisconnectPlayerByUsernameAsync(username);
                Assert.That(result, Is.False, "result should be false");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestDisconnectPlayerByUsernameUsernameBlank()
        {
            string username = "";
            try
            {
                var result = await ProfileClient.DisconnectPlayerByUsernameAsync(username);
                Assert.That(result, Is.False, "result should be false");
            }
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }
    }
}