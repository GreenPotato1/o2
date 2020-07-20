using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.AuditTrail.Properties;
using Com.O2Bionics.AuditTrail.Tests.Utils;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using FluentAssertions;
using JetBrains.Annotations;
using Jil;
using NUnit.Framework;
using TestCase = System.Collections.Generic.KeyValuePair<string, Com.O2Bionics.AuditTrail.Contract.Filter>;

namespace Com.O2Bionics.AuditTrail.Tests
{
    [TestFixture]
    public sealed class AuditTrailServiceTest : BaseElasticTest
    {
        private const string NotExistingSubstring = AuditEventBuilder.Substring1 + AuditEventBuilder.Substring1;

        private static readonly AuditEvent<DepartmentInfo> m_auditEvent;
        private static readonly Filter m_allFieldsFilter;

        private static readonly TestCase[] m_successCases, m_failCases;

        static AuditTrailServiceTest()
        {
            m_auditEvent = AuditEventBuilder.DepartmentUpdate(UtcNow);

            m_allFieldsFilter = new Filter(ProductCodes.Chat, 2)
                {
                    FromRow = 0,
                    CustomerId = TestConstants.CustomerIdString,
                    Operations = new List<string> { OperationKind.DepartmentUpdateKey },
                    Statuses = new List<string> { OperationStatus.SuccessKey },
                    AuthorIds = new List<string> { TestConstants.FakeUserId.ToString() },
                    FromTime = UtcNow,
                    ToTime = UtcNow.AddSeconds(1),
                    Substring = AuditEventBuilder.Substring1
                };

            m_successCases = SuccessCases();
            m_failCases = FailCases();
        }

        protected override void ContinueSetup()
        {
            var serializedJson = JSON.Serialize(m_auditEvent, JsonSerializerBuilder.SkipNullJilOptions);
            Service.Save(ProductCodes.Chat, serializedJson).WaitAndUnwrapException();
            ElasticClient.Flush(IndexName);
        }

        [AssertionMethod] // ParameterOnlyUsedForPreconditionCheck
        private List<string> SelectByFilter([NotNull] Filter filter, [NotNull] string name)
        {
            var error = filter.Validate(false);
            Assert.Null(error, "Filter validate, name={0}", name);

            return Service.SelectFacets(filter).WaitAndUnwrapException()?.RawDocuments;
        }

        private static TestCase[] SuccessCases()
        {
            var emptyFilter = new Filter(ProductCodes.Chat, 2)
                {
                    CustomerId = TestConstants.CustomerIdString
                };

            var statuses1 = new List<string> { OperationStatus.SuccessKey };
            var statuses3 = new List<string>
                {
                    OperationStatus.ValidationFailedKey,
                    OperationStatus.SuccessKey,
                    OperationStatus.AccessDeniedKey
                };
            var operation = OperationKind.DepartmentUpdateKey;
            var operations1 = new List<string> { operation };
            var operations3 = new List<string> { "10264653", operation, "196467676" };

            var testCases = new[]
                {
                    new TestCase(
                        "FromTime ToTime",
                        new Filter(ProductCodes.Chat, 2)
                            {
                                CustomerId = TestConstants.CustomerIdString,
                                FromTime = UtcNow,
                                ToTime = UtcNow.AddSeconds(1)
                            }),
                    new TestCase("AAA Top", emptyFilter),
                    new TestCase(
                        "Top with customer s key",
                        new Filter(ProductCodes.Chat, 2)
                            {
                                CustomerId = TestConstants.CustomerIdString
                            }),
                    new TestCase(
                        nameof(Filter.AuthorIds),
                        new Filter(ProductCodes.Chat, 2)
                            {
                                CustomerId = TestConstants.CustomerIdString,
                                AuthorIds = new List<string> { TestConstants.FakeUserId.ToString() }
                            }),
                    new TestCase(
                        "Operation",
                        new Filter(ProductCodes.Chat, 2)
                            {
                                CustomerId = TestConstants.CustomerIdString,
                                Operations = operations1
                            }),
                    new TestCase(
                        "Status",
                        new Filter(ProductCodes.Chat, 2)
                            {
                                CustomerId = TestConstants.CustomerIdString,
                                Statuses = statuses1
                            }),
                    new TestCase(
                        "Operation and status",
                        new Filter(ProductCodes.Chat, 2)
                            {
                                CustomerId = TestConstants.CustomerIdString,
                                Operations = operations1,
                                Statuses = statuses1
                            }),
                    new TestCase(
                        "Operation3 and status",
                        new Filter(ProductCodes.Chat, 2)
                            {
                                CustomerId = TestConstants.CustomerIdString,
                                Operations = operations3,
                                Statuses = statuses1
                            }),
                    new TestCase(
                        "Operation and status3",
                        new Filter(ProductCodes.Chat, 2)
                            {
                                CustomerId = TestConstants.CustomerIdString,
                                Operations = operations1,
                                Statuses = statuses3
                            }),
                    new TestCase(
                        "Operation3 and status3",
                        new Filter(ProductCodes.Chat, 2)
                            {
                                CustomerId = TestConstants.CustomerIdString,
                                Operations = operations3,
                                Statuses = statuses3
                            }),
                    new TestCase(
                        "Substring",
                        new Filter(ProductCodes.Chat, 2)
                            {
                                CustomerId = TestConstants.CustomerIdString,
                                Substring = AuditEventBuilder.Substring1
                            }),
                    new TestCase(
                        "All filter fields without Substring",
                        new Filter(m_allFieldsFilter)
                            {
                                CustomerId = TestConstants.CustomerIdString,
                                Substring = null
                            }),
                    new TestCase("All filter fields", m_allFieldsFilter),
                    new TestCase(
                        "All plus extra values in arrays",
                        new Filter(m_allFieldsFilter)
                            {
                                CustomerId = TestConstants.CustomerIdString,
                                Operations = m_allFieldsFilter.Operations.Union(new[] { operation + 5000 }).ToList(),
                                Statuses = m_allFieldsFilter.Statuses.Union(new[] { OperationStatus.OperationFailedKey }).ToList(),
                                AuthorIds = new[] { (TestConstants.FakeUserId + 20000).ToString() }.Union(m_allFieldsFilter.AuthorIds)
                                    .Union(new[] { (TestConstants.FakeUserId + 10005).ToString() })
                                    .ToList()
                            })
                };
            return testCases;
        }

        private static TestCase[] FailCases()
        {
            var operation = OperationKind.DepartmentUpdateKey;
            var operations = new List<string> { operation + 454574 };
            var statuses = new List<string> { OperationStatus.AccessDeniedKey };
            var authorIds = new List<string> { (TestConstants.FakeUserId + 2).ToString() };

            var timeFilter = new Filter(m_allFieldsFilter)
                {
                    CustomerId = TestConstants.CustomerIdString,
                    FromTime = m_allFieldsFilter.FromTime.AddSeconds(1),
                    ToTime = m_allFieldsFilter.ToTime.AddSeconds(2)
                };

            var notExistingCustomerId = (TestConstants.CustomerId + 1).ToString();
            var testCases = new[]
                {
                    new TestCase(nameof(Filter.Substring), new Filter(m_allFieldsFilter) { Substring = NotExistingSubstring }),
                    new TestCase(
                        "Several fields",
                        new Filter(timeFilter)
                            {
                                PageSize = 25,
                                FromRow = 1,
                                CustomerId = notExistingCustomerId,
                                Operations = operations,
                                Statuses = statuses,
                                AuthorIds = authorIds,
                                Substring = NotExistingSubstring
                            }),
                    new TestCase(nameof(Filter.CustomerId), new Filter(m_allFieldsFilter) { CustomerId = notExistingCustomerId }),
                    new TestCase(
                        "Second page, size 2",
                        new Filter(ProductCodes.Chat, 2)
                            {
                                CustomerId = TestConstants.CustomerIdString,
                                FromRow = 1
                            }),
                    new TestCase(
                        "Second page, size 1",
                        new Filter(ProductCodes.Chat, 1)
                            {
                                CustomerId = TestConstants.CustomerIdString,
                                FromRow = 1
                            }),
                    new TestCase("Operation", new Filter(m_allFieldsFilter) { Operations = operations }),
                    new TestCase("Status", new Filter(m_allFieldsFilter) { Statuses = new List<string> { OperationStatus.AccessDeniedKey } }),
                    new TestCase(
                        "Status2",
                        new Filter(m_allFieldsFilter)
                            {
                                Statuses = new List<string> { OperationStatus.AccessDeniedKey, OperationStatus.OperationFailedKey }
                            }),
                    new TestCase(
                        "Good Operation and status",
                        new Filter(m_allFieldsFilter)
                            {
                                Operations = new List<string> { operation },
                                Statuses = new List<string> { OperationStatus.AccessDeniedKey }
                            }),
                    new TestCase(
                        "Operation and status",
                        new Filter(m_allFieldsFilter)
                            {
                                Operations = operations,
                                Statuses = new List<string> { OperationStatus.AccessDeniedKey }
                            }),
                    new TestCase(
                        nameof(Filter.Statuses),
                        new Filter(m_allFieldsFilter) { Statuses = statuses }),
                    new TestCase(nameof(Filter.AuthorIds), new Filter(m_allFieldsFilter) { AuthorIds = authorIds }),
                    new TestCase(
                        nameof(Filter.FromTime),
                        timeFilter)
                };
            return testCases;
        }

        [TestCaseSource(nameof(m_successCases))]
        public void SelectExisting(TestCase testCase)
        {
            var name = testCase.Key;
            var selected = SelectByFilter(testCase.Value, name);
            Assert.NotNull(selected, "selected {0}", name);
            Assert.AreEqual(1, selected.Count, "selected.Count {0}", name);

            m_auditEvent.ClearAnalyzedFields();

            var actual = selected[0].JsonUnstringify2<AuditEvent<DepartmentInfo>>();
            m_auditEvent.Should().BeEquivalentTo(actual, "Selected document {0}", name);
        }

        [TestCaseSource(nameof(m_failCases))]
        public void SelectReturnNothing(TestCase testCase)
        {
            var name = testCase.Key;
            var selected = SelectByFilter(testCase.Value, name);
            Assert.IsNull(selected, testCase.Key);
        }

        private async Task<List<Facet>> FetchAuthors(Filter filter, int attempts)
        {
            // It takes time for the Elastic to build the aggregation.
            for (var i = 0; i < attempts; i++)
            {
                if (0 < i)
                {
                    var report = $"Trying to fetch authors, attempt={i}.";
                    Console.WriteLine(report);
                }

                var response = await Service.SelectFacets(filter);
                var authors = response?.Authors;
                if (null != authors && 0 < authors.Count)
                    return authors;

                const int millisecondsTimeout = 10;
                Thread.Sleep(millisecondsTimeout);
            }

            throw new Exception($"No authors after {attempts} attempts.");
        }

        [Test]
        public void NotExistingProduct()
        {
            const string name = "NotExistingProduct";

            var filter = new Filter(name, 2);
            try
            {
                SelectByFilter(filter, name);
                Assert.Fail("An exception must have been thrown.");
            }
            catch (ArgumentException e)
            {
                var expectedError = string.Format(Resources.UnknownProductCode1, name);
                Assert.AreEqual(expectedError, e.Message, nameof(e.Message));
            }
        }

        // Run first to ensure the Elastic has indexed the content.
        [Order(1)]
        [Test]
        public void SearchDirectly()
        {
            var savedAuditEvent = ElasticClient.FetchFirstDocument<AuditEvent<DepartmentInfo>>(
                IndexName,
                OperationKind.DepartmentUpdateKey);
            m_auditEvent.Should().BeEquivalentTo(savedAuditEvent.Value, "Sent and saved documents.");
        }

        [Test]
        public async Task SelectAuthorsTest()
        {
            var filter = new Filter
                {
                    ProductCode = ProductCodes.Chat,
                    PageSize = 2,
                    FromRow = 0,
                    CustomerId = TestConstants.CustomerIdString
                };
            filter.Validate();

            const int attempts = 1000;

            var authors = await FetchAuthors(filter, attempts);
            var expected = new List<Facet>
                {
                    new Facet(TestConstants.FakeUserId.ToString(), TestConstants.FakeUserName, 1)
                };
            expected.Should().BeEquivalentTo(authors, "authors");
        }
    }
}