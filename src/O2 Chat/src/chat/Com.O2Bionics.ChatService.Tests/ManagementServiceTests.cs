using System.Linq;
using Com.O2Bionics.AuditTrail.Client;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Impl;
using Com.O2Bionics.ChatService.Impl.Storage;
using Com.O2Bionics.ChatService.Objects;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.MailerService.Client;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.JsonSettings;
using FluentAssertions;
using log4net;
using NUnit.Framework;
using NSubstitute;

namespace Com.O2Bionics.ChatService.Tests
{
    [TestFixture]
    public class ManagementServiceTests
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(ManagementService));

        public static readonly string ConnectionString =
            new JsonSettingsReader().ReadFromFile<TestSettings>().ChatServiceDatabase;

        private const uint CustomerId = 1u;
        private const uint UserId = 9u;
        private const uint DepartmentId = 2;

        private INowProvider m_nowProvider;
        private IUserStorage m_userStorage;
        private IAccessManager m_accessManager;
        private TestChatServiceSettings m_settings;
        private ManagementService m_managementService;
        private IDepartmentStorage m_departmentStorage;

        [SetUp]
        public void SetUp()
        {
            new DatabaseManager(ConnectionString, false).RecreateSchema();
            new DatabaseManager(ConnectionString, false).ReloadData();

            m_nowProvider = new DefaultNowProvider();
            m_userStorage = new UserStorage(m_nowProvider);
            m_accessManager = new AccessManager();
            m_departmentStorage = new DepartmentStorage(m_nowProvider);
            m_settings = new TestChatServiceSettings();

            m_managementService = new ManagementService(
                Substitute.For<IUserManager>(),
                m_departmentStorage,
                m_userStorage,
                m_accessManager,
                Substitute.For<ISubscriptionManager>(),
                m_nowProvider,
                new ChatDatabaseFactory(m_settings.Database),
                Substitute.For<IChatSessionManager>(),
                Substitute.For<ICustomerStorage>(),
                Substitute.For<IMailerServiceClient>(),
                Substitute.For<IFeatureServiceClient>(),
                Substitute.For<IChatWidgetAppearanceManager>(),
                m_settings,
                Substitute.For<IVisitorStorage>(),
                Substitute.For<IChatSessionStorage>(),
                new CannedMessageStorage(m_nowProvider),
                Substitute.For<ICustomerWidgetLoadStorage>(),
                Substitute.For<IAuditTrailClient>());
        }

        [Test]
        public void Test_CreateNewCannedMessage_UserId_DepartmentsIsNull()
        {
            var cm = new CannedMessage(UserId, null, "create", "blah dsa fds");
            var r = m_managementService.CreateNewCannedMessage(CustomerId, UserId, cm.AsInfo());
            r.Status.StatusCode.Should().Be(CallResultStatusCode.Success);
        }

        [Test]
        public void Test_GetDepartmentCannedMessages()
        {
            var cm = new CannedMessage(null, DepartmentId, "create", "blah dsa fds2");
            var cm2 = new CannedMessage(UserId, null, "create2", "blah dsa fds2");
            var r = m_managementService.CreateNewCannedMessage(CustomerId, UserId, cm.AsInfo());
            var r2 = m_managementService.CreateNewCannedMessage(CustomerId, UserId, cm2.AsInfo());
            var read = m_managementService.GetDepartmentCannedMessages(CustomerId, UserId, DepartmentId);
            read.CannedMessages.Count(x => x.Id == r2.CannedMessage.Id).Should().Be(0);
            read.CannedMessages.Count(x => x.Id == r.CannedMessage.Id).Should().Be(1);
        }

        [Test]
        public void Test_CreateNewCannedMessage_Departments_UserIdIsNull()
        {
            var cm = new CannedMessage(null, DepartmentId, "create", "blah dsa fds");
            var r = m_managementService.CreateNewCannedMessage(CustomerId, UserId, cm.AsInfo());
            r.Status.StatusCode.Should().Be(CallResultStatusCode.Success);
        }

        [Test]
        public void Test_DeleteCannedMessage_UserId_DepartmentsIsNull()
        {
            var cm = new CannedMessage(UserId, null, "fd1", "blah");
            var r = m_managementService.CreateNewCannedMessage(CustomerId, UserId, cm.AsInfo());
            r.Status.StatusCode.Should().Be(CallResultStatusCode.Success);
        }

        [Test]
        public void Test_DeleteCannedMessage_Departments_UserIdIsNull()
        {
            var cm = new CannedMessage(null, DepartmentId, "fd1", "blah");
            var r = m_managementService.CreateNewCannedMessage(CustomerId, UserId, cm.AsInfo());
            r.Status.StatusCode.Should().Be(CallResultStatusCode.Success);
        }

        [Test]
        public void Test_UpdateCannedMessage_UserId_DepartmentsIsNull()
        {
            var cm = new CannedMessage(UserId, null, "update", "blah-blah dsa fds");
            var r1 = m_managementService.CreateNewCannedMessage(CustomerId, UserId, cm.AsInfo());
            var cm2 = new CannedMessageInfo
                {
                    UserId = UserId,
                    Key = "new key1",
                    Value = "new value2"
                };
            var r2 = m_managementService.UpdateCannedMessage(CustomerId, UserId, r1.CannedMessage.Id, cm2);
            r2.Status.StatusCode.Should().Be(CallResultStatusCode.Success);
        }

        [Test]
        public void Test_UpdateCannedMessage_Departments_UserIdIsNull()
        {
            var cm = new CannedMessage(null, DepartmentId, "update", "blah-blah dsa fds");
            var r1 = m_managementService.CreateNewCannedMessage(CustomerId, UserId, cm.AsInfo());
            var cm2 = new CannedMessageInfo
                {
                    DepartmentId = DepartmentId,
                    Key = "new key1",
                    Value = "new value2"
                };
            var r2 = m_managementService.UpdateCannedMessage(CustomerId, UserId, r1.CannedMessage.Id, cm2);
            r2.Status.StatusCode.Should().BeEquivalentTo(CallResultStatusCode.Success);
        }
    }
}