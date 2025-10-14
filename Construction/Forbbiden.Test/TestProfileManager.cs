using ProfileManager;

namespace Forbbiden.Test
{
    public class TestProfileManager
    {

        [SetUp]
        public void Setup()
        {
            var profileManager = new ProfileManagerClient();
        }

        [Test]
        public void TestIsEmailAvailableSuccess()
        {
            Assert.Pass();
        }
    }
}