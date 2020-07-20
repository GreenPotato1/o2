using System;
using System.Net;
using System.Threading.Tasks;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.AuditTrail.Contract.Names;
using Com.O2Bionics.AuditTrail.Contract.Settings;
using Com.O2Bionics.Chat.App.Tests.Utilities;
using Com.O2Bionics.ChatService;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Impl.AuditTrail;
using Com.O2Bionics.ChatService.Impl.Storage;
using Com.O2Bionics.Elastic;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils.JsonSettings;
using Com.O2Bionics.Utils.Network;
using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Com.O2Bionics.Chat.App.Tests
{
    [TestFixture]
    public sealed class LoginTests : BaseControllerTest
    {
        [SetUp]
        [TearDown]
        public async Task SetUp()
        {
            RepairUser();
            Clear();
            await ValidPassword();
        }

        private readonly int m_maxFailedLogins;

        private readonly AuditTrailServiceSettings m_serviceSettings;

        private readonly string m_indexName;

        private readonly EsIndexSettings m_auditIndexSettings;

        private bool m_indexDeleted;

        private uint m_userId;

        private uint UserId
        {
            get
            {
                if (0 == m_userId)
                {
                    var userStorage = new UserStorage(new TestNowProvider());
                    using (var db = new ChatDatabase(Settings.Database))
                    {
                        var user = userStorage.GetByEmail(db, TestConstants.TestUserEmail1, false);
                        Assert.IsNotNull(user, TestConstants.TestUserEmail1);
                        Assert.AreEqual(TestConstants.CustomerId, user.CustomerId, nameof(user.CustomerId));
                        Assert.AreEqual(ObjectStatus.Active, user.Status, nameof(user.Status));
                        m_userId = user.Id;
                    }
                }

                return m_userId;
            }
        }

        private void Clear()
        {
            if (m_indexDeleted)
                return;

            var client = new EsClient(m_serviceSettings.ElasticConnection);
            client.DeleteIndex(m_indexName);
            m_indexDeleted = true;
            AuditIndexHelper.CreateIndex(client, m_auditIndexSettings);
        }

        public LoginTests()
        {
            Assert.IsNotEmpty(Server, nameof(Server));

            m_maxFailedLogins = GetFailedLogins(Settings);
            m_serviceSettings = new JsonSettingsReader().ReadFromFile<AuditTrailServiceSettings>();
            m_indexName = IndexNameFormatter.Format(m_serviceSettings.Index.Name, ProductCodes.Chat);
            m_auditIndexSettings = new EsIndexSettings(m_serviceSettings.Index, m_indexName);
        }

        private static int GetFailedLogins([NotNull] ChatServiceSettings settings)
        {
            var customerStorage = new CustomerStorage(new TestNowProvider());
            var customer = customerStorage.Get(new ChatDatabase(settings.Database), TestConstants.CustomerId);
            Assert.IsNotNull(customer, nameof(customer));

            var result = 3; // TODO: task-121. customer.FailedLogins;
            Assert.Greater(result, 2, nameof(m_maxFailedLogins));
            return result;
        }

        private static void RepairUser()
        {
            // TODO: task-121. repair the user in CUSTOMER_USER.
        }

        private void CheckUser(bool isSuccess = true, bool shouldBeActive = true, int failedLogins = 0)
        {
            var elasticClient = new EsClient(m_serviceSettings.ElasticConnection);

            // TODO: task-121. check the user in CUSTOMER_USER.
            var actual = elasticClient.FetchFirstDocument<AuditEvent<UserInfo>>(m_indexName, OperationKind.UserLoginKey).Value;

            var status = isSuccess ? OperationStatus.SuccessKey : OperationStatus.NotFoundKey;
            Assert.AreEqual(status, actual.Status, nameof(actual.Status));
            Assert.IsNotNull(actual.Author, nameof(actual.Author));

            var expectedAuthor = new Author(UserId, TestConstants.TestUserFullName1);
            expectedAuthor.Should().BeEquivalentTo(actual.Author, nameof(actual.Author));

            if (shouldBeActive)
            {
            }

            if (0 == failedLogins)
            {
            }
        }

        private async Task ValidPassword(bool shouldBeActive = true, int failedLogins = 0, bool shallSendToken = true)
        {
            Clear();
            m_indexDeleted = false;
            await Login(true, shallSendToken);
            CheckUser(shouldBeActive, shouldBeActive, failedLogins);
        }

        private async Task InvalidPassword(bool shouldBeActive, int failedLogins)
        {
            Clear();
            m_indexDeleted = false;
            try
            {
                await ControllerClient.Login(Server, TestConstants.TestUserEmail1, "_invalid_pass_" + TestConstants.TestUserPassword1);
            }
            catch (LoginFailedException)
            {
                CheckUser(false, shouldBeActive, failedLogins);
                return;
            }

            throw new Exception($"The server '{Server}' should have failed on the wrong password.");
        }

        private async Task LockUser(int count)
        {
            Assert.Greater(count, 0, nameof(count));

            for (var i = 1; i <= count; i++)
                await InvalidPassword(i < m_maxFailedLogins, Math.Min(i, m_maxFailedLogins));
        }

        [Test]
        [NUnit.Framework.Ignore("Waiting for task-302")]
        public async Task NFailedAttempts_UserLocked()
        {
            await LockUser(m_maxFailedLogins);
            await ValidPassword(false, m_maxFailedLogins);
        }

        [Test]
        [NUnit.Framework.Ignore("Waiting for task-302")]
        public async Task NMinusOneFailedAttempts_UserNotLocked()
        {
            await LockUser(m_maxFailedLogins - 1);
            await ValidPassword();
        }

        [Test]
        [NUnit.Framework.Ignore("Waiting for task-302")]
        public async Task NPlusOneFailedAttempts_UserLocked()
        {
            await LockUser(m_maxFailedLogins + 1);
            await ValidPassword(false, m_maxFailedLogins);
        }

        [Test]
        [NUnit.Framework.Ignore("Waiting for task-302")]
        public async Task OneFailedAttempt_UserNotLocked()
        {
            await LockUser(1);
            await ValidPassword();
            await ValidPassword();
        }

        [Test]
        public async Task ZeroFailedAttempts_UserNotLocked()
        {
            await ValidPassword();
        }

        [Test]
        public void SkipToken_AccessForbidden()
        {
            var ex = Assert.ThrowsAsync<PostException>(async () => await ValidPassword(shallSendToken: false).ConfigureAwait(false));
            Assert.AreEqual((int)HttpStatusCode.Forbidden, ex.HttpCode, nameof(ex.HttpCode));
        }
    }
}