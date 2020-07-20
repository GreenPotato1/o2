using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Com.O2Bionics.AuditTrail.Client;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.AuditTrail.Tests.Utils;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.ChatService.Contract.Widget;
using Com.O2Bionics.ChatService.Impl.Storage;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using FluentAssertions;
using JetBrains.Annotations;
using Jil;
using NSubstitute;
using NUnit.Framework;

namespace Com.O2Bionics.AuditTrail.Tests
{
    [TestFixture]
    public sealed class UnknownDomainStorageTests : BaseElasticTest
    {
        private readonly uint[] m_customerIds = { TestConstants.CustomerId, 2 };
        private readonly AuditEvent<WidgetUnknownDomain>[] m_histories = new AuditEvent<WidgetUnknownDomain>[20];
        private readonly AuditEvent<WidgetUnknownDomain>[] m_buffer;
        private readonly Dictionary<uint, HashSet<string>> m_expected;

        public UnknownDomainStorageTests()
        {
            m_buffer = new AuditEvent<WidgetUnknownDomain>[m_histories.Length];
            m_expected = new Dictionary<uint, HashSet<string>>(m_histories.Length);
            foreach (var customerId in m_customerIds)
            {
                m_expected[customerId] = new HashSet<string>();
            }

            for (var i = 0; i < m_histories.Length; i++)
            {
                var history = m_histories[i] = AuditEventBuilder.WidgetUnknownDomainEvent(UtcNow);

                var pos = i < m_histories.Length / 2 ? 0 : 1;
                history.CustomerId = m_customerIds[pos].ToString();
                history.NewValue.Name = "Name_" + i + "_";
                history.ClearAnalyzedFields();

                m_expected[m_customerIds[pos]].Add(history.NewValue.Name);
            }
        }

        protected override void ContinueSetup()
        {
            Parallel.For(0, m_histories.Length, InsertDocument);
            ElasticClient.Flush(IndexName);

            var list = new AuditEvent<WidgetUnknownDomain>[m_histories.Length];
            var count = 0;
            foreach (var customerId in m_customerIds)
            {
                Service.FetchAndParse(m_histories[0].Operation, m_histories.Length / 2, m_buffer, customerId: customerId.ToString()).WaitAndUnwrapException();

                for (var i = 0; i < m_histories.Length / 2; i++)
                {
                    list[count++] = m_buffer[i];
                    m_buffer[i] = null;
                }
            }

            Sort(list);
            Sort(m_histories);
            list.Should().BeEquivalentTo(m_histories, "Documents saved");
        }

        private static void Sort([NotNull] AuditEvent<WidgetUnknownDomain>[] arr)
        {
            Array.Sort(arr, (a, b) => a.Id.CompareTo(b.Id));
        }

        private void InsertDocument(int i)
        {
            var history = m_histories[i];
            var serializedJson = JSON.Serialize(history, JsonSerializerBuilder.SkipNullJilOptions);

            Service.Save(ProductCodes.Chat, serializedJson).WaitAndUnwrapException();
        }

        [Test]
        public async Task Test()
        {
            var domainStorage = new UnknownDomainLoader();
            var chunkSize = m_histories.Length / 7;
            domainStorage.SetChunkSize(chunkSize);
            var expectedCalls = (m_histories.Length + chunkSize - 1) / chunkSize + 1;

            var nowProvider = new TestNowProvider(UtcNow);
            var client = new AuditTrailClient(ClientSettings, nowProvider, ProductCodes.Chat);
            var actualCalls = 0;

            var substitute = Substitute.For<IAuditTrailClient>();
            substitute.SelectFacets(Arg.Any<Filter>()).ReturnsForAnyArgs(
                args =>
                    {
                        var filter = args.Arg<Filter>();
                        Assert.IsNotNull(filter, nameof(filter));
                        Interlocked.Increment(ref actualCalls);
                        return client.SelectFacets(filter).WaitAndUnwrapException();
                    });

            var actual = await domainStorage.Load(substitute, UtcNow, m_histories.Length);
            Assert.IsNotNull(actual, nameof(actual));
            actual.Should().BeEquivalentTo(m_expected, "Fetched data");
            Assert.AreEqual(expectedCalls, actualCalls, nameof(actualCalls));
        }
    }
}