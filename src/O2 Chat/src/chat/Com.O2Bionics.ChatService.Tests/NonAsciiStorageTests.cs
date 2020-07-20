using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Impl;
using Com.O2Bionics.ChatService.Objects;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils.JsonSettings;
using FluentAssertions;
using LinqToDB;
using LinqToDB.Data;
using log4net;
using NUnit.Framework;

namespace Com.O2Bionics.ChatService.Tests
{
    [TestFixture]
    public class NonAsciiStorageTests
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(NonAsciiStorageTests));

        public static readonly string ConnectionString =
            new JsonSettingsReader().ReadFromFile<TestSettings>().ChatServiceDatabase;

        private readonly ChatDatabaseFactory m_dbFactory = new ChatDatabaseFactory(ConnectionString, true);

        [SetUp]
        public void SetUp()
        {
            new DatabaseManager(ConnectionString, false).RecreateSchema();
            new DatabaseManager(ConnectionString, false).ReloadData();
        }

        [Test]
        public void TestNonAsciiCharsChatEventText()
        {
            m_dbFactory.Query(
                db =>
                    {
                        var sessionId = db.CHAT_SESSION.Any() ? db.CHAT_SESSION.Max(x => x.CHAT_SESSION_ID) + 1 : 1;
                        var eventId = db.CHAT_EVENT.Any() ? db.CHAT_EVENT.Max(x => x.CHAT_EVENT_ID) + 1 : 1;

                        var cs = new CHAT_SESSION
                            {
                                CHAT_SESSION_ID = sessionId,
                                CUSTOMER_ID = 1,
                                CHAT_SESSION_STATUS_ID = 0,
                                IS_OFFLINE = 0,
                                ADD_TIMESTAMP = DateTime.UtcNow,
                            };
                        db.Insert(cs);

                        // not works
                        // var ev = new CHAT_EVENT
                        //     {
                        //         CHAT_SESSION_ID = sessionId,
                        //         CHAT_EVENT_ID = eventId,
                        //         CHAT_EVENT_TYPE_ID = 1,
                        //         TIMESTAMP = DateTime.UtcNow,
                        //         TEXT = text,
                        //     };
                        // db.Insert(ev);

                        const string text = "asd фываФЫВА ⱣⱤⱠ";

                        db.Execute(
                            @"insert into CHAT_EVENT
                                (CHAT_SESSION_ID, CHAT_EVENT_ID, CHAT_EVENT_TYPE_ID, TIMESTAMP, TEXT) 
                                values 
                                (:sessionId, :eventId, :eventTypeId, :timestamp, :text)",
                            new DataParameter("sessionId", sessionId),
                            new DataParameter("eventId", eventId),
                            new DataParameter("eventTypeId", 1),
                            new DataParameter("timestamp", DateTime.UtcNow),
                            new DataParameter("text", text, DataType.NText));

                        var ev = db.CHAT_EVENT.FirstOrDefault(x => x.CHAT_EVENT_ID == eventId);
                        ev.Should().NotBeNull();
                        ev.TEXT.Should().Be(text);
                    });
        }

        [Test]
        public void TestNonAsciiCharsDepartment()
        {
            m_dbFactory.Query(
                db =>
                    {
                        var customerId = db.CUSTOMERs.Select(x => x.ID).FirstOrDefault();

                        var obj1 = new Department(customerId, "фыва", "йцук", true);
                        var obj11 = Department.Insert(db, DateTime.UtcNow, obj1);
                        obj11.Should().NotBeSameAs(obj1);
                        obj11.Should().BeEquivalentTo(
                            obj1,
                            x => x
                                .Excluding(y => y.Id)
                                .Excluding(y => y.AddTimestampUtc)
                                .Excluding(y => y.UpdateTimestampUtc));

                        var obj2 = new Department(customerId, "фыва 2", "йцук 3", false);
                        var obj21 = Department.Insert(db, DateTime.UtcNow, obj2);

                        db.DEPARTMENTs.FirstOrDefault(x => x.ID == obj11.Id)
                            .Should().BeEquivalentTo(
                                new
                                    {
                                        DEPARTMENT_ID = obj11.Id,
                                        NAME = obj1.Name,
                                        DESCRIPTION = obj1.Description,
                                    },
                                x => x.ExcludingMissingMembers());

                        db.DEPARTMENTs.FirstOrDefault(x => x.ID == obj21.Id)
                            .Should().BeEquivalentTo(
                                new
                                    {
                                        DEPARTMENT_ID = obj21.Id,
                                        NAME = obj2.Name,
                                        DESCRIPTION = obj2.Description,
                                    },
                                x => x.ExcludingMissingMembers());

                        var update = new Department.UpdateInfo
                            {
                                Name = "фыв ввв",
                                Description = "зщзщлоВВВ",
                            };
                        Department.Update(db, DateTime.UtcNow, obj11.Id, update);
                        db.DEPARTMENTs.FirstOrDefault(x => x.ID == obj11.Id)
                            .Should().BeEquivalentTo(
                                new
                                    {
                                        NAME = update.Name,
                                        DESCRIPTION = update.Description,
                                    },
                                x => x.ExcludingMissingMembers());
                    });
        }

        [Test]
        public void TestNonAsciiCharsUser()
        {
            m_dbFactory.Query(
                db =>
                    {
                        var customerId = db.CUSTOMERs.Select(x => x.ID).FirstOrDefault();

                        var dept = new Department(customerId, "test1", "asdf", true);
                        var depId = Department.Insert(db, DateTime.UtcNow, dept).Id;

                        var obj1 = new User(
                            customerId,
                            ObjectStatus.Active,
                            "asd@asd.asd",
                            "ФЫвйцу",
                            "длодлодл",
                            "asd",
                            null,
                            true,
                            true,
                            new HashSet<uint>(),
                            new HashSet<uint>());
                        var obj11 = User.Insert(db, DateTime.UtcNow, obj1);
                        obj11.Should().NotBeSameAs(obj1);
                        obj11.Should().BeEquivalentTo(
                            obj1,
                            x => x
                                .Excluding(y => y.Id)
                                .Excluding(y => y.AddTimestampUtc)
                                .Excluding(y => y.UpdateTimestampUtc));

                        var obj2 = new User(
                            customerId,
                            ObjectStatus.Active,
                            "asd2@asd.asd",
                            "ФЫвдолойцу",
                            "22длодывалодл",
                            "22asd",
                            null,
                            true,
                            false,
                            new HashSet<uint>(),
                            new HashSet<uint>());
                        var obj21 = User.Insert(db, DateTime.UtcNow, obj2);

                        db.CUSTOMER_USER.FirstOrDefault(x => x.ID == obj11.Id)
                            .Should().BeEquivalentTo(
                                new
                                    {
                                        USERID = obj11.Id,
                                        FIRST_NAME = obj1.FirstName,
                                        LAST_NAME = obj1.LastName,
                                    },
                                x => x.ExcludingMissingMembers());

                        db.CUSTOMER_USER.FirstOrDefault(x => x.ID == obj21.Id)
                            .Should().BeEquivalentTo(
                                new
                                    {
                                        USERID = obj21.Id,
                                        FIRST_NAME = obj2.FirstName,
                                        LAST_NAME = obj2.LastName,
                                    },
                                x => x.ExcludingMissingMembers());

                        var update = new User.UpdateInfo
                            {
                                Email = "email2@asd.asd",
                                FirstName = "ллдлд",
                                LastName = "нгщшщВВВ",
                                Status = ObjectStatus.Disabled,
                                PasswordHash = "asdasdasd1",
                                IsAdmin = true,
                                IsOwner = true,
                                AgentDepartments = new HashSet<uint>(new[] { depId }),
                                SupervisorDepartments = new HashSet<uint>(new[] { depId }),
                            };
                        User.Update(db, DateTime.UtcNow, obj11.Id, update);
                        db.CUSTOMER_USER.FirstOrDefault(x => x.ID == obj11.Id)
                            .Should().BeEquivalentTo(
                                new
                                    {
                                        FIRST_NAME = update.FirstName,
                                        LAST_NAME = update.LastName,
                                    },
                                x => x.ExcludingMissingMembers());
                    });
        }

        [Test]
        public void TestNonAsciiCharsVisitor()
        {
            m_dbFactory.Query(
                db =>
                    {
                        var customerId = db.CUSTOMERs.Select(x => x.ID).FirstOrDefault();
                        var lastVisitorId = db.VISITORs.Any() ? (ulong)db.VISITORs.Max(x => x.VISITOR_ID) : 0;

                        lastVisitorId++;
                        var obj = new Visitor(customerId, lastVisitorId, DateTime.UtcNow)
                            {
                                Email = "asd@asd.asd",
                                MediaSupport = null,
                                Name = "фывфыв",
                                Phone = "124324332",
                                TranscriptMode = null,
                                UpdateTimestampUtc = DateTime.UtcNow,
                            };
                        Visitor.Insert(db, obj);

                        lastVisitorId++;
                        var obj2 = new Visitor(customerId, lastVisitorId, DateTime.UtcNow)
                            {
                                Email = "asd2@asd.asd",
                                MediaSupport = MediaSupport.Video,
                                Name = "йцуйцу!",
                                Phone = "123asdf",
                                TranscriptMode = VisitorSendTranscriptMode.Always,
                                UpdateTimestampUtc = DateTime.UtcNow,
                            };
                        Visitor.Insert(db, obj2);

                        db.VISITORs.FirstOrDefault(x => x.VISITOR_ID == obj.Id)
                            .Should().BeEquivalentTo(
                                new
                                    {
                                        NAME = obj.Name,
                                        PHONE = obj.Phone,
                                        TRANSCRIPT_MODE = (sbyte?)obj.TranscriptMode,
                                        MEDIA_SUPPORT = (sbyte?)obj.MediaSupport,
                                    },
                                x => x.ExcludingMissingMembers());

                        db.VISITORs.FirstOrDefault(x => x.VISITOR_ID == obj2.Id)
                            .Should().BeEquivalentTo(
                                new
                                    {
                                        NAME = obj2.Name,
                                        PHONE = obj2.Phone,
                                        TRANSCRIPT_MODE = (sbyte?)obj2.TranscriptMode,
                                        MEDIA_SUPPORT = (sbyte?)obj2.MediaSupport,
                                    },
                                x => x.ExcludingMissingMembers());

                        Visitor.Update(db, obj, obj2);
                        db.VISITORs.FirstOrDefault(x => x.VISITOR_ID == obj2.Id)
                            .Should().BeEquivalentTo(
                                new
                                    {
                                        NAME = obj2.Name,
                                        PHONE = obj2.Phone,
                                        TRANSCRIPT_MODE = (sbyte?)obj2.TranscriptMode,
                                        MEDIA_SUPPORT = (sbyte?)obj2.MediaSupport,
                                    },
                                x => x.ExcludingMissingMembers());
                    });
        }

        [Test]
        [Explicit]
        public void TestParallelAccess()
        {
            var dbf = new ChatDatabaseFactory(ConnectionString, false);

            var threads = Enumerable.Range(0, 10).Select(
                i => new Thread(
                    () =>
                        {
                            m_log.Debug("started");
                            var rand = new Random();

                            for (var j = 0; j < 1000; j++)
                            {
                                var text = "test-" + rand.Next(100);

                                try
                                {
                                    using (var ctx = dbf.CreateContext())
                                    {
//                                        var a1 = ctx.Db.VISITOR_USER_AGENT
//                                            .Where(x => x.USER_AGENT == text)
//                                            .Select(x => x.VISITOR_USER_AGENT_SKEY)
//                                            .FirstOrDefault();
//                                        if (a1 == default(decimal))
//                                        {
//                                            a1 = Convert.ToDecimal(ctx.Db.InsertWithIdentity(new VISITOR_USER_AGENT { USER_AGENT = text }));
//                                        }

//                                        m_log.DebugFormat("{0} - {1}", i, a1);
                                        ctx.Commit();
                                    }
                                }
                                catch (Exception e)
                                {
                                    m_log.Error($"Failed {i} {text}", e);
                                }
                            }

                            m_log.Debug("completed");
                        }) { IsBackground = true }).ToList();
            foreach (var t in threads) t.Start();
            foreach (var t in threads) t.Join();
        }
    }
}