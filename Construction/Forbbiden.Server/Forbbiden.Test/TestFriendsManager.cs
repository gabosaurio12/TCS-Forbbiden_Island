using FriendsManager;
using log4net;
using NUnit.Framework.Internal;
using ProfileManager;
using System.Data.Entity.Core;

namespace Forbbiden.Test
{
    [TestFixture]
    public class TestFriendsClient
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TestFriendsClient));
        private const string ClassName = "TestFriendsClient - ";
        private FriendsManagerClient FriendsClient;
        private List<string> UsernamesToDelete;
        private string HashTestPass;

        [OneTimeSetUp]
        public async Task Setup()
        {
            FriendsClient = new FriendsManagerClient();
            UsernamesToDelete = [];
            HashTestPass = BCrypt.Net.BCrypt.HashPassword(HashTestPass);

            ProfileManager.Player sender = new ProfileManager.Player
            {
                PlayerUsername = "testSender",
                PlayerPassword = HashTestPass,
                PlayerEmail = "testSender@email.net"
            };

            ProfileManager.Player receiver = new ProfileManager.Player
            {
                PlayerUsername = "testReceiver",
                PlayerPassword = HashTestPass,
                PlayerEmail = "testReceiver@email.net"
            };

            ProfileManager.Player firstFriend = new ProfileManager.Player
            {
                PlayerUsername = "firstFriend",
                PlayerPassword = HashTestPass,
                PlayerEmail = "firstFriend@email.net"
            };

            ProfileManager.Player secondFriend = new ProfileManager.Player
            {
                PlayerUsername = "secondFriend",
                PlayerPassword = HashTestPass,
                PlayerEmail = "secondFriend@email.net"
            };

            try
            {
                var profileClient = new ProfileManagerClient();

                await profileClient.SignUpAsync(sender);
                UsernamesToDelete.Add(sender.PlayerUsername);
                await profileClient.SignUpAsync(receiver);
                UsernamesToDelete.Add(receiver.PlayerUsername);

                var frienshipClient = new FriendsManagerClient();

                await profileClient.SignUpAsync(firstFriend);
                UsernamesToDelete.Add(firstFriend.PlayerUsername);
                await profileClient.SignUpAsync(secondFriend);
                UsernamesToDelete.Add(secondFriend.PlayerUsername);

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

                await FriendsClient.CancelFriendRequestAsync(sender, firstFriend);
                await FriendsClient.CancelFriendRequestAsync(sender, receiver);
                await profileClient.DeletePlayerByUsernameAsync(sender);
                await profileClient.DeletePlayerByUsernameAsync(receiver);

                await profileClient.DeletePlayerByUsernameAsync(firstFriend);
                await profileClient.DeletePlayerByUsernameAsync(secondFriend);
            }
            catch (EntityException ex)
            {
                log.Error(ClassName, ex);
            }
            FriendsClient.Close();
        }

        [Test]
        public async Task TestSendFriendRequestSuccess()
        {

            string sender = "testSender";
            string receiver = "testReceiver";
            try {
                var result = await FriendsClient.SendFriendRequestAsync(sender, receiver);

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

            string sender = "testSender";
            string fakeReceiver = "FriendTest";
            try
            {
                var result = await FriendsClient.SendFriendRequestAsync(sender, fakeReceiver);

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

            string senderSim = "firstFriend";
            string receiverSim = "secondFriend";
            try
            {
                var result = await FriendsClient.SendFriendRequestAsync(senderSim, receiverSim);
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

            string sender = "testSender";
            string recieverSim = "firstFriend";

            try
            {
                var result = await FriendsClient.AcceptFriendRequestAsync(sender, recieverSim);
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

            string sender = "testSender";
            string recieverSim = "FriendTest";

            try
            {
                var result = await FriendsClient.AcceptFriendRequestAsync(sender, recieverSim);
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

            string sender = "testSender";
            string recieverSim = "secondFriend";

            try
            {
                var result = await FriendsClient.CancelFriendRequestAsync(sender, recieverSim);
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

            string sender = "testSender";
            string recieverSim = "FriendTest";

            try
            {
                var result = await FriendsClient.CancelFriendRequestAsync(sender, recieverSim);
                Assert.That(result, Is.False, "result should be false");
            }
            catch (EntityException ex)
            {
                log.Error(ClassName, ex);
            }
        }
    }
}