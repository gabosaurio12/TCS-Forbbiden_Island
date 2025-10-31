using FriendsManager;
using log4net;
using NUnit.Framework.Internal;
using ProfileManager;
using System.Data.Entity.Core;

namespace Forbbiden.Test
{
    [TestFixture]
    public class TestFriendsManager
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TestFriendsManager));
        private const string ClassName = "TestFriendsManager - ";

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

            Player player = new Player
            {
                PlayerUsername = "testUser",
                PlayerPassword = "T3st_pass",
                PlayerEmail = "testuser@email.net"
            };

            try
            {
                await client.SignUpAsync(friend);

                await client.SignUpAsync(player);
            }
            catch (EntityException ex)
            {
                log.Error(ClassName, ex);
            }
        }

        [OneTimeTearDown]
        public async Task TearDown()
        {
            var profileClient = new ProfileManagerClient();
            string playerUser = "testUser";
            string friendUser = "testFriend";

            var friendClient = new FriendsManagerClient();
            try
            {
                await friendClient.CancelFriendRequestAsync(playerUser, friendUser);

                await profileClient.DeletePlayerByUsernameAsync(playerUser);

                await profileClient.DeletePlayerByUsernameAsync(friendUser);
            }
            catch (EntityException ex)
            {
                log.Error(ClassName, ex);
            }
            
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
            catch (EntityException ex)
            {
                log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestSendFriendRequestInvalidUsername()
        {
            var client = new FriendsManagerClient();
            string playerUsername = "testUser";
            string friendUsername = "FriendTest";
            try
            {
                var result = await client.SendFriendRequestAsync(playerUsername, friendUsername);

                Assert.That(result, Is.False, "result should be false");
            }
            catch (EntityException ex)
            {
                log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestSendFriendRequestDuplicateFriend()
        {
            var client = new FriendsManagerClient();
            string playerUsername = "testUser";
            string friendUsername = "testFriend";
            try
            {
                var result = await client.SendFriendRequestAsync(playerUsername, friendUsername);
                Assert.That(result, Is.False, "result should be false");
            }
            catch (EntityException ex)
            {
                log.Error(ClassName, ex);
            }
        }
    }
}
