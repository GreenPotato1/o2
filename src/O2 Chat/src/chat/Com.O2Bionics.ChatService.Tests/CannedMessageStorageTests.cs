using NUnit.Framework;
using Com.O2Bionics.ChatService.Impl.Storage;
using log4net;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Impl;
using System;
using System.Collections.Generic;
using Com.O2Bionics.ChatService.Objects;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.JsonSettings;
using FluentAssertions;

namespace Com.O2Bionics.ChatService.Tests
{
    [TestFixture]
    public class CannedMessageStorageTests
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(CannedMessageStorage));

        public static readonly string ConnectionString =
            new JsonSettingsReader().ReadFromFile<TestSettings>().ChatServiceDatabase;

        private const uint CustomerId = 1u;
        private const uint UserId = 9u;
        private const uint DepartmentId = 2;

        private DateTime m_now;
        private INowProvider m_nowProvider;
        private ChatDatabaseFactory m_dbFactory;
        private ICannedMessageStorage m_storage;

        [SetUp]
        public void SetUp()
        {
            new DatabaseManager(ConnectionString, false).RecreateSchema();
            new DatabaseManager(ConnectionString, false).ReloadData();

            m_now = DateTime.UtcNow;
            m_nowProvider = new TestNowProvider(m_now);
            m_dbFactory = new ChatDatabaseFactory(ConnectionString, true);
            m_storage = new CannedMessageStorage(m_nowProvider);
        }

        [Test]
        public void Test_Create_UserMessage()
        {
            var obj = new CannedMessage(UserId, null, "/Denis", "My name is Denis Prokhorchik");

            var created = m_dbFactory.Query(db => m_storage.CreateNew(db, CustomerId, obj));
            created.Should().BeEquivalentTo(
                obj,
                x => x
                    .Excluding(y => y.Id)
                    .Excluding(y => y.AddTimestampUtc)
                    .Excluding(y => y.UpdateTimestampUtc));
            created.Id.Should().BeGreaterThan(0);
            created.AddTimestampUtc.Should().BeCloseTo(m_now, 1000);
            created.UpdateTimestampUtc.Should().BeCloseTo(m_now, 1000);

            var found = m_dbFactory.Query(db => m_storage.GetMany(db, CustomerId, UserId, null));
            found.Should().BeEquivalentTo(new List<CannedMessage> { created });
        }

        [Test]
        public void Test_Update_UserMessage()
        {
            var obj = new CannedMessage(UserId, null, "/Denis", "My name is Denis Prokhorchik");
            var created = m_dbFactory.Query(db => m_storage.CreateNew(db, CustomerId, obj));
            var update = new CannedMessage.UpdateInfo { MessageKey = "/Dionis2", MessageValue = "Bla-bla-bla" };
            var updated = m_dbFactory.Query(db => m_storage.Update(db, CustomerId, created.Id, update));
            var found = m_dbFactory.Query(db => m_storage.GetMany(db, CustomerId, UserId, null));
            found.Should().BeEquivalentTo(new List<CannedMessage> { updated });
        }

        [Test]
        public void Test_Delete_UserMessage()
        {
            var obj = new CannedMessage(UserId, null, "/Denis", "My name is Denis Prokhorchik");
            var created = m_dbFactory.Query(db => m_storage.CreateNew(db, CustomerId, obj));
            m_dbFactory.Query(db => m_storage.Delete(db, CustomerId, created.Id));
            var found = m_dbFactory.Query(db => m_storage.GetMany(db, CustomerId, UserId, null));
            found.Should().BeEmpty();
        }

        [Test]
        public void Test_Create_DepartmentMessage()
        {
            var obj = new CannedMessage(null, DepartmentId, "/Denis", "My name is Denis Prokhorchik");

            var created = m_dbFactory.Query(db => m_storage.CreateNew(db, CustomerId, obj));
            created.Should().BeEquivalentTo(
                obj,
                x => x
                    .Excluding(y => y.Id)
                    .Excluding(y => y.AddTimestampUtc)
                    .Excluding(y => y.UpdateTimestampUtc));
            created.Id.Should().BeGreaterThan(0);
            created.AddTimestampUtc.Should().BeCloseTo(m_now, 1000);
            created.UpdateTimestampUtc.Should().BeCloseTo(m_now, 1000);

            var found = m_dbFactory.Query(
                db => m_storage.GetMany(db, CustomerId, null, new HashSet<uint> { DepartmentId }));
            found.Should().BeEquivalentTo(new List<CannedMessage> { created });
        }

        [Test]
        public void Test_Update_DepartmentMessage()
        {
            var obj = new CannedMessage(null, DepartmentId, "/Denis", "My name is Denis Prokhorchik");
            var created = m_dbFactory.Query(db => m_storage.CreateNew(db, CustomerId, obj));
            var update = new CannedMessage.UpdateInfo { MessageKey = "/Dionis", MessageValue = "Bla-bla-bla" };
            var updated = m_dbFactory.Query(db => m_storage.Update(db, CustomerId, created.Id, update));
            var found = m_dbFactory.Query(
                db => m_storage.GetMany(db, CustomerId, null, new HashSet<uint> { DepartmentId }));
            found.Should().BeEquivalentTo(new List<CannedMessage> { updated });
        }

        [Test]
        public void Test_Delete_DepartmentMessage()
        {
            var obj = new CannedMessage(null, DepartmentId, "/Denis", "My name is Denis Prokhorchik");
            var created = m_dbFactory.Query(db => m_storage.CreateNew(db, CustomerId, obj));
            m_dbFactory.Query(db => m_storage.Delete(db, CustomerId, created.Id));
            var found = m_dbFactory.Query(
                db => m_storage.GetMany(db, CustomerId, null, new HashSet<uint> { DepartmentId }));
            found.Should().BeEmpty();
        }
    }
}