using System;
using System.Collections.Generic;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Impl;
using Com.O2Bionics.ChatService.Impl.Storage;
using Com.O2Bionics.ChatService.Objects;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.JsonSettings;
using FluentAssertions;
using log4net;
using NUnit.Framework;

namespace Com.O2Bionics.ChatService.Tests
{
    [TestFixture]
    public class UserStorageTests
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(UserStorageTests));

        public static readonly string ConnectionString =
            new JsonSettingsReader().ReadFromFile<TestSettings>().ChatServiceDatabase;

        [OneTimeSetUp]
        public void FuxtureSetUp()
        {
            new DatabaseManager(ConnectionString, false).RecreateSchema();
        }

        [Test]
        public void TestCreate()
        {
            new DatabaseManager(ConnectionString, false).ReloadData();

            var customerId = 1u;
            var now = DateTime.UtcNow;
            var nowProvider = new TestNowProvider(now);
            var dbFactory = new ChatDatabaseFactory(ConnectionString, true);
            var us = new UserStorage(nowProvider);

            var user = new User(
                customerId,
                ObjectStatus.Active,
                "test@test.com",
                "name1",
                "name2",
                "test",
                null,
                true,
                true,
                new HashSet<uint>(),
                new HashSet<uint>());

            var created = dbFactory.Query(db => us.CreateNew(db, user));
            created.Should().BeEquivalentTo(
                user,
                u => u
                    .Excluding(x => x.Id)
                    .Excluding(x => x.AddTimestampUtc)
                    .Excluding(x => x.UpdateTimestampUtc));
            created.Id.Should().BeGreaterThan(0);
            created.AddTimestampUtc.Should().BeCloseTo(now, 1000);
            created.UpdateTimestampUtc.Should().BeCloseTo(now, 1000);

            var found = dbFactory.Query(db => us.Get(db, customerId, created.Id));
            found.Should().BeEquivalentTo(
                created);
        }

        [Test]
        public void TestDelete()
        {
            new DatabaseManager(ConnectionString, false).ReloadData();

            var customerId = 1u;
            var now = DateTime.UtcNow;
            var nowProvider = new TestNowProvider(now);
            var dbFactory = new ChatDatabaseFactory(ConnectionString, true);
            var us = new UserStorage(nowProvider);

            var user = new User(
                customerId,
                ObjectStatus.Active,
                "test@test.com",
                "fname",
                "lname",
                "test",
                null,
                true,
                true,
                new HashSet<uint>(),
                new HashSet<uint>());
            var created = dbFactory.Query(db => us.CreateNew(db, user));

            var update = new User.UpdateInfo { Status = ObjectStatus.Deleted };
            var updated = dbFactory.Query(db => us.Update(db, customerId, created.Id, update, true));

            updated.Should().BeNull();

            var found = dbFactory.Query(db => us.Get(db, customerId, created.Id));
            found.Should().BeNull();
        }

        [Test]
        [Explicit]
        // 0 - 12 sec
        public void TestUpdatePerformance()
        {
            new DatabaseManager(ConnectionString, false).ReloadData();

            var customerId = 1u;

            var now = DateTime.UtcNow;
            var nowProvider = new TestNowProvider(now);
            var dbFactory = new ChatDatabaseFactory(ConnectionString, false);
            var us = new UserStorage(nowProvider);

            var deps = dbFactory.Query(
                db => db.DEPARTMENTs.Where(x => x.CUSTOMER_ID == customerId).Select(x => x.ID).ToList());

            var user = dbFactory.Query(
                db =>
                    {
                        var t = new User(
                            customerId,
                            ObjectStatus.Active,
                            "t1@asd.asd",
                            "FName",
                            "LName",
                            "test".ToPasswordHash(),
                            null,
                            false,
                            false,
                            new HashSet<uint>(deps),
                            new HashSet<uint>());
                        return us.CreateNew(db, t);
                    });

            for (var i = 0; i < 500; i++)
            {
                var update = new User.UpdateInfo
                    {
                        FirstName = $"FName {i}",
                        LastName = $"LName {i}",
                        IsAdmin = i % 2 == 0,
                        IsOwner = i % 2 == 1,
                        AgentDepartments = i % 3 == 0
                            ? new HashSet<uint>(deps)
                            : i % 3 == 1
                                ? new HashSet<uint>()
                                : null,
                        SupervisorDepartments = i % 3 == 1
                            ? new HashSet<uint>(deps)
                            : i % 3 == 2
                                ? new HashSet<uint>()
                                : null,
                    };
                dbFactory.Query(
                    db => us.Update(db, customerId, user.Id, update));
            }
        }
    }
}