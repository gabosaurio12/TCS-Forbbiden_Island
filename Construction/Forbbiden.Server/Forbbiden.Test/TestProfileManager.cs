using log4net;
using ProfileManager;
using System.ServiceModel;

namespace Forbbiden.Test
{
    [TestFixture]
    public class TestProfileManager
    {

        private static readonly ILog Log = LogManager.GetLogger(typeof(TestProfileManager));
        private const string ClassName = "TestProfileManager - ";
        private ProfileManagerClient ProfileClient;
        private List<string> UsernamesToDelete;
        private Player testPlayer;
        private string HashTestPass;

        [OneTimeSetUp]
        public async Task Setup()
        {
            ProfileClient = new ProfileManagerClient();
            UsernamesToDelete = [];

            HashTestPass = BCrypt.Net.BCrypt.HashPassword("T3st_pass");

            testPlayer = new Player
            {
                PlayerUsername = "testUser",
                PlayerPassword = HashTestPass,
                PlayerEmail = "testuser@email.net"
            };

            try
            {
                testPlayer.PlayerId = await ProfileClient.SignUpAsync(testPlayer);
                UsernamesToDelete.Add(testPlayer.PlayerUsername);
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
        public async Task TestValidateEmailSuccess()
        {
            string email = "randomEmail@email.net";

            try
            {
                var result = await ProfileClient.ValidateEmailAsync(email);
                Assert.That(result, Is.True, "result should be true");
            }
            catch (FaultException<Fault> ex)
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
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestValidateEmailEmailBlank()
        {
            string email = "";

            try
            {
                var result = await ProfileClient.ValidateEmailAsync(email);
                Assert.That(result, Is.False, "result should be false");
            }
            catch (FaultException<Fault> ex)
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
            string username = testPlayer.PlayerUsername;

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
        public async Task TestSendEmailSuccess()
        {
            string email = "mazinger.gl@gmail.com";

            try
            {
                var result = await ProfileClient.SendEmailAsync(email, testPlayer.PlayerId);
                Assert.That(result, Is.True, "result should be true");
            }
            catch (FaultException<Fault> ex)
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
            catch (FaultException<Fault> ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestSendEmailBlankEmail()
        {
            string email = "";

            try
            {
                var result = await ProfileClient.SendEmailAsync(email, 0);
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
                PlayerPassword = HashTestPass,
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
                int playerId = await ProfileClient.SignUpAsync(testPlayer);
                bool result = playerId == -1;
                Assert.That(result, Is.True, "result should be true");

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
                var player = await ProfileClient.LoginAsync(testPlayer.PlayerUsername, testPlayer.PlayerPassword);
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
                PlayerPassword = HashTestPass
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
            string username = testPlayer.PlayerUsername;
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
            int playerId = testPlayer.PlayerId;
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
            testPlayer.PlayerName = "Player's Name";
            testPlayer.SocialMedia = [];
            try
            {
                var result = await ProfileClient.UpdatePlayerAsync(testPlayer);
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
                PlayerPassword = HashTestPass,
                PlayerEmail = "updateTest@email.net"
            };

            try
            {
                await ProfileClient.SignUpAsync(player);
                UsernamesToDelete.Add(player.PlayerUsername);

                player.PlayerUsername = testPlayer.PlayerUsername;
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
                PlayerPassword = HashTestPass,
                PlayerEmail = "updateTest@email.net"
            };

            try
            {
                await ProfileClient.SignUpAsync(player);
                UsernamesToDelete.Add(player.PlayerUsername);

                player.PlayerEmail = testPlayer.PlayerEmail;
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