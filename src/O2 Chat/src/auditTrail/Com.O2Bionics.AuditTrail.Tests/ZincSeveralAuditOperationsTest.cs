using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.AuditTrail.Tests.Utils;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.ChatService.Contract.WidgetAppearance;
using Com.O2Bionics.Utils;
using FluentAssertions;
using JetBrains.Annotations;
using Jil;
using NUnit.Framework;

namespace Com.O2Bionics.AuditTrail.Tests
{
    // Zinc prefix to make it execute the last.
    [TestFixture]
    public sealed class ZincSeveralAuditOperationsTest : BaseElasticTest
    {
        private const int MinLog = 5, MaxLog = MinLog;

        private readonly TestNameResolver m_testNameResolver = new TestNameResolver();

        private readonly AuditEvent<CustomerInfo> m_customer;
        private readonly AuditEvent<CustomerInfo>[] m_customers = new AuditEvent<CustomerInfo>[1 << MaxLog];
        private readonly AuditEvent<CustomerInfo>[] m_customersTemp = new AuditEvent<CustomerInfo>[1 << MaxLog];

        private readonly AuditEvent<ChatWidgetAppearance> m_chatWidgetAppearance;
        private readonly AuditEvent<ChatWidgetAppearance>[] m_chatWidgetAppearances = new AuditEvent<ChatWidgetAppearance>[1 << MaxLog];
        private readonly AuditEvent<ChatWidgetAppearance>[] m_chatWidgetAppearancesTemp = new AuditEvent<ChatWidgetAppearance>[1 << MaxLog];

        private readonly AuditEvent<DepartmentInfo> m_department;
        private readonly AuditEvent<DepartmentInfo>[] m_departments = new AuditEvent<DepartmentInfo>[1 << MaxLog];
        private readonly AuditEvent<DepartmentInfo>[] m_departmentsTemp = new AuditEvent<DepartmentInfo>[1 << MaxLog];

        private readonly AuditEvent<UserInfo> m_user;
        private readonly AuditEvent<UserInfo>[] m_users = new AuditEvent<UserInfo>[1 << MaxLog];
        private readonly AuditEvent<UserInfo>[] m_usersTemp = new AuditEvent<UserInfo>[1 << MaxLog];
        private int m_size = 1;

        public ZincSeveralAuditOperationsTest()
        {
            m_chatWidgetAppearance = AuditEventBuilder.ChatWidgetAppearanceUpdate(UtcNow);
            m_customer = AuditEventBuilder.CustomerUpdate(UtcNow);
            m_department = AuditEventBuilder.DepartmentUpdate(UtcNow);
            m_user = AuditEventBuilder.UserUpdate(UtcNow, m_testNameResolver);
        }

        private async Task SaveChanges()
        {
            var report = $"Save {m_size} clones of each entity.";
            Console.WriteLine(report);

            var stopwatch = Stopwatch.StartNew();

            var loginEvent = AuditEventBuilder.UnknownUserLogin(UtcNow, m_testNameResolver);
            var loginEvents = new[] { loginEvent };

            var widgetOverloadEvent = AuditEventBuilder.WidgetOverloadEvent(UtcNow);
            var widgetOverloadEvents = new[] { widgetOverloadEvent };

            var widgetUnknownDomainEvent = AuditEventBuilder.WidgetUnknownDomainEvent(UtcNow);
            var widgetUnknownDomainEvents = new[] { widgetUnknownDomainEvent };

            var widgetUnknownDomainTooManyEvent = AuditEventBuilder.WidgetUnknownDomainTooManyEvent(UtcNow);
            var widgetUnknownDomainTooManyEvents = new[] { widgetUnknownDomainTooManyEvent };

            var tasks = new[]
                {
                    Task.Run(() => SaveEntities(m_chatWidgetAppearance, m_chatWidgetAppearances)),
                    Task.Run(() => SaveEntities(m_customer, m_customers)),
                    Task.Run(() => SaveEntities(m_department, m_departments)),
                    Task.Run(() => SaveEntities(m_user, m_users)),
                    Task.Run(() => SaveEntity(loginEvent, loginEvents, 0)),
                    Task.Run(() => SaveEntity(widgetOverloadEvent, widgetOverloadEvents, 0)),
                    Task.Run(() => SaveEntity(widgetUnknownDomainEvent, widgetUnknownDomainEvents, 0)),
                    Task.Run(() => SaveEntity(widgetUnknownDomainTooManyEvent, widgetUnknownDomainTooManyEvents, 0)),
                };
            await Task.WhenAll(tasks);

            ElasticClient.Flush(IndexName);

            stopwatch.Stop();
            report += $" Took {stopwatch.ElapsedMilliseconds} ms.";
            Console.WriteLine(report);
        }

        private void SaveEntities<T>([NotNull] AuditEvent<T> sampleHistory, [NotNull] AuditEvent<T>[] data)
        {
            var degree = Math.Min(m_size, Environment.ProcessorCount * 10);
            Enumerable.Range(0, m_size).AsParallel().WithDegreeOfParallelism(degree).ForAll(
                i => { SaveEntity(sampleHistory, data, i); });
        }

        private void SaveEntity<T>(AuditEvent<T> sampleEvent, AuditEvent<T>[] data, int i)
        {
            var auditEvent = data[i] = new AuditEvent<T>(sampleEvent)
                {
                    Id = Guid.NewGuid(),

                    // Documents are sorted by Timestamp descent.
                    Timestamp = sampleEvent.Timestamp.AddSeconds(-i)
                };
            if (m_size / 2 <= i)
            {
                var id1 = i % 7;
                var id2 = id1 % 2;
                auditEvent.Author.Id = $"Identifier{id2}";
                auditEvent.Author.Name = $"Pane{id1}";
            }

            var serializedJson = JSON.Serialize(auditEvent, JsonSerializerBuilder.SkipNullJilOptions);
            Service.Save(ProductCodes.Chat, serializedJson).WaitAndUnwrapException();
        }

        private async Task FetchCompare<T>(
            [NotNull] AuditEvent<T> sampleHistory,
            [NotNull] AuditEvent<T>[] expected,
            [NotNull] AuditEvent<T>[] actual)
        {
            await Service.FetchAndParse(sampleHistory.Operation, m_size, actual);
            CompareDocuments(sampleHistory, expected, actual);
        }

        private void CompareDocuments<T>(AuditEvent<T> sampleHistory, AuditEvent<T>[] expected, AuditEvent<T>[] actual)
        {
            var stopwatch = Stopwatch.StartNew();
            var report = $"Compare the fetched {m_size} {sampleHistory.Operation}.";
            Console.WriteLine(report);

            Enumerable.Range(0, m_size).AsParallel().ForAll(
                i =>
                    {
                        expected[i].ClearAnalyzedFields();
                        actual[i].Should().BeEquivalentTo(expected[i], "{0} [{1}].", sampleHistory.Operation, i);
                    });

            stopwatch.Stop();
            report += $" Took {stopwatch.ElapsedMilliseconds} ms.";
            Console.WriteLine(report);
        }

        [Test]
        public async Task Test()
        {
            var elapsedMs = new long[1 + MaxLog];

            for (var log = MinLog; log <= MaxLog; log++)
            {
                var stopwatch = Stopwatch.StartNew();

                m_size = 1 << log;
                var separator = 0 < log ? Environment.NewLine : string.Empty;
                var report = $"{separator}Test size {m_size}";
                Console.WriteLine(report);

                await SaveChanges();
                var tasks = new[]
                    {
                        Task.Run(() => FetchCompare(m_chatWidgetAppearance, m_chatWidgetAppearances, m_chatWidgetAppearancesTemp)),
                        Task.Run(() => FetchCompare(m_customer, m_customers, m_customersTemp)),
                        Task.Run(() => FetchCompare(m_department, m_departments, m_departmentsTemp)),
                        Task.Run(() => FetchCompare(m_user, m_users, m_usersTemp))
                    };
                await Task.WhenAll(tasks);

                if (log < MaxLog) // Leave the last subtest.
                    Clear();

                elapsedMs[log] = stopwatch.ElapsedMilliseconds;
                report += $" took {elapsedMs[log]} ms.";
                Console.WriteLine(report);
            }

            {
                const string report = "\nSummary\nSize\tTime, ms\n";
                Console.WriteLine(report);
            }

            for (var log = MinLog; log <= MaxLog; log++)
            {
                var report = $"{1 << log}\t{elapsedMs[log]}";
                Console.WriteLine(report);
            }
        }
    }
}