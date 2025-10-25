using ProfileManager;
using FriendsManager;

namespace Forbbiden.Test
{
    [TestFixture]
    public class TestFriendsManager
    {
        [OneTimeSetUp]
        public void Setup()
        {
            var client = new ProfileManagerClient();

            Player friend = new Player
            {
                PlayerUsername = "testFriend",
                PlayerPassword = "T3st_pass",
                PlayerEmail = "testfriend@email.net"
            };

            client.SignUpAsync(friend);

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
            string playerUser = "testUser";
            client.DeletePlayerByUsernameAsync(playerUser);
            string friendUser = "testFriend";
            client.DeletePlayerByUsernameAsync(friendUser);
        }

        [Test]
        public async Task TestAddFriendSuccess()
        {
            var client = new FriendsManagerClient();
            string playerUsername = "testUser";
            string friendUsername = "testFriend";
            var result = await client.AddSendFriendRequestAsync(playerUsername, friendUsername);

            Assert.That(result, Is.True, "result should be true");
        }
    }
}
