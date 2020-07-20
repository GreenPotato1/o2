using System.Threading.Tasks;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.AuditTrail.Tests.Utils;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.ChatService.Impl.AuditTrail;
using Com.O2Bionics.Utils;
using FluentAssertions;
using Jil;
using NUnit.Framework;

namespace Com.O2Bionics.AuditTrail.Tests
{
    [TestFixture]
    public sealed class SeveralCustomersTest : BaseElasticTest
    {
        private readonly AuditEvent<DepartmentInfo>[] m_histories = new AuditEvent<DepartmentInfo>[3];

        public SeveralCustomersTest()
        {
            for (var i = 0; i < m_histories.Length; i++)
            {
                var history = m_histories[i] = AuditEventBuilder.DepartmentUpdate(UtcNow);

                history.CustomerId = (i + 1).ToString();
                history.OldValue.Name = $"Nam{i + 105}";

                // Each customer uses its own operation.
                history.Operation = OperationKind.UserChangePasswordKey + i;

                // Repair
                history.FieldChanges = history.OldValue.Diff(history.NewValue);
                history.SetAnalyzedFields();
            }
        }

        protected override void ContinueSetup()
        {
            Parallel.For(0, m_histories.Length, InsertDocument);
            ElasticClient.Flush(IndexName);
        }

        private void InsertDocument(int i)
        {
            var history = m_histories[i];
            var serializedJson = JSON.Serialize(history, JsonSerializerBuilder.SkipNullJilOptions);

            Service.Save(ProductCodes.Chat, serializedJson).WaitAndUnwrapException();

            var saved = ElasticClient.FetchFirstDocument<AuditEvent<DepartmentInfo>>(
                IndexName,
                history.Operation);
            history.Should().BeEquivalentTo(saved.Value, $"Before and after saving, i={i}.");
        }

        [Test]
        public void CustomerCanSeeOnlyHisOwnDocuments()
        {
            Parallel.For(
                0,
                m_histories.Length,
                i =>
                    {
                        var history = m_histories[i];
                        history.ClearAnalyzedFields();
                        var filter = new Filter(ProductCodes.Chat, 10)
                            {
                                CustomerId = history.CustomerId
                            };
                        var response = Service.SelectFacets(filter).WaitAndUnwrapException();
                        Assert.IsNotNull(response, $"Response{i}");

                        var rawDocuments = response.RawDocuments;
                        Assert.IsNotNull(rawDocuments, nameof(response.RawDocuments));
                        Assert.AreEqual(1, rawDocuments.Count, "Count");

                        var actual = rawDocuments[0].JsonUnstringify2<AuditEvent<DepartmentInfo>>();
                        history.Should().BeEquivalentTo(actual, $"m_histories[{i}]");

                        response.CheckFacets(history.Operation);
                    });
        }
    }
}