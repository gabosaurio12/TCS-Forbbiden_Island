using Forbbiden.Contracts;
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

            Player sender = new Player
            {
                PlayerUsername = "testSender",
                PlayerPassword = "T3st_pass",
                PlayerEmail = "testSender@email.net"
            };

            Player receiver = new Player
            {
                PlayerUsername = "testReceiver",
                PlayerPassword = "T3st_pass",
                PlayerEmail = "testReceiver@email.net"
            };

            Player firstFriend = new Player
            {
                PlayerUsername = "firstFriend",
                PlayerPassword = "T3st_pass",
                PlayerEmail = "firstFriend@email.net"
            };

            Player secondFriend = new Player
            {
                PlayerUsername = "secondFriend",
                PlayerPassword = "T3st_pass",
                PlayerEmail = "secondFriend@email.net"
            };

            try
            {
                var profileClient = new ProfileManagerClient();

                await profileClient.SignUpAsync(sender);
                await profileClient.SignUpAsync(receiver);

                var frienshipClient = new FriendsManagerClient();

                await profileClient.SignUpAsync(firstFriend);
                await profileClient.SignUpAsync(secondFriend);

                await frienshipClient.SendFriendRequestAsync(firstFriend.PlayerUsername, secondFriend.PlayerUsername);
                await frienshipClient.AcceptFriendRequestAsync(firstFriend.PlayerUsername, secondFriend.PlayerUsername);

                await frienshipClient.SendFriendRequestAsync(sender.PlayerUsername, firstFriend.PlayerUsername);
                await frienshipClient.SendFriendRequestAsync(sender.PlayerUsername, secondFriend.PlayerUsername);

            }
            catch (EntityException ex)
            {
                log.Error(ClassName, ex);
            }
        }

        [OneTimeTearDown]
        public async Task TearDown()
        {
            try {
                var profileClient = new ProfileManagerClient();
                string sender = "testSender";
                string receiver = "testReceiver";

                string firstFriend = "firstFriend";
                string secondFriend = "secondFriend";

                var friendClient = new FriendsManagerClient();

                await friendClient.CancelFriendRequestAsync(sender, firstFriend);
                await friendClient.CancelFriendRequestAsync(sender, receiver);
                await profileClient.DeletePlayerByUsernameAsync(sender);
                await profileClient.DeletePlayerByUsernameAsync(receiver);

                await profileClient.DeletePlayerByUsernameAsync(firstFriend);
                await profileClient.DeletePlayerByUsernameAsync(secondFriend);
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
            string sender = "testSender";
            string receiver = "testReceiver";
            try {
                var result = await client.SendFriendRequestAsync(sender, receiver);

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
            string sender = "testSender";
            string fakeReceiver = "FriendTest";
            try
            {
                var result = await client.SendFriendRequestAsync(sender, fakeReceiver);

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
            string senderSim = "firstFriend";
            string receiverSim = "secondFriend";
            try
            {
                var result = await client.SendFriendRequestAsync(senderSim, receiverSim);
                Assert.That(result, Is.False, "result should be false");
            }
            catch (EntityException ex)
            {
                log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestAcceptFriendRequestSuccess()
        {
            var client = new FriendsManagerClient();
            string sender = "testSender";
            string recieverSim = "firstFriend";

            try
            {
                var result = await client.AcceptFriendRequestAsync(sender, recieverSim);
                Assert.That(result, Is.True, "result should be true");
            }
            catch (EntityException ex)
            {
                log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestAcceptFriendRequestInvalidUsername()
        {
            var client = new FriendsManagerClient();
            string sender = "testSender";
            string recieverSim = "FriendTest";

            try
            {
                var result = await client.AcceptFriendRequestAsync(sender, recieverSim);
                Assert.That(result, Is.False, "result should be false");
            }
            catch (EntityException ex)
            {
                log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestCancelFriendRequestSuccess()
        {
            var client = new FriendsManagerClient();
            string sender = "testSender";
            string recieverSim = "secondFriend";

            try
            {
                var result = await client.CancelFriendRequestAsync(sender, recieverSim);
                Assert.That(result, Is.True, "result should be true");
            }
            catch (EntityException ex)
            {
                log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestCancelFriendRequestInvalidUsername()
        {
            var client = new FriendsManagerClient();
            string sender = "testSender";
            string recieverSim = "FriendTest";

            try
            {
                var result = await client.CancelFriendRequestAsync(sender, recieverSim);
                Assert.That(result, Is.False, "result should be false");
            }
            catch (EntityException ex)
            {
                log.Error(ClassName, ex);
            }
        }
    }
}
