using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Com.O2Bionics.AuditTrail.Client;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.ChatService.Contract.Widget;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Impl.AuditTrail;
using Com.O2Bionics.ChatService.Impl.Storage;
using Com.O2Bionics.ChatService.Tests.Mocks;
using Com.O2Bionics.ChatService.Tests.WidgetLoadLimiter.Mocks;
using Com.O2Bionics.FeatureService.Constants;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using FluentAssertions;
using JetBrains.Annotations;
using LinqToDB.Data;
using NSubstitute;
using NUnit.Framework;
using CountIsExceeded = System.Collections.Generic.KeyValuePair<long, bool>;

namespace Com.O2Bionics.ChatService.Tests.WidgetLoadLimiter
{
    [TestFixture]
    public sealed class PageLoadServiceTests : WidgetTestBase
    {
        private const int CountersDbUpdateDelta = 5;
        private const int DailyLoads = CountersDbUpdateDelta * 3 + 1;
        private const int CountersDbUpdateMinimumIntervalSeconds = 50;

        private readonly WidgetLoadLimiterSettings m_widgetLoadLimiterSettings = new WidgetLoadLimiterSettings
            {
                CountersDbUpdateDelta = CountersDbUpdateDelta,
                CountersDbUpdateMinimumIntervalSeconds = CountersDbUpdateMinimumIntervalSeconds,
            };

        private readonly Dictionary<string, string> m_features =
            new Dictionary<string, string> { { FeatureCodes.WidgetDailyViewLimit, DailyLoads.ToString() } };

        private readonly FeatureServiceClientMock m_featureServiceClientMock;
        private readonly IAuditTrailClient m_auditTrailClient;
        private readonly AuditTrailClientSaveMock<WidgetDailyViewCountExceededEvent> m_auditMock;
        private List<AuditEvent<WidgetDailyViewCountExceededEvent>> AuditEvents => m_auditMock.AuditEvents;
        private readonly TestNowProvider m_nowProvider;

        public PageLoadServiceTests()
        {
            m_featureServiceClientMock = new FeatureServiceClientMock(m_features);
            m_nowProvider = new TestNowProvider(() => Date);

            m_auditTrailClient = Substitute.For<IAuditTrailClient>();
            m_auditMock = new AuditTrailClientSaveMock<WidgetDailyViewCountExceededEvent>(m_auditTrailClient);
        }

        protected override void ContinueSetup()
        {
            m_features[FeatureCodes.WidgetDailyViewLimit] = DailyLoads.ToString();
            m_auditTrailClient.ClearReceivedCalls();
            m_auditMock.Clear();
        }

        [Test]
        public async Task MainTest()
        {
            var dataLogicMock = new CustomerWidgetLoadStorageMock();

            var now = Date;
            Date = now.AddSeconds(123);
            var expectedData = new Dictionary<DateTime, CountIsExceeded>();

            var auditEvent = new AuditEvent<WidgetDailyViewCountExceededEvent>
                {
                    Operation = OperationKind.WidgetDailyOverloadKey,
                    Status = OperationStatus.AccessDeniedKey,
                    CustomerId = TestConstants.CustomerIdString,
                    NewValue = new WidgetDailyViewCountExceededEvent
                        {
                            Total = DailyLoads + 1,
                            Limit = DailyLoads,
                            Date = Date.AddDays(2).RemoveTime()
                        }
                };
            auditEvent.SetAnalyzedFields();
            var expectedAuditEvents = new List<AuditEvent<WidgetDailyViewCountExceededEvent>> { auditEvent };

            using (var service = new WidgetLoadCounterStorage(
                m_nowProvider,
                m_auditTrailClient,
                m_featureServiceClientMock,
                DatabaseFactory,
                m_widgetLoadLimiterSettings,
                dataLogicMock))
            {
                await Init(service);
                service.Save(true);

                //Day zero.
                for (var i = 0; i < CountersDbUpdateDelta; i++)
                {
                    if (CountersDbUpdateDelta - 1 == i)
                        Date = Date.AddSeconds(CountersDbUpdateMinimumIntervalSeconds - 1);

                    Assert.IsTrue(await Add(service), $"First load number {i}.");

                    using (var dataContext = DatabaseFactory.CreateContext())
                    {
                        var arr = dataContext.Db.WIDGET_LOAD.ToList();
                        Assert.IsEmpty(arr, $"Must be no data in the table {nameof(dataContext.Db.WIDGET_LOAD)}. First load number {i}.");
                    }

                    Assert.AreEqual(0, dataLogicMock.UpdateCalls, $"Updates, i={i}.");
                    Assert.IsEmpty(dataLogicMock.Data, "Page loads");
                }

                Date = Date.AddSeconds(1);
                Assert.IsTrue(await Add(service), "Add after " + nameof(CountersDbUpdateMinimumIntervalSeconds));
                Assert.AreEqual(1, dataLogicMock.UpdateCalls, "Must be updates after " + nameof(CountersDbUpdateMinimumIntervalSeconds));
                expectedData[now] = new CountIsExceeded(CountersDbUpdateDelta + 1, false);
                dataLogicMock.Data.Should().BeEquivalentTo(expectedData, "Page loads after " + nameof(CountersDbUpdateMinimumIntervalSeconds));

                Assert.IsTrue(await Add(service), "Add one more after " + nameof(CountersDbUpdateMinimumIntervalSeconds));
                dataLogicMock.Data.Should().BeEquivalentTo(expectedData, "Page loads after the first save and 1 add.");

                //Change date - day 1.
                Date = Date.AddDays(1);
                expectedData[now] = new CountIsExceeded(expectedData[now].Key + 1, false);
                now = now.AddDays(1);
                Assert.IsTrue(await Add(service), "Add after date change.");
                Assert.AreEqual(2, dataLogicMock.UpdateCalls, "Updates after date change.");
                dataLogicMock.Data.Should().BeEquivalentTo(expectedData, "Page loads after date change.");

                //exactly DailyLoads.
                Date = Date.AddSeconds(CountersDbUpdateMinimumIntervalSeconds + 1);
                Assert.IsTrue(await Add(service, DailyLoads - 1), "Add after " + nameof(DailyLoads));
                Assert.AreEqual(3, dataLogicMock.UpdateCalls, "Updates after " + nameof(DailyLoads));
                expectedData[now] = new CountIsExceeded(DailyLoads, false);
                dataLogicMock.Data.Should().BeEquivalentTo(expectedData, "Page loads after " + nameof(DailyLoads));

                //Overloaded
                for (int i = 0; i < DailyLoads; i++)
                {
                    //Locked in the memory, but not saved to the storage.
                    Assert.IsFalse(await Add(service), $"Add after {nameof(DailyLoads)}, i={i}.");
                    dataLogicMock.Data.Should().BeEquivalentTo(expectedData, $"Page loads must not change after exceeding {nameof(DailyLoads)}, i={i}.");
                    Assert.AreEqual(3, dataLogicMock.UpdateCalls, $"Updates after {nameof(DailyLoads)}, i={i}.");
                }

                Assert.IsEmpty(AuditEvents, "AuditEvents day1");


                //Change date - day 2, must unlock after lock.
                Date = Date.AddDays(1);
                expectedData[now] = new CountIsExceeded(expectedData[now].Key + 1, true);
                now = now.AddDays(1);
                Assert.IsTrue(await Add(service), "Successful add after lock on the second date change.");
                Assert.AreEqual(4, dataLogicMock.UpdateCalls, "Updates after second date change.");
                dataLogicMock.Data.Should().BeEquivalentTo(expectedData, "Page loads after second date change.");
                AuditEvents.Should().BeEquivalentTo(expectedAuditEvents, "AuditEvents day 2.");

                //Change date - day 3, no lock before and after.
                Date = Date.AddDays(1);
                expectedData[now] = new CountIsExceeded(1, false);
                Assert.IsTrue(await Add(service), "Successful add after lock on the third date change.");
                Assert.AreEqual(5, dataLogicMock.UpdateCalls, "Updates after third date change.");
                dataLogicMock.Data.Should().BeEquivalentTo(expectedData, "Page loads after third date change.");

                service.Save(true);
                Assert.AreEqual(0, service.Save(true), "Save result");
                now = now.AddDays(1);
                expectedData[now] = new CountIsExceeded(1, false);
                Assert.AreEqual(6, dataLogicMock.UpdateCalls, $"Updates after {nameof(WidgetLoadCounterStorage.Save)}.");
                dataLogicMock.Data.Should().BeEquivalentTo(expectedData, $"Page loads after {nameof(WidgetLoadCounterStorage.Save)}.");

                Assert.IsTrue(await Add(service), "Successful second add after lock on the third date change.");
                expectedData[now] = new CountIsExceeded(expectedData[now].Key + 1, false);
            }

            Assert.AreEqual(7, dataLogicMock.UpdateCalls, $"Updates after {nameof(WidgetLoadCounterStorage.Dispose)}.");
            dataLogicMock.Data.Should().BeEquivalentTo(expectedData, $"Page loads after {nameof(WidgetLoadCounterStorage.Dispose)}.");
            AuditEvents.Should().BeEquivalentTo(expectedAuditEvents, "AuditEvents after Dispose.");
        }

        private Task<bool> Add(WidgetLoadCounterStorage service, int count = 1)
        {
            return service.Add(TestConstants.CustomerId, Date, count);
        }

        [Test]
        public async Task MustLoadDataFromDatabaseAtServiceStart()
        {
            var dataLogic = new CustomerWidgetLoadStorage();

            const int amount = 2 * DailyLoads / 3;
            var endDate = Date.AddDays(1);

            using (var context = DatabaseFactory.CreateContext())
            {
                var loads = dataLogic.GetForCustomer(context.Db, TestConstants.CustomerId, Date, endDate);
                Assert.IsEmpty(loads, "Empty table before testing.");
                dataLogic.Update(context.Db, TestConstants.CustomerId, Date, amount, false);
                context.Commit();
            }

            using (var service = new WidgetLoadCounterStorage(
                m_nowProvider,
                m_auditTrailClient,
                m_featureServiceClientMock,
                DatabaseFactory,
                m_widgetLoadLimiterSettings,
                dataLogic))
            {
                await Init(service);
                var actual = await service.Add(TestConstants.CustomerId, Date, amount);
                Assert.IsFalse(actual, $"Old data loading from the database must work, customerId={TestConstants.CustomerId}.");
            }

            using (var context = DatabaseFactory.CreateContext())
            {
                var actual = dataLogic.GetForCustomer(context.Db, TestConstants.CustomerId, Date, endDate).ToList();
                var expected = new List<WidgetViewStatisticsEntry>
                    {
                        new WidgetViewStatisticsEntry
                            {
                                CustomerId = TestConstants.CustomerId,
                                Count = 2 * amount,
                                Date = Date,
                                IsViewCountExceeded = true
                            }
                    };
                expected.Should().BeEquivalentTo(actual, $"Load after {nameof(WidgetLoadCounterStorage.Dispose)}");
            }
        }

        [Test]
        public async Task TwoServices()
        {
            var dataLogic3 = new CustomerWidgetLoadStorage();

            const int amount = 2 * DailyLoads / 3;
            var dataLogic1 = new CustomerWidgetLoadStorage();
            var dataLogic2 = new CustomerWidgetLoadStorage();

            using (var service1 = new WidgetLoadCounterStorage(
                m_nowProvider,
                m_auditTrailClient,
                m_featureServiceClientMock,
                DatabaseFactory,
                m_widgetLoadLimiterSettings,
                dataLogic1))
            using (var service2 = new WidgetLoadCounterStorage(
                m_nowProvider,
                m_auditTrailClient,
                m_featureServiceClientMock,
                DatabaseFactory,
                m_widgetLoadLimiterSettings,
                dataLogic2))
            {
                await Init(service1);
                await Init(service2);
                service1.Save(true);
                service2.Save(true);

                //First update by another service.
                using (var context = DatabaseFactory.CreateContext())
                {
                    dataLogic3.Update(context.Db, TestConstants.CustomerId, Date, amount, false);
                    context.Commit();
                }

                {
                    var add = await service1.Add(TestConstants.CustomerId, Date, amount);
                    Assert.IsTrue(add, "The service1 doesn't know about the first update.");
                    var actual = SelectLoad();
                    var expected = new List<WidgetViewStatisticsEntry>
                        {
                            new WidgetViewStatisticsEntry
                                {
                                    CustomerId = TestConstants.CustomerId,
                                    Count = amount,
                                    Date = Date,
                                    IsViewCountExceeded = false
                                }
                        };
                    expected.Should().BeEquivalentTo(actual, "The total sum is over the limit but each update is within the limit.");
                }

                //Second update by another service.
                const int secondLoad = 1;
                using (var context = DatabaseFactory.CreateContext())
                {
                    dataLogic3.Update(context.Db, TestConstants.CustomerId, Date, secondLoad, true);
                    context.Commit();
                }

                {
                    var add = await service1.Add(TestConstants.CustomerId, Date, 1);
                    Assert.IsTrue(add, "The service1 now knows about the first update.");
                    var actual = SelectLoad();
                    var expected = new List<WidgetViewStatisticsEntry>
                        {
                            new WidgetViewStatisticsEntry
                                {
                                    CustomerId = TestConstants.CustomerId,
                                    Count = amount + secondLoad,
                                    Date = Date,
                                    IsViewCountExceeded = true
                                }
                        };
                    expected.Should().BeEquivalentTo(actual, "After service1 second add.");
                }

                {
                    var add = await service2.Add(TestConstants.CustomerId, Date, amount);
                    Assert.IsTrue(add, "The service2 doesn't know about the first update.");
                    var actual = SelectLoad();
                    var expected = new List<WidgetViewStatisticsEntry>
                        {
                            new WidgetViewStatisticsEntry
                                {
                                    CustomerId = TestConstants.CustomerId,
                                    Count = amount + secondLoad,
                                    Date = Date,
                                    IsViewCountExceeded = true
                                }
                        };
                    expected.Should().BeEquivalentTo(actual, $"{nameof(WidgetViewStatisticsEntry.IsViewCountExceeded)} must not be reset.");
                }
            }

            {
                var actual = SelectLoad();
                var expected = new List<WidgetViewStatisticsEntry>
                    {
                        new WidgetViewStatisticsEntry
                            {
                                CustomerId = TestConstants.CustomerId,
                                Count = 3 * amount + 2,
                                Date = Date,
                                IsViewCountExceeded = true
                            }
                    };
                expected.Should().BeEquivalentTo(actual, "After both services are disposed.");
            }

            Assert.IsEmpty(AuditEvents, "AuditEvents after Dispose.");
        }

        [Test]
        public async Task LimitIncreasesThenDababaseResetMustNotOverload([Values(0, DailyLoads * 2)] int newLimit)
        {
            var isUnlimited = 0 == newLimit;

            var dataLogic = new CustomerWidgetLoadStorage();
            var expectedLoad = new WidgetViewStatisticsEntry
                {
                    CustomerId = TestConstants.CustomerId,
                    Count = DailyLoads + 1,
                    Date = Date,
                    IsViewCountExceeded = true
                };
            var expected = new List<WidgetViewStatisticsEntry> { expectedLoad };

            using (var service = new WidgetLoadCounterStorage(
                m_nowProvider,
                m_auditTrailClient,
                m_featureServiceClientMock,
                DatabaseFactory,
                m_widgetLoadLimiterSettings,
                dataLogic))
            {
                await Init(service);
                {
                    //ViewCounterExceeded
                    var add = await service.Add(TestConstants.CustomerId, Date, DailyLoads + 1);
                    Assert.IsFalse(add, "First add.");

                    service.Save(true);
                    var actual = SelectLoad();
                    expected.Should().BeEquivalentTo(actual, "After first save.");
                }

                //Change the limit.
                m_features[FeatureCodes.WidgetDailyViewLimit] = newLimit.ToString();

                //Update the database.
                using (var context = DatabaseFactory.CreateContext())
                {
                    var sql = $"UPDATE {nameof(WIDGET_LOAD)} SET {nameof(WIDGET_LOAD.ISOVERLOAD)} = 0";
                    context.Db.Execute(sql);
                    context.Commit();
                }

                {
                    //Verify the direct database update.
                    var actual = SelectLoad();
                    expectedLoad.IsViewCountExceeded = false;
                    expected.Should().BeEquivalentTo(actual, $"The {nameof(WIDGET_LOAD.ISOVERLOAD)} must have been updated.");
                }
                {
                    service.Save(true);
                    service.LoadMany(new[] { TestConstants.CustomerId }, true);
                }
                {
                    var add = await service.Add(TestConstants.CustomerId, Date, DailyLoads - 1);
                    Assert.IsTrue(add, "Second add must take the new limit into account.");

                    service.Save(true);
                    var actual = SelectLoad();
                    expectedLoad.Count += DailyLoads - 1;
                    actual.Should().BeEquivalentTo(expected, "After second add.");
                }

                {
                    var add = await service.Add(TestConstants.CustomerId, Date, 1);
                    Assert.AreEqual(isUnlimited, add, "Third add.");
                }
            }

            {
                var actual = SelectLoad();
                expectedLoad.IsViewCountExceeded = !isUnlimited;
                expectedLoad.Count++;
                actual.Should().BeEquivalentTo(expected, "After service dispose.");
            }
        }

        [Test]
        public async Task LimitDecreasesMustOverload()
        {
            var dataLogic = new CustomerWidgetLoadStorage();
            var expectedLoad = new WidgetViewStatisticsEntry
                {
                    CustomerId = TestConstants.CustomerId,
                    Count = DailyLoads - 1,
                    Date = Date,
                    IsViewCountExceeded = false
                };
            var expected = new List<WidgetViewStatisticsEntry> { expectedLoad };
            const int newDailyLimit = DailyLoads / 2;

            using (var service = new WidgetLoadCounterStorage(
                m_nowProvider,
                m_auditTrailClient,
                m_featureServiceClientMock,
                DatabaseFactory,
                m_widgetLoadLimiterSettings,
                dataLogic))
            {
                await Init(service);
                {
                    var add = await service.Add(TestConstants.CustomerId, Date, expectedLoad.Count);
                    Assert.IsTrue(add, "First add.");

                    service.Save(true);
                    var actual = SelectLoad();
                    expected.Should().BeEquivalentTo(actual, "After first save.");
                }

                Assert.IsEmpty(AuditEvents, "AuditEvents before limit decrease.");

                m_features[FeatureCodes.WidgetDailyViewLimit] = newDailyLimit.ToString();

                {
                    var add = await service.Add(TestConstants.CustomerId, Date, 1);
                    Assert.IsFalse(add, "Second add must overload.");
                }
            }

            {
                var actual = SelectLoad();
                expectedLoad.IsViewCountExceeded = true;
                actual.Should().BeEquivalentTo(expected, "The database must have been updated.");
            }
            {
                var auditEvent = new AuditEvent<WidgetDailyViewCountExceededEvent>
                    {
                        Operation = OperationKind.WidgetDailyOverloadKey,
                        Status = OperationStatus.AccessDeniedKey,
                        CustomerId = TestConstants.CustomerIdString,
                        NewValue = new WidgetDailyViewCountExceededEvent
                            {
                                Total = expectedLoad.Count,
                                Limit = newDailyLimit,
                                Date = Date.RemoveTime()
                            }
                    };
                auditEvent.SetAnalyzedFields();
                var expectedAuditEvents = new List<AuditEvent<WidgetDailyViewCountExceededEvent>> { auditEvent };

                AuditEvents.Should().BeEquivalentTo(expectedAuditEvents, "AuditEvents after Dispose.");
            }
        }

        [Test]
        public async Task InitialLoadOverTheLimitMustOverload()
        {
            var storage = new CustomerWidgetLoadStorage();
            const int oldLoad = DailyLoads * 2;
            using (var dataContext = DatabaseFactory.CreateContext())
            {
                storage.Update(dataContext.Db, TestConstants.CustomerId, Date, oldLoad, false);
                dataContext.Commit();
            }

            var expectedLoad = new WidgetViewStatisticsEntry
                {
                    CustomerId = TestConstants.CustomerId,
                    Count = oldLoad,
                    Date = Date,
                    IsViewCountExceeded = true
                };
            var expected = new List<WidgetViewStatisticsEntry> { expectedLoad };
            const int newDailyLimit = DailyLoads / 2;
            m_features[FeatureCodes.WidgetDailyViewLimit] = newDailyLimit.ToString();

            using (var service = new WidgetLoadCounterStorage(
                m_nowProvider,
                m_auditTrailClient,
                m_featureServiceClientMock,
                DatabaseFactory,
                m_widgetLoadLimiterSettings,
                storage))
            {
                await Init(service);
                var add = await service.Add(TestConstants.CustomerId, Date, 1);
                Assert.IsFalse(add, "First add.");
                Assert.AreEqual(0, service.Save(true), "Save");

                var actual = SelectLoad();
                actual.Should().BeEquivalentTo(expected, "The database must have been updated.");
            }

            {
                var auditEvent = new AuditEvent<WidgetDailyViewCountExceededEvent>
                    {
                        Operation = OperationKind.WidgetDailyOverloadKey,
                        Status = OperationStatus.AccessDeniedKey,
                        CustomerId = TestConstants.CustomerIdString,
                        NewValue = new WidgetDailyViewCountExceededEvent
                            {
                                Total = oldLoad,
                                Limit = newDailyLimit,
                                Date = Date.RemoveTime()
                            }
                    };
                auditEvent.SetAnalyzedFields();
                var expectedAuditEvents = new List<AuditEvent<WidgetDailyViewCountExceededEvent>> { auditEvent };

                AuditEvents.Should().BeEquivalentTo(expectedAuditEvents, "AuditEvents after Dispose.");
            }
        }

        private static async Task Init([NotNull] IWidgetLoadCounterStorage service)
        {
            await service.Load();
            var customerCacheNotify = Substitute.For<ICustomerCacheNotifier>();
            service.SetNotifier(customerCacheNotify);
        }

        [NotNull]
        private List<WidgetViewStatisticsEntry> SelectLoad()
        {
            var storage = new CustomerWidgetLoadStorage();
            var endDate = Date.AddDays(1);
            using (var context = DatabaseFactory.CreateContext())
            {
                var result1 = storage.GetForCustomer(context.Db, TestConstants.CustomerId, Date, endDate).ToList();
                return result1;
            }
        }
    }
}