using log4net;
using MatchManager;
using ProfileManager;
using System.Data.Entity.Core;
using System.ServiceModel;

namespace Forbbiden.Test
{
    [TestFixture]
    public class TestMatchManager
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TestMatchManager));
        private const string ClassName = "TestMatchManager - ";

        private MatchManagerClient _matchClient;
        private ProfileManagerClient _profileClient;

        private readonly List<string> _usersToDelete = new();
        private readonly List<int> _matchesToDelete = new();

        private string _hostUsername;
        private string _guestUsername;
        private string _hash;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _matchClient = new MatchManagerClient();
            _profileClient = new ProfileManagerClient();

            _hash = BCrypt.Net.BCrypt.HashPassword("T3st_pass");
            _hostUsername = $"host_{Guid.NewGuid():N}".Substring(0, 12);
            _guestUsername = $"guest_{Guid.NewGuid():N}".Substring(0, 12);

            try
            {
                await _profileClient.SignUpAsync(new ProfileManager.Player
                {
                    PlayerUsername = _hostUsername,
                    PlayerPassword = _hash,
                    PlayerEmail = $"{_hostUsername}@example.com"
                });
                _usersToDelete.Add(_hostUsername);

                await _profileClient.SignUpAsync(new ProfileManager.Player
                {
                    PlayerUsername = _guestUsername,
                    PlayerPassword = _hash,
                    PlayerEmail = $"{_guestUsername}@example.com"
                });
                _usersToDelete.Add(_guestUsername);
            }
            catch (EntityException ex)
            {
                log.Error(ClassName + "Setup failed", ex);
                throw;
            }
        }

        [OneTimeTearDown]
        public async Task TearDown()
        {
            try
            {
                // Delete matches created during tests
                foreach (var matchId in _matchesToDelete)
                {
                    try { await _matchClient.DeleteMatchAsync(matchId); } catch { /* ignore */ }
                }

                // Delete users created during tests
                foreach (var user in _usersToDelete)
                {
                    try { await _profileClient.DeletePlayerByUsernameAsync(user); } catch { /* ignore */ }
                }
            }
            catch (EntityException ex)
            {
                log.Error(ClassName + "TearDown failed", ex);
            }
            _matchClient?.Close();
            _profileClient?.Close();
        }

        private async Task<int> CreatePublicMatchAsync(string hostUsername, string name = null, int capacity = 4)
        {
            var request = new MatchManager.CreateMatchRequest
            {
                HostUsername = hostUsername,
                MatchName = name ?? $"match_{Guid.NewGuid():N}".Substring(0, 12),
                Capacity = capacity,
                Difficulty = "Normal",
                Visibility = "Public"
            };
            var id = await _matchClient.CreateMatchAsync(request);
            if (id > 0) _matchesToDelete.Add(id);
            return id;
        }

        private async Task<int> CreatePrivateMatchAsync(string hostUsername)
        {
            var request = new MatchManager.CreateMatchRequest
            {
                HostUsername = hostUsername,
                MatchName = $"pmatch_{Guid.NewGuid():N}".Substring(0, 12),
                Capacity = 4,
                Visibility = "Private"
            };
            var id = await _matchClient.CreateMatchAsync(request);
            if (id > 0) _matchesToDelete.Add(id);
            return id;
        }

        // ============================
        // CREATE MATCH
        // ============================

        [Test]
        public async Task CreateMatch_Succeeds_ForValidHost()
        {
            var matchId = await CreatePublicMatchAsync(_hostUsername);
            Assert.That(matchId, Is.GreaterThan(0));
        }

        [Test]
        public void CreateMatch_Throws_ForMissingHost()
        {
            var request = new MatchManager.CreateMatchRequest { HostUsername = "" };
            Assert.ThrowsAsync<FaultException>(async () => await _matchClient.CreateMatchAsync(request));
        }

        // ============================
        // JOIN MATCH
        // ============================

        [Test]
        public async Task JoinMatch_Succeeds_ForExistingMatch()
        {
            var matchId = await CreatePublicMatchAsync(_hostUsername);
            var ok = await _matchClient.JoinMatchAsync(new MatchManager.JoinMatchRequest
            {
                MatchId = matchId,
                Username = _guestUsername
            });
            Assert.That(ok, Is.True);
        }

        [Test]
        public async Task JoinMatch_Fails_WhenCapacityReached()
        {
            var matchId = await CreatePublicMatchAsync(_hostUsername, capacity: 2);

            // First guest
            var guest1 = $"g1_{Guid.NewGuid():N}".Substring(0, 10);
            var guest2 = $"g2_{Guid.NewGuid():N}".Substring(0, 10);
            await _profileClient.SignUpAsync(new ProfileManager.Player { PlayerUsername = guest1, PlayerPassword = _hash, PlayerEmail = $"{guest1}@example.com" });
            await _profileClient.SignUpAsync(new ProfileManager.Player { PlayerUsername = guest2, PlayerPassword = _hash, PlayerEmail = $"{guest2}@example.com" });
            _usersToDelete.Add(guest1);
            _usersToDelete.Add(guest2);

            var ok1 = await _matchClient.JoinMatchAsync(new MatchManager.JoinMatchRequest { MatchId = matchId, Username = guest1 });
            var ok2 = await _matchClient.JoinMatchAsync(new MatchManager.JoinMatchRequest { MatchId = matchId, Username = guest2 });
            var ok3 = await _matchClient.JoinMatchAsync(new MatchManager.JoinMatchRequest { MatchId = matchId, Username = $"extra_{Guid.NewGuid():N}" });

            Assert.That(ok1, Is.True);
            Assert.That(ok2, Is.True);
            Assert.That(ok3, Is.False, "Should fail when capacity is exceeded");
        }

        [Test]
        public async Task JoinMatch_Fails_ForInvalidMatch()
        {
            var ok = await _matchClient.JoinMatchAsync(new MatchManager.JoinMatchRequest
            {
                MatchId = 999999,
                Username = _guestUsername
            });
            Assert.That(ok, Is.False);
        }

        // ============================
        // INVITE CODE
        // ============================

        [Test]
        public async Task GetInviteCode_ReturnsCode_ForPrivateMatch()
        {
            var matchId = await CreatePrivateMatchAsync(_hostUsername);
            var code = await _matchClient.GetInviteCodeAsync(matchId);
            Assert.That(code, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task ValidateInvite_ReturnsTrue_ForValidCode()
        {
            var matchId = await CreatePrivateMatchAsync(_hostUsername);
            var code = await _matchClient.GetInviteCodeAsync(matchId);
            var valid = await _matchClient.ValidateInviteAsync(matchId, code);
            Assert.That(valid, Is.True);
        }

        [Test]
        public async Task ValidateInvite_ReturnsFalse_ForWrongCode()
        {
            var matchId = await CreatePrivateMatchAsync(_hostUsername);
            var valid = await _matchClient.ValidateInviteAsync(matchId, "WRONG");
            Assert.That(valid, Is.False);
        }

        // ============================
        // GET / LIST MATCH
        // ============================

        [Test]
        public async Task GetMatchById_ReturnsMatch_ForExisting()
        {
            var matchId = await CreatePublicMatchAsync(_hostUsername);
            var match = await _matchClient.GetMatchByIdAsync(matchId);
            Assert.That(match, Is.Not.Null);
            Assert.That(match.MatchId, Is.EqualTo(matchId));
            Assert.That(match.HostUsername, Is.EqualTo(_hostUsername));
        }

        [Test]
        public async Task GetMatchById_ReturnsEmpty_ForNonExisting()
        {
            var match = await _matchClient.GetMatchByIdAsync(999999);
            Assert.That(match.MatchId, Is.EqualTo(0));
        }

        [Test]
        public async Task ListMatches_ReturnsAtLeastOneMatch()
        {
            // Ensure at least one match exists
            await CreatePublicMatchAsync(_hostUsername);
            var matches = await _matchClient.ListMatchesAsync();
            Assert.That(matches, Is.Not.Empty);
        }

        // ============================
        // DELETE MATCH
        // ============================

        [Test]
        public async Task DeleteMatch_Succeeds_ForExistingMatch()
        {
            var matchId = await CreatePublicMatchAsync(_hostUsername);
            var deleted = await _matchClient.DeleteMatchAsync(matchId);
            Assert.That(deleted, Is.True);
            _matchesToDelete.Remove(matchId);
        }

        [Test]
        public async Task DeleteMatch_ReturnsFalse_ForNonExisting()
        {
            var deleted = await _matchClient.DeleteMatchAsync(999999);
            Assert.That(deleted, Is.False);
        }
    }
}