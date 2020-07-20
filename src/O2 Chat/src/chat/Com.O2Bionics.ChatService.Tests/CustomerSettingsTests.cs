using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Impl;
using Com.O2Bionics.ChatService.Impl.Storage;
using FluentAssertions;
using log4net;
using LinqToDB;
using NUnit.Framework;

namespace Com.O2Bionics.ChatService.Tests
{
    [TestFixture]
    public sealed class CustomerSettingsTests : DatabaseTestBase
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(CustomerSettingsTests));

        private readonly ChatDatabaseFactory m_dbFactory = new ChatDatabaseFactory(ConnectionString);

        [Test]
        public void CustomizationSettingsTest()
        {
            var existingProperties = m_dbFactory.Query(db => db.PROPERTY_BAG.ToList());

            var customerId = 1u;
            var testString = "{name: \"value\"}";

            try
            {
                using (var settingsStorage = new SettingsStorage())
                {
                    using (var dc = m_dbFactory.CreateContext())
                    {
                        settingsStorage.Load(dc);
                        dc.Commit();
                    }

                    var settings = settingsStorage.GetCustomerSettings(customerId);
                    settings.Customization.Should().Be(settings.CustomizationDefault);


                    var wrtblSettings = settingsStorage.GetWritableCustomerSettings(customerId);
                    wrtblSettings.Customization.Should().Be(wrtblSettings.CustomizationDefault);

                    wrtblSettings.Customization = testString;

                    using (var dc = m_dbFactory.CreateContext())
                    {
                        settingsStorage.SaveCustomerSettings(dc, customerId, wrtblSettings);
                        dc.Commit();
                    }

                    var newSettings = settingsStorage.GetCustomerSettings(customerId);
                    newSettings.Customization.Should().Be(testString);
                }

                using (var settingsStorage = new SettingsStorage())
                {
                    using (var dc = m_dbFactory.CreateContext())
                    {
                        settingsStorage.Load(dc);
                        dc.Commit();
                    }

                    var newSettings = settingsStorage.GetCustomerSettings(customerId);
                    newSettings.Customization.Should().Be(testString);
                }
            }
            finally
            {
                m_dbFactory.Query(
                    db =>
                        {
                            var ids = existingProperties.Select(x => x.PROPERTY_BAG_SKEY).ToList();
                            var newProperties = db.PROPERTY_BAG.Where(x => !ids.Contains(x.PROPERTY_BAG_SKEY)).ToList();
                            //newProperties.Should().OnlyContain(x => x.USERID == customerId);
                            db.PROPERTY_BAG.Delete();
                            foreach (var x in existingProperties) db.InsertWithIdentity(x);
                        });
            }
        }

        [Test]
        public void ServiceSettingsAccessTest()
        {
            var existingProperties = m_dbFactory.Query(db => db.PROPERTY_BAG.ToList());

            try
            {
                using (var settingsStorage = new SettingsStorage())
                {
                    using (var dc = m_dbFactory.CreateContext())
                    {
                        settingsStorage.Load(dc);
                        dc.Commit();
                    }

                    var settings1 = settingsStorage.GetServiceSettings();
                    settings1.StringProperty.Should().Be(settings1.StringPropertyDefault);

                    var settings2 = settingsStorage.GetWritableServiceSettings();
                    settings2.StringProperty.Should().Be(settings2.StringPropertyDefault);

                    settings2.StringProperty = "AAA";
                    using (var dc = m_dbFactory.CreateContext())
                    {
                        settingsStorage.SaveServiceSettings(dc, settings2);
                        dc.Commit();
                    }

                    var settings3 = settingsStorage.GetServiceSettings();
                    settings3.StringProperty.Should().Be("AAA");
                }

                using (var settingsStorage = new SettingsStorage())
                {
                    using (var dc = m_dbFactory.CreateContext())
                    {
                        settingsStorage.Load(dc);
                        dc.Commit();
                    }

                    var settings4 = settingsStorage.GetServiceSettings();
                    settings4.StringProperty.Should().Be("AAA");
                }
            }
            finally
            {
                m_dbFactory.Query(
                    db =>
                        {
                            var ids = existingProperties.Select(x => x.PROPERTY_BAG_SKEY).ToList();
                            var newProperties = db.PROPERTY_BAG.Where(x => !ids.Contains(x.PROPERTY_BAG_SKEY)).ToList();
                            newProperties.Should().OnlyContain(x => x.CUSTOMER_ID == null && x.USER_ID == null);
                            db.PROPERTY_BAG.Delete();
                            foreach (var x in existingProperties) db.InsertWithIdentity(x);
                        });
            }
        }

        [Test]
        public void CustomerSettingsAccessTest()
        {
            var now = DateTime.UtcNow;

            var customer = m_dbFactory.Query(
                db =>
                    {
                        var newCustomerId = db.CUSTOMERs.Any()
                            ? db.CUSTOMERs.Max(x => x.ID) + 1
                            : 1;
                        var c = DatabaseObjectHelper.Customer(now, newCustomerId, "test name", "domain.com");
                        db.Insert(c);
                        return c;
                    });

            try
            {
                using (var settingsStorage = new SettingsStorage())
                {
                    using (var dc = m_dbFactory.CreateContext())
                    {
                        settingsStorage.Load(dc);
                        dc.Commit();
                    }

                    var settings1 = settingsStorage.GetCustomerSettings(customer.ID);
                    settings1.IsProactiveChatEnabled.Should().Be(settings1.IsProactiveChatEnabledDefault);

                    var settings2 = settingsStorage.GetWritableCustomerSettings(customer.ID);
                    settings2.IsProactiveChatEnabled.Should().Be(settings2.IsProactiveChatEnabledDefault);

                    settings2.IsProactiveChatEnabled = !settings2.IsProactiveChatEnabledDefault;
                    using (var dc = m_dbFactory.CreateContext())
                    {
                        settingsStorage.SaveCustomerSettings(dc, customer.ID, settings2);
                        dc.Commit();
                    }

                    var settings3 = settingsStorage.GetCustomerSettings(customer.ID);
                    settings3.IsProactiveChatEnabled.Should().Be(!settings3.IsProactiveChatEnabledDefault);
                }

                using (var settingsStorage = new SettingsStorage())
                {
                    using (var dc = m_dbFactory.CreateContext())
                    {
                        settingsStorage.Load(dc);
                        dc.Commit();
                    }

                    var settings4 = settingsStorage.GetCustomerSettings(customer.ID);
                    settings4.IsProactiveChatEnabled.Should().Be(!settings4.IsProactiveChatEnabledDefault);
                }
            }
            finally
            {
                m_dbFactory.Query(
                    db =>
                        {
                            db.PROPERTY_BAG.Where(x => x.CUSTOMER_ID == customer.ID).Delete();
                            db.CUSTOMERs.Where(x => x.ID == customer.ID).Delete();
                        });
            }
        }

        [Test]
        [Explicit]
        public void CustomerSettingsConcurrentAccessTest(
            [Values(0, 1, 4)] int writers,
            [Values(0, 1, 10)] int readers)
        {
            Assume.That(readers != 0 || writers != 0);

            var now = DateTime.UtcNow;
            var customer = m_dbFactory.Query(
                db =>
                    {
                        var newCustomerId = db.CUSTOMERs.Any()
                            ? db.CUSTOMERs.Max(x => x.ID) + 1
                            : 1;
                        var c = DatabaseObjectHelper.Customer(now, newCustomerId, "test name", "domain.com");
                        db.Insert(c);
                        return c;
                    });

            try
            {
                using (var settingsStorage = new SettingsStorage())
                {
                    using (var dc = m_dbFactory.CreateContext())
                    {
                        settingsStorage.Load(dc);
                        dc.Commit();
                    }

                    var writerThreads = CreateThreads(
                        writers,
                        "writer",
                        () =>
                            {
                                m_log.Info("started");
                                for (var i = 0; i < 1000; i++)
                                {
                                    var settings = settingsStorage.GetWritableCustomerSettings(customer.ID);
                                    settings.IsProactiveChatEnabled = i % 2 == 0;
                                    settings.MaxPagesInHistory = i;
                                    using (var dc = m_dbFactory.CreateContext())
                                    {
                                        settingsStorage.SaveCustomerSettings(dc, customer.ID, settings);
                                        dc.Commit();
                                    }
                                }

                                m_log.Info("finished");
                            });

                    var readerThreads = CreateThreads(
                        readers,
                        "reader",
                        () =>
                            {
                                m_log.Info("started");
                                for (var i = 0; i < 2000; i++)
                                {
                                    var settings = settingsStorage.GetCustomerSettings(customer.ID);
                                    var a = settings.IsProactiveChatEnabled;
                                    var b = settings.MaxPagesInHistory;
                                    var d = Math.Abs(b + (a ? 1 : 2));
                                }

                                m_log.Info("finished");
                            });

                    var threads = readerThreads.Concat(writerThreads).ToList();
                    foreach (var thread in threads) thread.Start();
                    foreach (var thread in threads) thread.Join();
                }
            }
            finally
            {
                m_dbFactory.Query(
                    db =>
                        {
                            db.PROPERTY_BAG.Where(x => x.CUSTOMER_ID == customer.ID).Delete();
                            db.CUSTOMERs.Where(x => x.ID == customer.ID).Delete();
                        });
            }
        }

        private static List<Thread> CreateThreads(int count, string name, Action action)
        {
            return Enumerable.Range(0, count)
                .Select(i => new Thread(x => action()) { IsBackground = true, Name = name + " " + i })
                .ToList();
        }
    }
}