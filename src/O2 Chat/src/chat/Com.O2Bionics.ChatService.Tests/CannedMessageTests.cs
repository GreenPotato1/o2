using System;
using System.Collections.Generic;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Impl;
using Com.O2Bionics.ChatService.Impl.Storage;
using Com.O2Bionics.ChatService.Objects;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils.JsonSettings;
using FluentAssertions;
using log4net;
using NUnit.Framework;

namespace Com.O2Bionics.ChatService.Tests
{
    [TestFixture]
    public class CannedMessageTests
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(CannedMessageStorage));

        public static readonly string ConnectionString =
            new JsonSettingsReader().ReadFromFile<TestSettings>().ChatServiceDatabase;

        private const uint CustomerId = 1u;
        private const uint UserId = 9u;
        private const uint DepartmentId = 4;

        private DateTime m_now;
        private ChatDatabaseFactory m_dbf;

        [SetUp]
        public void SetUp()
        {
            new DatabaseManager(ConnectionString, false).RecreateSchema();
            new DatabaseManager(ConnectionString, false).ReloadData();

            m_now = DateTime.UtcNow;
            m_dbf = new ChatDatabaseFactory(ConnectionString, true);
        }

        [Test]
        public void Test_Insert_Get_UserMessage()
        {
            var obj = new CannedMessage(UserId, null, "Insert", "Message insert");
            var inserted = m_dbf.Query(db => CannedMessage.Insert(db, m_now, CustomerId, obj));
            var read = m_dbf.Query(db => CannedMessage.Get(db, inserted.Id));
            inserted.Should().BeEquivalentTo(read);
        }

        [Test]
        public void Test_Insert_Get_DepartmentMessage()
        {
            var obj = new CannedMessage(null, DepartmentId, "Insert", "Message insert");
            var inserted = m_dbf.Query(db => CannedMessage.Insert(db, m_now, CustomerId, obj));
            var read = m_dbf.Query(db => CannedMessage.Get(db, inserted.Id));
            inserted.Should().BeEquivalentTo(read);
        }

        [Test]
        public void Test_Insert_GetCustomerData_UserMessages()
        {
            var list = new List<CannedMessage>();
            for (var i = 0; i < 3; i++)
            {
                var obj = new CannedMessage(UserId, null, "key" + i, "My name is Denis Prokhorchik " + i);
                var inserted = m_dbf.Query(db => CannedMessage.Insert(db, m_now, CustomerId, obj));

                inserted.Should().NotBeNull();
                list.Add(inserted);
            }

            var read = m_dbf.Query(db => CannedMessage.GetCustomerData(db, CustomerId));
            read.Should().BeEquivalentTo(list);
        }

        [Test]
        public void Test_Insert_GetCustomerData_DepartmentMessages()
        {
            var list = new List<CannedMessage>();
            for (var i = 0; i < 3; i++)
            {
                var obj = new CannedMessage(null, DepartmentId, "key" + i, "My name is Denis Prokhorchik " + i);
                var inserted = m_dbf.Query(db => CannedMessage.Insert(db, m_now, CustomerId, obj));

                inserted.Should().NotBeNull();
                list.Add(inserted);
            }

            var read = m_dbf.Query(db => CannedMessage.GetCustomerData(db, CustomerId));
            read.Should().BeEquivalentTo(list);
        }

        [Test]
        public void Test_Insert_Delete_Get_UserMessage()
        {
            var obj = new CannedMessage(UserId, null, "Insert", "Message insert");
            var inserted = m_dbf.Query(db => CannedMessage.Insert(db, m_now, CustomerId, obj));
            m_dbf.Query(db => CannedMessage.Delete(db, inserted.Id));
            var read = m_dbf.Query(db => CannedMessage.Get(db, inserted.Id));
            read.Should().BeNull();
        }

        [Test]
        public void Test_Insert_Delete_Get_DepartmentMessage()
        {
            var obj = new CannedMessage(null, DepartmentId, "Insert", "Message insert");
            var inserted = m_dbf.Query(db => CannedMessage.Insert(db, m_now, CustomerId, obj));
            m_dbf.Query(db => CannedMessage.Delete(db, inserted.Id));
            var read = m_dbf.Query(db => CannedMessage.Get(db, inserted.Id));
            read.Should().BeNull();
        }
    }
}