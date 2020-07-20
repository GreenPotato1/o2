using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Com.O2Bionics.AuditTrail.Client;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.ChatService.Contract.Widget;
using Com.O2Bionics.ChatService.Impl.AuditTrail;
using Com.O2Bionics.ChatService.Impl.Storage;
using Com.O2Bionics.ChatService.Tests.Mocks;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using FluentAssertions;
using NUnit.Framework;
using NSubstitute;

namespace Com.O2Bionics.ChatService.Tests.WidgetLoadLimiter
{
    [TestFixture]
    [Parallelizable]
    public sealed class UnknownDomainAuditServiceTests
    {
        private const int MaxCount = 3;

        private readonly List<AuditEvent<WidgetUnknownDomain>> m_expected = new List<AuditEvent<WidgetUnknownDomain>>();
        private readonly List<AuditEvent<WidgetUnknownDomainTooManyEvent>> m_expectedTooMany = new List<AuditEvent<WidgetUnknownDomainTooManyEvent>>();

        private readonly IAuditTrailClient m_auditTrailClient;
        private readonly AuditTrailClientSaveMock<WidgetUnknownDomain> m_mock;
        private readonly AuditTrailClientSaveMock<WidgetUnknownDomainTooManyEvent> m_mockTooMany;

        public UnknownDomainAuditServiceTests()
        {
            m_auditTrailClient = Substitute.For<IAuditTrailClient>();
            m_mock = new AuditTrailClientSaveMock<WidgetUnknownDomain>(m_auditTrailClient);
            m_mockTooMany = new AuditTrailClientSaveMock<WidgetUnknownDomainTooManyEvent>(m_auditTrailClient);
        }

        [SetUp]
        public void Setup()
        {
            m_auditTrailClient.ClearReceivedCalls();

            m_expected.Clear();
            m_expectedTooMany.Clear();
            m_mock.Clear();
            m_mockTooMany.Clear();
        }

        [Test]
        public async Task Test()
        {
            var time = DateTime.UtcNow;
            var date = time.RemoveTime();
            var domainStorage = Substitute.For<IUnknownDomainLoader>();
            var nowProvider = new TestNowProvider(time);

            var customerCacheNotify = Substitute.For<ICustomerCacheNotifier>();
            int flag = 0, flagExpected = 0;
            customerCacheNotify.WhenForAnyArgs(c => c.NotifyMany(Arg.Any<uint[]>())).Do(
                c => { ++flag; });

            var service = new WidgetLoadUnknownDomainStorage(
                nowProvider,
                m_auditTrailClient,
                domainStorage) { MaximumUnknownDomains = MaxCount };
            Assert.AreEqual(MaxCount, service.MaximumUnknownDomains, nameof(service.MaximumUnknownDomains));
            await service.Load();
            service.SetNotifier(customerCacheNotify);

            const int size = MaxCount * 2;
            for (var i = 0; i <= size; i++)
            {
                var str = i.ToString();
                string domains = "domain_list;abcd.ef.gh" + str, name = "unknown.domain" + str;
                if (i < size)
                {
                    var isTooMany = await service.Add(time, TestConstants.CustomerId, domains, name, true);
                    var expected = service.MaximumUnknownDomains - 1 == i;
                    Assert.AreEqual(expected, isTooMany, "isTooMany, i=" + i);
                    if (expected)
                        flagExpected = 1;
                    Assert.AreEqual(flagExpected, flag, "flag, i=" + i);
                }

                if (i < MaxCount)
                    Add(date, domains, name, i == MaxCount - 1);

                m_mock.AuditEvents.Should().BeEquivalentTo(m_expected, "i=" + str);
                m_mockTooMany.AuditEvents.Should().BeEquivalentTo(m_expectedTooMany, "Too many, i=" + str);
            }
        }

        private void Add(DateTime date, string domains, string name, bool isEdge)
        {
            Debug.Assert(date == date.RemoveTime());
            if (isEdge)
            {
                var auditEvent = new AuditEvent<WidgetUnknownDomainTooManyEvent>
                    {
                        Operation = OperationKind.WidgetUnknownDomainTooManyKey,
                        Status = OperationStatus.AccessDeniedKey,
                        CustomerId = TestConstants.CustomerIdString,
                        NewValue = new WidgetUnknownDomainTooManyEvent
                            {
                                Domains = domains,
                                Limit = MaxCount,
                                Date = date
                            },
                    };
                auditEvent.SetAnalyzedFields();
                m_expectedTooMany.Add(auditEvent);
            }
            else
            {
                var auditEvent = new AuditEvent<WidgetUnknownDomain>
                    {
                        Operation = OperationKind.WidgetUnknownDomainKey,
                        Status = OperationStatus.AccessDeniedKey,
                        CustomerId = TestConstants.CustomerIdString,
                        NewValue = new WidgetUnknownDomain { Name = name },
                    };
                auditEvent.NewValue.Domains = domains;
                auditEvent.SetAnalyzedFields();
                m_expected.Add(auditEvent);
            }
        }
    }
}