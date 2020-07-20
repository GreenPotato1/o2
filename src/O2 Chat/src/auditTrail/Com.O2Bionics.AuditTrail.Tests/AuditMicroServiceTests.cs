using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Com.O2Bionics.AuditTrail.Client;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.ChatService.Contract.WidgetAppearance;
using Com.O2Bionics.ChatService.Impl.AuditTrail;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Com.O2Bionics.AuditTrail.Tests
{
    [TestFixture]
    public sealed class AuditMicroServiceTests : BaseElasticTest, IDisposable
    {
        private const uint CustomerId = 1497674;

        private readonly uint[] m_departmentIds =
            {
                2345234562,
                35462456,
                890906890,
                657894,
                3457,
                245734,
                554634567,
                764735673,
                1567536876
            };

        private const string OldName = "Name1";
        private const string NewName = "Name2";

        private readonly AuditTrailClient m_auditTrailClient;
        private readonly INameResolver m_nameResolver;
        private readonly TestNowProvider m_nowProvider = new TestNowProvider();

        public AuditMicroServiceTests()
        {
            m_auditTrailClient = new AuditTrailClient(ClientSettings, m_nowProvider, ProductCodes.Chat);
            m_nameResolver = CreateNameResolver();
        }

        public void Dispose()
        {
            m_auditTrailClient.Dispose();
        }

        private static AuditEvent<ChatWidgetAppearance> BuildChatWidgetAppearance()
        {
            var oldValue = new ChatWidgetAppearance
                {
                    ThemeId = "Some them",
                    Location = ChatWidgetLocation.BottomLeft,
                    OffsetX = 10,
                    OffsetY = 274,
                    MinimizedStateTitle = "tit1",
                    CustomCssUrl = "http://server/rule",
                    PoweredByVisible = false,
                    ThemeMinId = "m1"
                };
            var newValue = new ChatWidgetAppearance
                {
                    ThemeId = "Other theme",
                    Location = ChatWidgetLocation.BottomRight,
                    OffsetX = 689,
                    OffsetY = 560467,
                    MinimizedStateTitle = "tit2",
                    CustomCssUrl = "http://server/rule/2",
                    PoweredByVisible = true,
                    ThemeMinId = "m2"
                };

            var result = new AuditEvent<ChatWidgetAppearance>
                {
                    Status = OperationStatus.SuccessKey,
                    Operation = OperationKind.WidgetAppearanceUpdateKey,
                    OldValue = oldValue,
                    NewValue = newValue
                };
            result.FieldChanges = result.OldValue.Diff(result.NewValue);
            return result;
        }

        private AuditEvent<CustomerInfo> BuildCustomer()
        {
            var now = m_nowProvider.UtcNow;
            var addTimestamp = now.AddDays(-10);
            var oldValue = new CustomerInfo
                {
                    Id = CustomerId,
                    AddTimestampUtc = addTimestamp,
                    UpdateTimestampUtc = now,
                    Status = ObjectStatus.Disabled,
                    Name = "Name1",
                    Domains = new[] { "DOM 8", "http://d1.ta.da", "d2", "d3" },
                    CreateIp = "127.0.0.5"
                };
            var newValue = new CustomerInfo
                {
                    Id = CustomerId,
                    AddTimestampUtc = now,
                    UpdateTimestampUtc = oldValue.UpdateTimestampUtc.AddDays(1003),
                    Status = ObjectStatus.Active,
                    Name = "Name2",
                    Domains = new[] { "d1", "d4", "d3", "d5" },
                    CreateIp = "127.0.0.2"
                };

            var result = new AuditEvent<CustomerInfo>
                {
                    Status = OperationStatus.AccessDeniedKey,
                    Operation = OperationKind.CustomerUpdateKey,
                    OldValue = oldValue,
                    NewValue = newValue
                };
            result.FieldChanges = result.OldValue.Diff(result.NewValue);
            return result;
        }

        private static AuditEvent<DepartmentInfo> BuildDepartment()
        {
            const int departmentId = 5464646;

            var auditEvent = new AuditEvent<DepartmentInfo>
                {
                    Status = OperationStatus.ValidationFailedKey,
                    Operation = OperationKind.DepartmentUpdateKey,
                    OldValue = new DepartmentInfo
                        {
                            Name = OldName,
                            CustomerId = CustomerId,
                            Id = departmentId,
                            Status = ObjectStatus.Disabled,
                            Description = "d1",
                            IsPublic = false
                        },
                    NewValue = new DepartmentInfo
                        {
                            Name = NewName,
                            CustomerId = CustomerId,
                            Id = departmentId,
                            Status = ObjectStatus.Active,
                            Description = "D2",
                            IsPublic = true
                        }
                };
            auditEvent.FieldChanges = auditEvent.OldValue.Diff(auditEvent.NewValue);
            return auditEvent;
        }

        private AuditEvent<UserInfo> BuildUser()
        {
            var now = m_nowProvider.UtcNow;

            var oldValue = new UserInfo
                {
                    CustomerId = CustomerId,
                    Id = 1924562435u,
                    AddTimestampUtc = now.AddDays(-10),
                    UpdateTimestampUtc = now.AddDays(-9),
                    Status = ObjectStatus.NotConfirmed,
                    FirstName = "fist",
                    LastName = "la",
                    Email = "e@ma.il",
                    IsOwner = false,
                    IsAdmin = true,
                    Avatar = "again",
                    AgentDepartments = new HashSet<uint> { m_departmentIds[0], m_departmentIds[1], m_departmentIds[2] },
                    SupervisorDepartments = new HashSet<uint> { m_departmentIds[3], m_departmentIds[4], m_departmentIds[5] }
                };
            var newValue = new UserInfo
                {
                    CustomerId = CustomerId,
                    Id = 1924563576u,
                    AddTimestampUtc = now.AddDays(-8),
                    UpdateTimestampUtc = oldValue.UpdateTimestampUtc.AddDays(3),
                    Status = ObjectStatus.Active,
                    FirstName = "first",
                    LastName = "last",
                    Email = "e@ma.ill",
                    IsOwner = true,
                    IsAdmin = false,
                    Avatar = "not again",
                    AgentDepartments = new HashSet<uint>
                        {
                            m_departmentIds[1],
                            m_departmentIds[0],
                            m_departmentIds[2],
                            m_departmentIds[4],
                            m_departmentIds[8]
                        },
                    SupervisorDepartments = new HashSet<uint> { m_departmentIds[6], m_departmentIds[7] }
                };

            var result = new AuditEvent<UserInfo>
                {
                    Status = OperationStatus.OperationFailedKey,
                    Operation = OperationKind.UserUpdateKey,
                    OldValue = oldValue,
                    NewValue = newValue,
                    ObjectNames = new Dictionary<string, Dictionary<string, string>>
                        {
                            {
                                "some key", new Dictionary<string, string>
                                    {
                                        { "key1", "value 1" },
                                        { "sum2", "value 1" },
                                        { "key", "value 1" }
                                    }
                            },
                            {
                                "some key2", new Dictionary<string, string>
                                    {
                                        { "key1", "value 1" },
                                        { "sum2", "value 1" },
                                        { "key", "value 1" }
                                    }
                            }
                        },
                    CustomValues = new Dictionary<string, string>
                        {
                            { "key1", "value 1" },
                            { "sum2", "value 12" },
                            { "key", "value 31" }
                        }
                };
            result.FieldChanges = result.OldValue.Diff(result.NewValue, m_nameResolver);
            return result;
        }

        private INameResolver CreateNameResolver()
        {
            var nameResolver = Substitute.For<INameResolver>();

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < m_departmentIds.Length; i++)
                nameResolver.GetDepartmentName(CustomerId, m_departmentIds[i]).Returns($"Department{i + 1}");
            return nameResolver;
        }

        private async Task TestImpl<T>(Func<AuditEvent<T>> build)
            where T : class
        {
            var auditEvent = build();
            Assert.NotNull(auditEvent, "Built entity");

            auditEvent.Author = new Author("1", "Slava Tut");
            auditEvent.CustomerId = TestConstants.CustomerIdString;

            await m_auditTrailClient.Save(auditEvent);

            Assert.GreaterOrEqual(DateTime.UtcNow, auditEvent.Timestamp, nameof(auditEvent.Timestamp));
            Assert.NotNull(auditEvent.FieldChanges, $"{nameof(auditEvent.FieldChanges)} after saving");

            var saved = ElasticClient.FetchFirstDocument<AuditEvent<T>>(
                IndexName,
                auditEvent.Operation);
            auditEvent.Should().BeEquivalentTo(saved.Value, "Before and after saving.");
        }

        [Test]
        public async Task ChatWidgetAppearanceAudit()
        {
            await TestImpl(BuildChatWidgetAppearance);
        }

        [Test]
        public async Task CustomerAudit()
        {
            await TestImpl(BuildCustomer);
        }

        [Test]
        public async Task DepartmentAudit()
        {
            await TestImpl(BuildDepartment);
        }

        [Test]
        public async Task UserAudit()
        {
            await TestImpl(BuildUser);
        }
    }
}