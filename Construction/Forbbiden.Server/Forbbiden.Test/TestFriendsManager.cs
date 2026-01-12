using FriendsManager;
using log4net;
using NUnit.Framework.Internal;
using ProfileManager;
using System.Data.Entity.Core;
using System.ServiceModel;

namespace Forbbiden.Test
{
    [TestFixture]
    public class TestFriendsClient
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TestFriendsClient));
        private const string ClassName = "TestFriendsClient";
        private FriendsManagerClient FriendsClient;
        private List<string> UsernamesToDelete;

        [OneTimeSetUp]
        public async Task Setup()
        {
            FriendsClient = new FriendsManagerClient();
            UsernamesToDelete = [];

            ProfileManager.Player sender = new ProfileManager.Player
            {
                PlayerUsername = "testSender",
                PlayerPassword = "T3st_pass",
                PlayerEmail = "testSender@email.net"
            };

            ProfileManager.Player receiver = new ProfileManager.Player
            {
                PlayerUsername = "testReceiver",
                PlayerPassword = "T3st_pass",
                PlayerEmail = "testReceiver@email.net"
            };

            ProfileManager.Player firstFriend = new ProfileManager.Player
            {
                PlayerUsername = "firstFriend",
                PlayerPassword = "T3st_pass",
                PlayerEmail = "firstFriend@email.net"
            };

            ProfileManager.Player secondFriend = new ProfileManager.Player
            {
                PlayerUsername = "secondFriend",
                PlayerPassword = "T3st_pass",
                PlayerEmail = "secondFriend@email.net"
            };

            try
            {
                var profileClient = new ProfileManagerClient();

                await profileClient.SignUpAsync(sender);
                UsernamesToDelete.Add(sender.PlayerUsername);
                await profileClient.SignUpAsync(receiver);
                UsernamesToDelete.Add(receiver.PlayerUsername);

                await profileClient.SignUpAsync(firstFriend);
                UsernamesToDelete.Add(firstFriend.PlayerUsername);
                await profileClient.SignUpAsync(secondFriend);
                UsernamesToDelete.Add(secondFriend.PlayerUsername);

                await FriendsClient.SendFriendRequestAsync(sender.PlayerUsername, firstFriend.PlayerUsername);
                await FriendsClient.SendFriendRequestAsync(sender.PlayerUsername, secondFriend.PlayerUsername);

                await FriendsClient.SendFriendRequestAsync(firstFriend.PlayerUsername, secondFriend.PlayerUsername);
                await FriendsClient.AcceptFriendRequestAsync(firstFriend.PlayerUsername, secondFriend.PlayerUsername);


            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [OneTimeTearDown]
        public async Task TearDown()
        {
            var profileClient = new ProfileManagerClient();
            foreach (var username in UsernamesToDelete)
            {
                try
                {
                    await profileClient.DeletePlayerByUsernameAsync(username);
                }
                catch (FaultException<Contracts.Fault> ex)
                {
                    Log.Error(ClassName, ex);
                }
            }

            profileClient.Close();
            FriendsClient.Close();
        }

        [Test]
        public async Task TestSendFriendRequestSuccess()
        {

            string sender = "testSender";
            string receiver = "testReceiver";
            try {
                var result = await FriendsClient.SendFriendRequestAsync(sender, receiver);

                Assert.That(result, Is.True, "The friend request should be sended successfully");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestSendFriendRequestSenderNonExist()
        {
            string fakeSender = "falseSender";
            string receiver = "testReceiver";
            try
            {
                var result = await FriendsClient.SendFriendRequestAsync(fakeSender, receiver);

                Assert.That(result, Is.False, "sending friend request should fail because " +
                    "sender doesn't exists");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestSendFriendRequestReceiverNonExist()
        {
            string sender = "testSender";
            string fakeReceiver = "falseReceiver";
            try
            {
                var result = await FriendsClient.SendFriendRequestAsync(sender, fakeReceiver);

                Assert.That(result, Is.False, "sending friend request should fail because " +
                    "receiver doesn't exists");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestSendFriendRequestSenderAndReceiverNonExist()
        {
            string fakeSender = "falseSender";
            string fakeReceiver = "falseReceiver";
            try
            {
                var result = await FriendsClient.SendFriendRequestAsync(fakeSender, fakeReceiver);

                Assert.That(result, Is.False, "sending friend request should fail because " +
                    "sender and receiver doesn't exists");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestSendFriendRequestSenderBlank()
        {
            string fakeSender = "";
            string receiver = "testReceiver";
            try
            {
                var result = await FriendsClient.SendFriendRequestAsync(fakeSender, receiver);

                Assert.That(result, Is.False, "sending friend request should fail because " +
                    "sender is blank");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestSendFriendRequestReceiverBlank()
        {
            string sender = "testSender";
            string fakeReceiver = "";
            try
            {
                var result = await FriendsClient.SendFriendRequestAsync(sender, fakeReceiver);

                Assert.That(result, Is.False, "sending friend request should fail because " +
                    "receiver is blank");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestSendFriendRequestSenderAndReceiverBlank()
        {
            string fakeSender = "";
            string fakeReceiver = "";
            try
            {
                var result = await FriendsClient.SendFriendRequestAsync(fakeSender, fakeReceiver);

                Assert.That(result, Is.False, "sending friend request should fail because " +
                    "sender and receiver are blank");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestSendFriendRequestDuplicateFriend()
        {

            string sender = "firstFriend";
            string receiver = "secondFriend";
            try
            {
                var result = await FriendsClient.SendFriendRequestAsync(sender, receiver);
                Assert.That(result, Is.False, "result should be false because there is already " +
                    "a friend request between them");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestAcceptFriendRequestSuccess()
        {

            string sender = "testSender";
            string reciever = "firstFriend";

            try
            {
                var result = await FriendsClient.AcceptFriendRequestAsync(sender, reciever);
                Assert.That(result, Is.True, "result should be true because receiver accepted " +
                    "the friend recuest");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestAcceptFriendRequestSenderNonExist()
        {

            string sender = "falseSender";
            string recieverSim = "firstFriend";

            try
            {
                var result = await FriendsClient.AcceptFriendRequestAsync(sender, recieverSim);
                Assert.That(result, Is.False, "result should be false because the sender " +
                    "doesn't exist");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestAcceptFriendRequestReceiverNonExist()
        {

            string sender = "testSender";
            string reciever = "falseFriend";

            try
            {
                var result = await FriendsClient.AcceptFriendRequestAsync(sender, reciever);
                Assert.That(result, Is.False, "result should be false because the receiver " +
                    "doesn't exist");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestAcceptFriendRequestSenderAndReceiverNonExist()
        {

            string sender = "falseSender";
            string reciever = "falseFriend";

            try
            {
                var result = await FriendsClient.AcceptFriendRequestAsync(sender, reciever);
                Assert.That(result, Is.False, "result should be false because the sender and " +
                    "receiver doesn't exist");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestAcceptFriendRequestSenderBlank()
        {

            string sender = "";
            string recieverSim = "firstFriend";

            try
            {
                var result = await FriendsClient.AcceptFriendRequestAsync(sender, recieverSim);
                Assert.That(result, Is.False, "result should be false because the sender " +
                    "is blank");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestAcceptFriendRequestReceiverBlank()
        {

            string sender = "testSender";
            string reciever = "";

            try
            {
                var result = await FriendsClient.AcceptFriendRequestAsync(sender, reciever);
                Assert.That(result, Is.False, "result should be false because the receiver " +
                    "is blank");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestAcceptFriendRequestSenderAndReceiverBlank()
        {

            string sender = "";
            string reciever = "";

            try
            {
                var result = await FriendsClient.AcceptFriendRequestAsync(sender, reciever);
                Assert.That(result, Is.False, "result should be false because the sender and " +
                    "receiver are blank");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestCancelFriendRequestSuccess()
        {

            string sender = "testSender";
            string receiver = "secondFriend";

            try
            {
                var result = await FriendsClient.CancelFriendRequestAsync(sender, receiver);
                Assert.That(result, Is.True, "result should be true because the receiver " +
                    "declined the friend request");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestCancelFriendRequestSenderNonExist()
        {

            string sender = "falseSender";
            string receiver = "secondFriend";

            try
            {
                var result = await FriendsClient.CancelFriendRequestAsync(sender, receiver);
                Assert.That(result, Is.False, "result should be false because the sender " +
                    "doesn't exist");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestCancelFriendRequestReceiverNonExist()
        {

            string sender = "testSender";
            string receiver = "falseFriend";

            try
            {
                var result = await FriendsClient.CancelFriendRequestAsync(sender, receiver);
                Assert.That(result, Is.False, "result should be false because the receiver " +
                    "doesn't exist");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestCancelFriendRequestSenderAndReceiverNonExist()
        {

            string sender = "falseSender";
            string receiver = "falseFriend";

            try
            {
                var result = await FriendsClient.CancelFriendRequestAsync(sender, receiver);
                Assert.That(result, Is.False, "result should be false because the sender " +
                    "and receiver don't exist");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }



        [Test]
        public async Task TestCancelFriendRequestSenderBlank()
        {
            string sender = "";
            string receiver = "secondFriend";

            try
            {
                var result = await FriendsClient.CancelFriendRequestAsync(sender, receiver);
                Assert.That(result, Is.False, "result should be false because the sender " +
                    "is blank");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestCancelFriendRequestReceiverBlank()
        {
            string sender = "testSender";
            string receiver = "";

            try
            {
                var result = await FriendsClient.CancelFriendRequestAsync(sender, receiver);
                Assert.That(result, Is.False, "result should be false because the receiver " +
                    "is blank");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestCancelFriendRequestSenderAndReceiverBlank()
        {
            string sender = "";
            string receiver = "";

            try
            {
                var result = await FriendsClient.CancelFriendRequestAsync(sender, receiver);
                Assert.That(result, Is.False, "result should be false because the sender " +
                    "and receiver are blank");
            }
            catch (EntityException ex)
            {
                Log.Error(ClassName, ex);
            }
        }

        [Test]
        public async Task TestGetSenderReceiverSuccess()
        {
            string sender = "testSender";
        }
    }
}