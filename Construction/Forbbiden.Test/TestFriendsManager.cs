using ProfileManager;
using FriendsManager;
using NUnit.Framework.Internal;

namespace Forbbiden.Test
{
    [TestFixture]
    public class TestFriendsManager
    {
        [OneTimeSetUp]
        public async Task Setup()
        {
            var client = new ProfileManagerClient();

            Player friend = new Player
            {
                PlayerUsername = "testFriend",
                PlayerPassword = "T3st_pass",
                PlayerEmail = "testfriend@email.net"
            };

            await client.SignUpAsync(friend);

            Player player = new Player
            {
                PlayerUsername = "testUser",
                PlayerPassword = "T3st_pass",
                PlayerEmail = "testuser@email.net"
            };

            await client.SignUpAsync(player);
        }

        [OneTimeTearDown]
        public async Task TearDown()
        {
            var profileClient = new ProfileManagerClient();
            string playerUser = "testUser";
            string friendUser = "testFriend";

            var friendClient = new FriendsManagerClient();
            await friendClient.CancelFriendRequestAsync(playerUser, friendUser);

            await profileClient.DeletePlayerByUsernameAsync(playerUser);
           
            await profileClient.DeletePlayerByUsernameAsync(friendUser);
        }

        [Test]
        public async Task TestSendFriendRequestSuccess()
        {
            var client = new FriendsManagerClient();
            string playerUsername = "testUser";
            string friendUsername = "testFriend";
            try {
                var result = await client.SendFriendRequestAsync(playerUsername, friendUsername);

                Assert.That(result, Is.True, "result should be true");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
