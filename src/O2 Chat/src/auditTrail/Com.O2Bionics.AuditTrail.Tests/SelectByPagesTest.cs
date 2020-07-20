using System;
using System.Linq;
using System.Threading.Tasks;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.AuditTrail.Tests.Utils;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.Utils;
using FluentAssertions;
using Jil;
using NUnit.Framework;

namespace Com.O2Bionics.AuditTrail.Tests
{
    [TestFixture]
    public sealed class SelectByPagesTest : BaseElasticTest
    {
        private const int HalfSize = 32;
        private const int PageSize = 7;
        private readonly AuditEvent<DepartmentInfo> m_sampleDepartment;

        private readonly AuditEvent<DepartmentInfo>[] m_departments = new AuditEvent<DepartmentInfo>[2 * HalfSize];
        private readonly AuditEvent<DepartmentInfo>[] m_buffer = new AuditEvent<DepartmentInfo>[2 * HalfSize];

        public SelectByPagesTest()
        {
            m_sampleDepartment = AuditEventBuilder.DepartmentUpdate(UtcNow);
        }

        protected override void ContinueSetup()
        {
            Save();
            CheckSaved().WaitAndUnwrapException();
        }

        private void Save()
        {
            Enumerable.Range(0, 2 * HalfSize).AsParallel().ForAll(
                i =>
                    {
                        //Half events have the same time.
                        var sec = i < HalfSize ? -i : -HalfSize;
                        var auditEvent = m_departments[i] = new AuditEvent<DepartmentInfo>(m_sampleDepartment)
                            {
                                Timestamp = m_sampleDepartment.Timestamp.AddSeconds(sec),
                                Id = new Guid(2 * HalfSize - i, 1, 1, 3, 4, 5, 6, 7, 8, 9, 10),
                                Status = 0 == (i & 1) ? OperationStatus.SuccessKey : OperationStatus.AccessDeniedKey
                            };
                        var serializedJson = JSON.Serialize(auditEvent, JsonSerializerBuilder.SkipNullJilOptions);

                        Service.Save(ProductCodes.Chat, serializedJson).WaitAndUnwrapException();
                    }
            );
            ElasticClient.Flush(IndexName);
        }

        private async Task CheckSaved()
        {
            await Service.FetchAndParse(m_sampleDepartment.Operation, 2 * HalfSize, m_buffer);
            for (var i = 0; i < 2 * HalfSize; ++i)
            {
                m_departments[i].ClearAnalyzedFields();
                m_departments[i].Should().BeEquivalentTo(m_buffer[i], $"i={i}");
            }
        }

        [Test]
        public async Task Test()
        {
            const int pages = (2 * HalfSize + PageSize - 1) / PageSize;

            SearchPositionInfo positionInfo = null;

            for (var i = 0; i < pages; i++)
            {
                var i2 = i;

                var isLastPage = pages - 1 == i2;
                var size = isLastPage ? 2 * HalfSize - (pages - 1) * PageSize : PageSize;
                Assert.Greater(size, 0, nameof(size));
                Assert.GreaterOrEqual(PageSize, size, nameof(size));

                void FilterAction(Filter fi)
                {
                    if (0 < i2)
                        fi.SearchPosition = positionInfo;
                }

                for (var j = 0; j <= PageSize; j++) m_buffer[j] = null;

                const int dataAlreadyInsertedMaxAttempts = 1;
                await Service.FetchAndParse(m_sampleDepartment.Operation, size, m_buffer, FilterAction, dataAlreadyInsertedMaxAttempts);

                for (var j = 0; j < size; j++)
                {
                    var index = PageSize * i + j;

                    m_departments[index].Should().BeEquivalentTo(
                        m_buffer[j],
                        $"Departments at index={index}, j={j}, i={i2}.");
                }

                var lastAuditEvent = m_buffer[size - 1];
                Assert.IsNotNull(lastAuditEvent, $"{nameof(m_buffer)}[{size - 1}], i={i2}");

                positionInfo = new SearchPositionInfo(
                    new[]
                        {
                            lastAuditEvent.Timestamp.ToUnixTimeMilliseconds().ToString("#"),
                            lastAuditEvent.Id.ToString()
                        });
            }
        }
    }
}