using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Impl;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.JsonSettings;
using FluentAssertions;
using log4net;
using LinqToDB;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;

namespace Com.O2Bionics.ChatService.Tests
{
    // those tests are just to illustrate oracle/linq2db behavior
    [TestFixture]
    [Explicit]
    public class OracleDatabaseBehaviorTests
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(OracleDatabaseBehaviorTests));

        public static readonly string ConnectionString =
            new JsonSettingsReader().ReadFromFile<TestSettings>().ChatServiceDatabase;

        private readonly ChatDatabaseFactory m_dbFactory = new ChatDatabaseFactory(ConnectionString, true);

        [SetUp]
        public void SetUp()
        {
            new DatabaseManager(ConnectionString, false).RecreateSchema();
            new DatabaseManager(ConnectionString, false).ReloadData();
        }

        // shows that failed insert inside a transaction doesn't breaks all the transaction changes 
        [Test]
        public void TestSpecificInsertFailureInTransaction()
        {
            var maxId = m_dbFactory.Query(db => db.OBJECT_STATUS.Max(x => x.OBJECT_STATUS_ID));

            m_dbFactory.Query(
                db =>
                    {
                        try
                        {
                            db.Insert(new OBJECT_STATUS { OBJECT_STATUS_ID = (sbyte)(maxId + 4), NAME = "test4" });
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }

                        try
                        {
                            // this shouldn't be added because of the duplicate id
                            db.Insert(new OBJECT_STATUS { OBJECT_STATUS_ID = (sbyte)(maxId + 4), NAME = "test4-1" });
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }

                        try
                        {
                            db.Insert(new OBJECT_STATUS { OBJECT_STATUS_ID = (sbyte)(maxId + 5), NAME = "test5" });
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    });

            var newItems = m_dbFactory.Query(
                db =>
                    db.OBJECT_STATUS
                        .Where(x => x.OBJECT_STATUS_ID > maxId)
                        .ToList());
            Console.WriteLine(newItems.JsonStringify2());
            newItems.Should().HaveCount(2);
        }

        // linq2db fails selecting an sbyte typed field
        [Test]
        public void TestSbyteFieldSelectFailsWithLinq2Db()
        {
            m_dbFactory.Query(
                db =>
                    {
                        var t = new OBJECT_STATUS { OBJECT_STATUS_ID = 8, NAME = "A", DESCRIPTION = "B" };
                        db.Insert(t);

                        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                        Action a = () => db.OBJECT_STATUS.FirstOrDefault(x => 8 == x.OBJECT_STATUS_ID);
                        a.Should().Throw<InvalidOperationException>()
                            .WithMessage("Operation is not valid due to the current state of the object.");
                    });
        }

        // inserting to a table locks the table until the transaction end
        [Test]
        [Timeout(60000)]
        public void TestDatabaseTableLock(
            [Values(IsolationLevel.ReadCommitted, IsolationLevel.Serializable)]
            IsolationLevel isolationLevel)
        {
            var thread1SleepBeforeCommit = TimeSpan.FromSeconds(6);
            var commandTimeout = thread1SleepBeforeCommit.Add(TimeSpan.FromSeconds(10));
            var now = DateTime.UtcNow;

            var startThreadsEvent = new ManualResetEventSlim(false);
            var thread1AfterInsertBeforeCommitEvent = new ManualResetEventSlim(false);

            CUSTOMER CreateRecord()
            {
                return new CUSTOMER
                    {
                        ID = 200u,
                        CREATE_TIMESTAMP = now,
                        UPDATE_TIMESTAMP = now,
                        STATUS_ID = 1,
                        NAME = "test1",
                        DOMAINS = "domains1",
                    };
            }

            void Thread1()
            {
                Wait(startThreadsEvent, nameof(startThreadsEvent));

                using (var db = new ChatDatabase(ConnectionString))
                {
                    db.CommandTimeout = (int)commandTimeout.TotalSeconds;
                    db.BeginTransaction(isolationLevel);

                    var customer = CreateRecord();
                    m_log.Debug("before insert");
                    db.Insert(customer);
                    m_log.Debug("after insert");

                    Set(thread1AfterInsertBeforeCommitEvent, nameof(thread1AfterInsertBeforeCommitEvent));

                    m_log.Debug("before sleep");
                    Thread.Sleep(thread1SleepBeforeCommit);
                    m_log.Debug("after sleep");

                    db.CommitTransaction();
                }
            }

            Exception thread2Exception = null;
            Stopwatch thread2Stopwatch = null;

            void Thread2()
            {
                Wait(startThreadsEvent, nameof(startThreadsEvent));

                using (var db = new ChatDatabase(ConnectionString))
                {
                    db.CommandTimeout = (int)commandTimeout.TotalSeconds;
                    db.BeginTransaction(isolationLevel);

                    Wait(thread1AfterInsertBeforeCommitEvent, nameof(thread1AfterInsertBeforeCommitEvent));

                    var customer = CreateRecord();
                    thread2Stopwatch = Stopwatch.StartNew();
                    try
                    {
                        m_log.Debug("before insert");
                        db.Insert(customer);
                        m_log.Debug("after insert");
                        db.CommitTransaction();
                    }
                    catch (Exception e)
                    {
                        thread2Exception = e;
                        m_log.DebugFormat("exception: {0}", e);
                    }
                    finally
                    {
                        thread2Stopwatch.Stop();
                    }
                }
            }

            m_log.DebugFormat("cs: {0}", ConnectionString);

            var th1 = StartBackgroundThread("th1", Thread1);
            var th2 = StartBackgroundThread("th2", Thread2);

            Set(startThreadsEvent, nameof(startThreadsEvent));

            th1.Join();
            th2.Join();

            m_log.DebugFormat("thread2Stopwatch: {0}", thread2Stopwatch.Elapsed);

            thread2Exception.Should().BeOfType<OracleException>()
                .Which.Number.Should().Be(1);
            thread2Stopwatch.Elapsed.Should().BeGreaterOrEqualTo(thread1SleepBeforeCommit);
        }

        private static Thread StartBackgroundThread(string name, Action a)
        {
            var thread = new Thread(
                () =>
                    {
                        try
                        {
                            a();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }) { IsBackground = true, Name = name, };
            thread.Start();
            return thread;
        }

        private static void Wait(ManualResetEventSlim ev, string name)
        {
            m_log.Debug($"wait {name}");
            ev.Wait();
            m_log.Debug($"got {name}");
        }

        private static void Set(ManualResetEventSlim ev, string name)
        {
            ev.Set();
            m_log.Debug($"set {name}");
        }
    }
}