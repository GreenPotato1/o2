using System;
using System.Collections.Generic;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.ChatService.Contract.Widget;
using Com.O2Bionics.ChatService.Contract.WidgetAppearance;
using Com.O2Bionics.ChatService.Impl.AuditTrail;
using Com.O2Bionics.ChatService.Impl.AuditTrail.Names;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;
using Jil;
using NUnit.Framework;
using pair = Com.O2Bionics.AuditTrail.Contract.IdName<uint>;

namespace Com.O2Bionics.AuditTrail.Tests.Utils
{
    /// <summary>
    /// Here all possible fields must be set so that the Elastic index had all fields.
    /// </summary>
    public static class AuditEventBuilder
    {
        private const int DepartmentId = 75435623;
        private const int UserId = 1346;
        public const string Substring1 = "name1";

        public static AuditEvent<ChatWidgetAppearance> ChatWidgetAppearanceUpdate(DateTime utcNow)
        {
            var oldValue = new ChatWidgetAppearance
                {
                    ThemeId = "Theme1",
                    ThemeMinId = "ThemeMinId1",
                    Location = ChatWidgetLocation.BottomLeft,
                    OffsetX = 12,
                    OffsetY = 345,
                    MinimizedStateTitle = "Minimized State Title1",
                    CustomCssUrl = "http://10.0.0.1/css1",
                    PoweredByVisible = false
                };
            var newValue = new ChatWidgetAppearance
                {
                    ThemeId = "Theme2",
                    ThemeMinId = "ThemeMinId2",
                    Location = ChatWidgetLocation.TopRight,
                    OffsetX = 98797,
                    OffsetY = 8755,
                    MinimizedStateTitle = "Minimized State Title2",
                    CustomCssUrl = "http://10.0.0.1/css2",
                    PoweredByVisible = true
                };
            var auditEvent = new AuditEvent<ChatWidgetAppearance>
                {
                    Id = Guid.NewGuid(),
                    Status = OperationStatus.SuccessKey,
                    Operation = OperationKind.WidgetAppearanceUpdateKey,
                    Author = new Author(TestConstants.FakeUserId, TestConstants.FakeUserName),
                    CustomerId = TestConstants.CustomerIdString,
                    Timestamp = utcNow,
                    OldValue = oldValue,
                    NewValue = newValue,
                };
            auditEvent.FieldChanges = auditEvent.OldValue.Diff(auditEvent.NewValue);
            auditEvent.SetAnalyzedFields();
            return auditEvent;
        }

        public static AuditEvent<CustomerInfo> CustomerUpdate(DateTime utcNow)
        {
            var oldValue = new CustomerInfo
                {
                    Id = TestConstants.CustomerId,
                    AddTimestampUtc = utcNow.AddDays(-5),
                    UpdateTimestampUtc = utcNow.AddDays(-2),
                    Status = ObjectStatus.Disabled,
                    Name = "Some first",
                    CreateIp = "1.2.3.4",
                    Domains = new[] { "dom1", "dom2", "dom4", }
                };
            var newValue = new CustomerInfo
                {
                    Id = TestConstants.CustomerId,
                    AddTimestampUtc = utcNow.AddDays(-5),
                    UpdateTimestampUtc = utcNow,
                    Status = ObjectStatus.Active,
                    Name = "Other second",
                    CreateIp = "2.3.4.5",
                    Domains = new[] { "dom1", "dom4", "dom5", "dom6", "dom7", }
                };
            var auditEvent = new AuditEvent<CustomerInfo>
                {
                    Id = Guid.NewGuid(),
                    Status = OperationStatus.SuccessKey,
                    Operation = OperationKind.CustomerUpdateKey,
                    Author = new Author(TestConstants.FakeUserId, TestConstants.FakeUserName),
                    CustomerId = TestConstants.CustomerIdString,
                    Timestamp = utcNow,
                    OldValue = oldValue,
                    NewValue = newValue,
                };
            auditEvent.FieldChanges = auditEvent.OldValue.Diff(auditEvent.NewValue);
            auditEvent.SetAnalyzedFields();
            return auditEvent;
        }

        public static AuditEvent<DepartmentInfo> DepartmentUpdate(DateTime utcNow)
        {
            var oldValue = new DepartmentInfo
                {
                    CustomerId = TestConstants.CustomerId,
                    Id = DepartmentId,
                    Name = "Some " + Substring1 + " first",
                    Description = "Some description1",
                    IsPublic = false,
                    Status = ObjectStatus.Disabled
                };
            var newValue = new DepartmentInfo
                {
                    CustomerId = oldValue.CustomerId,
                    Id = oldValue.Id,
                    Name = "Some name2",
                    Description = "Some description2",
                    IsPublic = true,
                    Status = ObjectStatus.Active
                };
            var auditEvent = new AuditEvent<DepartmentInfo>
                {
                    Id = Guid.NewGuid(),
                    Status = OperationStatus.SuccessKey,
                    Operation = OperationKind.DepartmentUpdateKey,
                    Author = new Author(TestConstants.FakeUserId, TestConstants.FakeUserName),
                    CustomerId = TestConstants.CustomerIdString,
                    Timestamp = utcNow,
                    OldValue = oldValue,
                    NewValue = newValue,
                    FieldChanges = new FieldChanges
                        {
                            BoolChanges = new List<PlainFieldChange<bool>>
                                {
                                    new PlainFieldChange<bool>(nameof(DepartmentInfo.IsPublic), oldValue.IsPublic, newValue.IsPublic)
                                },
                            StringChanges = new List<PlainFieldChange<string>>
                                {
                                    new PlainFieldChange<string>(nameof(DepartmentInfo.Name), oldValue.Name, newValue.Name),
                                    new PlainFieldChange<string>(nameof(DepartmentInfo.Description), oldValue.Description, newValue.Description),
                                    new PlainFieldChange<string>(
                                        nameof(DepartmentInfo.Status),
                                        oldValue.Status.ToString(),
                                        newValue.Status.ToString())
                                }
                        }
                };
            auditEvent.SetAnalyzedFields();

            var serialized = JSON.Serialize(auditEvent, JsonSerializerBuilder.SkipNullJilOptions);
            var split = serialized.Split(new[] { Substring1 }, StringSplitOptions.RemoveEmptyEntries);
            const int oldPlusChange = 5;
            Assert.AreEqual(oldPlusChange, split.Length, $"Substring '{Substring1}' occurrence.");
            return auditEvent;
        }

        public static AuditEvent<UserInfo> UserUpdate(DateTime utcNow, [NotNull] INameResolver nameResolver)
        {
            var oldValue = new UserInfo
                {
                    CustomerId = TestConstants.CustomerId,
                    Id = UserId,
                    AddTimestampUtc = utcNow.AddDays(-2),
                    UpdateTimestampUtc = utcNow.AddDays(-1),
                    Status = ObjectStatus.Disabled,
                    FirstName = "Paul",
                    LastName = "Schmidt",
                    Email = "some@ma.il",
                    IsAdmin = true,
                    IsOwner = false,
                    Avatar = "Ray",
                    AgentDepartments = new HashSet<uint> { 345, 45768, 1235478, 3465785869 },
                    SupervisorDepartments = new HashSet<uint> { 13, 567, 56767, 9805679 }
                };
            var newValue = new UserInfo
                {
                    CustomerId = oldValue.CustomerId,
                    Id = oldValue.Id,
                    AddTimestampUtc = oldValue.AddTimestampUtc,
                    UpdateTimestampUtc = utcNow,
                    Status = ObjectStatus.Active,
                    FirstName = "Liam",
                    LastName = "Smith",
                    Email = "one@ma.il",
                    IsAdmin = false,
                    IsOwner = true,
                    Avatar = "Robin",
                    AgentDepartments = new HashSet<uint> { 45768, 3687798 },
                    SupervisorDepartments = new HashSet<uint> { 456845, 9805679, 2354962459 }
                };
            var auditEvent = new AuditEvent<UserInfo>
                {
                    Id = Guid.NewGuid(),
                    Status = OperationStatus.ValidationFailedKey,
                    Operation = OperationKind.UserUpdateKey,
                    Author = new Author(TestConstants.FakeUserId, TestConstants.FakeUserName),
                    CustomerId = TestConstants.CustomerIdString,
                    Timestamp = utcNow,
                    OldValue = oldValue,
                    NewValue = newValue,
                    ObjectNames = new Dictionary<string, Dictionary<string, string>>
                        {
                            {
                                EntityNames.Department, new Dictionary<string, string>
                                    {
                                        { "13", "Department 13" },
                                        { "345", "Department 345" },
                                        { "567", "Department 567" },
                                        { "45768", "Department 45768" },
                                        { "56767", "Department 56767" },
                                        { "456845", "Department 456845" },
                                        { "1235478", "Department_ 1235478" },
                                        { "3687798", "Department 3687798" },
                                        { "9805679", "Department 9805679" },
                                        { "34657858679", "Department 34657858679" },
                                        { "235496245967", "Department 235496245967" }
                                    }
                            }
                        },
                    FieldChanges = new FieldChanges
                        {
                            BoolChanges = new List<PlainFieldChange<bool>>
                                {
                                    new PlainFieldChange<bool>(nameof(UserInfo.IsAdmin), oldValue.IsAdmin, newValue.IsAdmin),
                                    new PlainFieldChange<bool>(nameof(UserInfo.IsOwner), oldValue.IsOwner, newValue.IsOwner)
                                },
                            DateTimeChanges = new List<PlainFieldChange<DateTime>>
                                {
                                    new PlainFieldChange<DateTime>(
                                        nameof(UserInfo.UpdateTimestampUtc),
                                        oldValue.UpdateTimestampUtc,
                                        newValue.UpdateTimestampUtc)
                                },
                            DecimalChanges = new List<PlainFieldChange<decimal>>
                                {
                                    new PlainFieldChange<decimal>("decimal_field1", 345645376, 54634567)
                                },
                            StringChanges = new List<PlainFieldChange<string>>
                                {
                                    new PlainFieldChange<string>(nameof(UserInfo.Status), oldValue.Status.ToString(), newValue.Status.ToString()),
                                    new PlainFieldChange<string>(nameof(UserInfo.FirstName), oldValue.FirstName, newValue.FirstName),
                                    new PlainFieldChange<string>(nameof(UserInfo.LastName), oldValue.LastName, newValue.LastName),
                                    new PlainFieldChange<string>(nameof(UserInfo.Email), oldValue.Email, newValue.Email),
                                    new PlainFieldChange<string>(nameof(UserInfo.Avatar), oldValue.Avatar, newValue.Avatar),
                                    //Some special char string.
                                    new PlainFieldChange<string>(
                                        "string B",
                                        @" 9`~!@#$%^&*()7  UIOP{}:L<>nl;;l,kkn'545pk 45oy ",
                                        " 2 326 23456 45635647  ")
                                },
                            IdListChanges = new List<IdListChange>
                                {
                                    new IdListChange(
                                        nameof(UserInfo.AgentDepartments),
                                        new List<pair>
                                            {
                                                new pair(345, "Department 345"),
                                                new pair(1235478, "Department_ 1235478"),
                                                new pair(3465785867, "Department3465785867")
                                            },
                                        new List<pair>
                                            {
                                                new pair(3687798, "Department 3687798")
                                            }),
                                    new IdListChange(
                                        nameof(UserInfo.SupervisorDepartments),
                                        new List<pair>
                                            {
                                                new pair(13, "Department 13"),
                                                new pair(567, "Department 567"),
                                                new pair(56767, "Department 56767")
                                            },
                                        new List<pair>
                                            {
                                                new pair(456845, "Department 456845"),
                                                new pair(2354962456, "Department 2354962456")
                                            })
                                },
                            StringListChanges = new List<ListChange<string>>
                                {
                                    //Some fake values.
                                    new ListChange<string>(
                                        "List change 1",
                                        new List<string>
                                            {
                                                "Del1",
                                                "Del 3",
                                                "Del    4 "
                                            },
                                        new List<string>
                                            {
                                                "1 Add",
                                                "2 Add",
                                                "    4 Add"
                                            })
                                }
                        },
                    CustomValues = new Dictionary<string, string>
                        {
                            { CustomFieldNames.ClientIp, "1.2.3.4" },
                            { CustomFieldNames.VisitorId, "1297" },
                            { CustomFieldNames.ClientType, "ClientType" },
                            { CustomFieldNames.ClientVersion, "1.2.3" },
                            { CustomFieldNames.ClientLocalDate, utcNow.ToUtcString() },
                            { CustomFieldNames.ExceptionMessage, "Some long error message " + new string('1', 10 * 1000) + " error end." }
                        }
                };

            auditEvent.FetchDictionaries(nameResolver);
            auditEvent.SetAnalyzedFields(true);
            return auditEvent;
        }

        public static AuditEvent<UserInfo> UnknownUserLogin(DateTime utcNow, [NotNull] INameResolver nameResolver)
        {
            var auditEvent = UserUpdate(utcNow, nameResolver);
            auditEvent.Operation = OperationKind.UserLoginKey;
            auditEvent.Status = OperationStatus.NotFoundKey;
            auditEvent.OldValue.CustomerId = auditEvent.NewValue.CustomerId = 0;
            auditEvent.OldValue.Id = auditEvent.NewValue.Id = 0;
            auditEvent.OldValue.Email = auditEvent.NewValue.Email = "some@unknown.ema.il";
            auditEvent.SetAnalyzedFields(true);
            return auditEvent;
        }

        public static AuditEvent<WidgetDailyViewCountExceededEvent> WidgetOverloadEvent(DateTime utcNow)
        {
            var auditEvent = new AuditEvent<WidgetDailyViewCountExceededEvent>
                {
                    Id = Guid.NewGuid(),
                    Status = OperationStatus.AccessDeniedKey,
                    Operation = OperationKind.WidgetDailyOverloadKey,
                    CustomerId = TestConstants.CustomerIdString,
                    Timestamp = utcNow,
                    NewValue = new WidgetDailyViewCountExceededEvent
                        {
                            Total = 20,
                            Limit = 10,
                            Date = utcNow.RemoveTime()
                        },
                };
            auditEvent.SetAnalyzedFields(true);
            return auditEvent;
        }

        public static AuditEvent<WidgetUnknownDomainTooManyEvent> WidgetUnknownDomainTooManyEvent(DateTime utcNow)
        {
            var auditEvent = new AuditEvent<WidgetUnknownDomainTooManyEvent>
                {
                    Id = Guid.NewGuid(),
                    Status = OperationStatus.AccessDeniedKey,
                    Operation = OperationKind.WidgetUnknownDomainTooManyKey,
                    CustomerId = TestConstants.CustomerIdString,
                    Timestamp = utcNow,
                    NewValue = new WidgetUnknownDomainTooManyEvent
                        {
                            Domains = "domain.com",
                            Limit = 123,
                            Date = utcNow.RemoveTime()
                        },
                };
            auditEvent.SetAnalyzedFields(true);
            return auditEvent;
        }

        public static AuditEvent<WidgetUnknownDomain> WidgetUnknownDomainEvent(DateTime utcNow)
        {
            var auditEvent = new AuditEvent<WidgetUnknownDomain>
                {
                    Id = Guid.NewGuid(),
                    Status = OperationStatus.AccessDeniedKey,
                    Operation = OperationKind.WidgetUnknownDomainKey,
                    CustomerId = TestConstants.CustomerIdString,
                    Timestamp = utcNow,
                    NewValue = new WidgetUnknownDomain
                        {
                            Domains = "domain.com",
                            Name = "other.domain2.com",
                        },
                };
            auditEvent.SetAnalyzedFields(true);
            return auditEvent;
        }
    }
}