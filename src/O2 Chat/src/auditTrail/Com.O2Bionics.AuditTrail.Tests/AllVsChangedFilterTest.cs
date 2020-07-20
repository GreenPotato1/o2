using System.Threading.Tasks;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.AuditTrail.Tests.Utils;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.ChatService.Impl.AuditTrail;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using FluentAssertions;
using Jil;
using NUnit.Framework;
using TestCase = System.ValueTuple<string, Com.O2Bionics.AuditTrail.Contract.Filter, bool>;

namespace Com.O2Bionics.AuditTrail.Tests
{
    /// <summary>
    ///     If a field is not changed,
    ///     the full text search must find its value only in <seealso cref="AuditEvent{T}.All" />,
    ///     not in <seealso cref="AuditEvent{T}.Changed" />
    /// </summary>
    [TestFixture]
    public sealed class AllVsChangedFilterTest : BaseElasticTest
    {
        private const string Name = "one two three Weather",
            OldDescription = "Weather five six seven",
            NewDescription = "Snow nine ten eleven",
            NotExisting = "Exec";

        private readonly AuditEvent<DepartmentInfo> m_history;

        private static readonly TestCase[] m_testCases =
            {
                new TestCase(
                    "Changed_Name",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            ChangedOnly = true,
                            Substring = Name
                        },
                    false),
                new TestCase(
                    "Changed_Name_0",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            ChangedOnly = true,
                            Substring = "one"
                        },
                    false),
                new TestCase(
                    "Changed_Name_2",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            ChangedOnly = true,
                            Substring = "three"
                        },
                    false),
                //
                new TestCase(
                    "All_Name",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            Substring = Name
                        },
                    true),
                new TestCase(
                    "Empty substring",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            Substring = string.Empty
                        },
                    true),
                //
                new TestCase(
                    "All_OldDescription",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            Substring = OldDescription
                        },
                    true),
                new TestCase(
                    "Changed_OldDescription",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            ChangedOnly = true,
                            Substring = OldDescription
                        },
                    true),
                //
                new TestCase(
                    "All_OldDescription_0",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            Substring = "Weather"
                        },
                    true),
                new TestCase(
                    "Changed_OldDescription_0",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            ChangedOnly = true,
                            Substring = "Weather"
                        },
                    true),
                new TestCase(
                    "All_OldDescription_0_lowercase",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            Substring = "weather"
                        },
                    true),
                new TestCase(
                    "Changed_OldDescription_0_lowercase",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            ChangedOnly = true,
                            Substring = "weather"
                        },
                    true),
                new TestCase(
                    "All_OldDescription_0_UPPERCASE",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            Substring = "WEATHER"
                        },
                    true),
                new TestCase(
                    "Changed_OldDescription_0_UPPERCASE",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            ChangedOnly = true,
                            Substring = "WEATHER"
                        },
                    true),
                //
                new TestCase(
                    "All_OldDescription_1",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            Substring = "five"
                        },
                    true),
                new TestCase(
                    "Changed_OldDescription_1",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            ChangedOnly = true,
                            Substring = "five"
                        },
                    true),
                //
                new TestCase(
                    "All_OldDescription_3",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            Substring = "seven"
                        },
                    true),
                new TestCase(
                    "Changed_OldDescription_3",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            ChangedOnly = true,
                            Substring = "seven"
                        },
                    true),
                //
                new TestCase(
                    "All_NewDescription",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            Substring = NewDescription
                        },
                    true),
                new TestCase(
                    "Changed_NewDescription",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            ChangedOnly = true,
                            Substring = NewDescription
                        },
                    true),
                //
                new TestCase(
                    "All_NewDescription_0",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            Substring = "Snow"
                        },
                    true),
                new TestCase(
                    "Changed_NewDescription_0",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            ChangedOnly = true,
                            Substring = "Snow"
                        },
                    true),
                //
                new TestCase(
                    "All_NewDescription_2",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            Substring = "ten"
                        },
                    true),
                new TestCase(
                    "Changed_NewDescription_3",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            ChangedOnly = true,
                            Substring = "elEven"
                        },
                    true),
                //
                new TestCase(
                    "All_NotExisting",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            Substring = NotExisting
                        },
                    false),
                new TestCase(
                    "Changed_NotExisting",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            ChangedOnly = true,
                            Substring = NotExisting
                        },
                    false),
                //Prefix
                new TestCase(
                    "All_Prefix",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            Substring = "we"
                        },
                    true),
                new TestCase(
                    "Changed_Prefix_0",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            ChangedOnly = true,
                            Substring = "we"
                        },
                    true),
                new TestCase(
                    "All_Prefix_Description",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            Substring = "fiv"
                        },
                    true),
                new TestCase(
                    "Changed_Prefix_Description",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            ChangedOnly = true,
                            Substring = "fiv"
                        },
                    true),
                new TestCase(
                    "All_Prefix_Name",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            Substring = "thr"
                        },
                    true),
                new TestCase(
                    "Changed_Prefix_Name",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            ChangedOnly = true,
                            Substring = "thr"
                        },
                    false),
                //Middle
                new TestCase(
                    "All_Middle",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            Substring = "eve"
                        },
                    false),
                new TestCase(
                    "Changed_Middle",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            ChangedOnly = true,
                            Substring = "eve"
                        },
                    false),
                //Suffix
                new TestCase(
                    "All_Suffix",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            Substring = "ven"
                        },
                    false),
                new TestCase(
                    "Changed_Suffix",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            ChangedOnly = true,
                            Substring = "ven"
                        },
                    false),
                //Many words
                new TestCase(
                    "All_Words_Name",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            Substring = "two three "
                        },
                    true),
                new TestCase(
                    "All_Words_Name_Reversed",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            //TODO. p2. task-298. Elastic correct?
                            Substring = "three two"
                        },
                    false),
                new TestCase(
                    "Changed_Words_Name",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            ChangedOnly = true,
                            Substring = "two three"
                        },
                    false),
                new TestCase(
                    "All_Words_Description",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            Substring = "  two three "
                        },
                    true),
                new TestCase(
                    "Changed_Words_Description",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            ChangedOnly = true,
                            Substring = "nine ten"
                        },
                    true),
                new TestCase(
                    "Changed_Words_Description3",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            ChangedOnly = true,
                            Substring = "nine ten eleven"
                        },
                    true),
                new TestCase(
                    "Changed_Words_Description_mixed",
                    new Filter(ProductCodes.Chat, 2)
                        {
                            CustomerId = TestConstants.CustomerIdString,
                            ChangedOnly = true,
                            //TODO. p2. task-298. Elastic correct?
                            Substring = "nine eleven ten"
                        },
                    false)
            };

        public AllVsChangedFilterTest()
        {
            m_history = AuditEventBuilder.DepartmentUpdate(UtcNow);

            m_history.NewValue.Name = m_history.OldValue.Name = Name;

            m_history.OldValue.Description = OldDescription;
            m_history.NewValue.Description = NewDescription;

            //Repair
            m_history.FieldChanges = m_history.OldValue.Diff(m_history.NewValue);
            m_history.SetAnalyzedFields();
        }

        protected override void ContinueSetup()
        {
            Save();
            EnsureSaved();
        }

        private void Save()
        {
            var serializedJson = JSON.Serialize(m_history, JsonSerializerBuilder.SkipNullJilOptions);
            Service.Save(ProductCodes.Chat, serializedJson).WaitAndUnwrapException();
            ElasticClient.Flush(IndexName);
        }

        private void EnsureSaved()
        {
            var saved = ElasticClient.FetchFirstDocument<AuditEvent<DepartmentInfo>>(
                IndexName,
                m_history.Operation).Value;
            m_history.Should().BeEquivalentTo(saved, "Before and after saving.");

            m_history.ClearAnalyzedFields();
        }

        [Test]
        [TestCaseSource(nameof(m_testCases))]
        public async Task Test(TestCase testCase)
        {
            (var _, var filter, var shouldExist) = testCase;

            var response = await Service.SelectFacets(filter);
            if (!shouldExist)
            {
                Assert.IsNull(response, "Response");
                return;
            }

            Assert.IsNotNull(response, "Response");
            response.CheckFacets(OperationKind.DepartmentUpdateKey);

            var rawDocuments = response.RawDocuments;
            Assert.IsNotNull(rawDocuments, nameof(response.RawDocuments));
            Assert.AreEqual(1, rawDocuments.Count, "Count");

            var actual = rawDocuments[0].JsonUnstringify2<AuditEvent<DepartmentInfo>>();
            m_history.Should().BeEquivalentTo(actual, nameof(m_history));
        }
    }
}