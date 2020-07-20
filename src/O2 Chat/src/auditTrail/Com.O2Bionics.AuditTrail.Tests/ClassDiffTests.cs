using System;
using System.Collections.Generic;
using System.Linq;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.WidgetAppearance;
using Com.O2Bionics.ChatService.Impl.AuditTrail;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using pair = Com.O2Bionics.AuditTrail.Contract.IdName<uint>;

namespace Com.O2Bionics.AuditTrail.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public sealed class ClassDiffTests
    {
        private const int CustomerId = 3467901;

        private static pair IdToPair(uint id)
        {
            return new pair(id, BuildDepartmentName(id));
        }

        private static string BuildDepartmentName(long id)
        {
            return $"{CustomerId} {id}";
        }

        private static INameResolver CreateNameResolver()
        {
            var objectResolver = Substitute.For<INameResolver>();

            objectResolver.GetDepartmentName(Arg.Any<uint>(), Arg.Any<uint>())
                .Returns(x => BuildDepartmentName((uint)x[1]));

            return objectResolver;
        }

        [Test]
        public void TestChatWidgetAppearance()
        {
            var oldValue = new ChatWidgetAppearance
                {
                    ThemeId = null,
                    Location = ChatWidgetLocation.BottomRight,
                    OffsetX = 100,
                    OffsetY = 1000,
                    MinimizedStateTitle = null,
                    CustomCssUrl = string.Empty,
                    PoweredByVisible = false
                };
            var newValue = new ChatWidgetAppearance
                {
                    ThemeId = null,
                    Location = ChatWidgetLocation.TopLeft,
                    OffsetX = 102,
                    OffsetY = oldValue.OffsetY,
                    MinimizedStateTitle = "Tit",
                    CustomCssUrl = "Cs",
                    PoweredByVisible = true
                };
            var actual = oldValue.Diff(newValue);
            Assert.NotNull(actual, nameof(actual));

            var stringChanges = new List<PlainFieldChange<string>>
                {
                    new PlainFieldChange<string>(
                        nameof(ChatWidgetAppearance.Location),
                        oldValue.Location.ToString(),
                        newValue.Location.ToString()),
                    new PlainFieldChange<string>(
                        nameof(ChatWidgetAppearance.MinimizedStateTitle),
                        oldValue.MinimizedStateTitle,
                        newValue.MinimizedStateTitle),
                    new PlainFieldChange<string>(
                        nameof(ChatWidgetAppearance.CustomCssUrl),
                        oldValue.CustomCssUrl,
                        newValue.CustomCssUrl)
                };
            var decimalChanges = new List<PlainFieldChange<decimal>>
                {
                    new PlainFieldChange<decimal>(
                        nameof(ChatWidgetAppearance.OffsetX),
                        oldValue.OffsetX,
                        newValue.OffsetX)
                };

            var expected = new FieldChanges
                {
                    BoolChanges = new List<PlainFieldChange<bool>>
                        {
                            new PlainFieldChange<bool>(
                                nameof(ChatWidgetAppearance.PoweredByVisible),
                                oldValue.PoweredByVisible,
                                newValue.PoweredByVisible)
                        },
                    DecimalChanges = decimalChanges,
                    StringChanges = stringChanges
                };
            actual.Should().BeEquivalentTo(expected, "Before changes");


            newValue.ThemeId = string.Empty;
            stringChanges.Add(
                new PlainFieldChange<string>(
                    nameof(ChatWidgetAppearance.ThemeId),
                    oldValue.ThemeId,
                    newValue.ThemeId));

            newValue.OffsetY = oldValue.OffsetY + 10;
            decimalChanges.Add(
                new PlainFieldChange<decimal>(
                    nameof(ChatWidgetAppearance.OffsetY),
                    oldValue.OffsetY,
                    newValue.OffsetY));

            var actual2 = oldValue.Diff(newValue);
            Assert.NotNull(actual2, nameof(actual2));
            actual2.Should().BeEquivalentTo(expected, "After changes");
        }

        [Test]
        public void TestCustomer()
        {
            const string same1 = "did";
            const string delete1 = "do";
            const string delete2 = "do2";
            const string delete3 = "do3";
            var oldValue = new CustomerInfo
                {
                    Id = 10,
                    AddTimestampUtc = new DateTime(20, 8, 25).ToUniversalTime(),
                    UpdateTimestampUtc = new DateTime(10, 5, 27).ToUniversalTime(),
                    Status = ObjectStatus.Disabled,
                    Name = "Name1",
                    Domains = new[] { delete1, same1, delete2, delete3 },
                    CreateIp = "p 1"
                };

            const string insert1 = "does";
            const string insert2 = "done";
            var newValue = new CustomerInfo
                {
                    Id = 20,
                    AddTimestampUtc = oldValue.AddTimestampUtc.AddDays(3),
                    UpdateTimestampUtc = oldValue.UpdateTimestampUtc.AddDays(10),
                    Status = ObjectStatus.Active,
                    Name = "Name2",
                    Domains = new[] { insert1, same1, insert2 },
                    CreateIp = "p 23"
                };
            var actual = oldValue.Diff(newValue);
            Assert.NotNull(actual, nameof(actual));

            var expected = new FieldChanges
                {
                    StringChanges = new List<PlainFieldChange<string>>
                        {
                            new PlainFieldChange<string>(
                                nameof(CustomerInfo.Status),
                                oldValue.Status.ToString(),
                                newValue.Status.ToString()),
                            new PlainFieldChange<string>(
                                nameof(CustomerInfo.Name),
                                oldValue.Name,
                                newValue.Name),
                            new PlainFieldChange<string>(
                                nameof(CustomerInfo.CreateIp),
                                oldValue.CreateIp,
                                newValue.CreateIp)
                        },
                    DateTimeChanges = new List<PlainFieldChange<DateTime>>
                        {
                            new PlainFieldChange<DateTime>(
                                nameof(CustomerInfo.UpdateTimestampUtc),
                                oldValue.UpdateTimestampUtc,
                                newValue.UpdateTimestampUtc)
                        },
                    StringListChanges = new List<ListChange<string>>
                        {
                            new ListChange<string>(
                                nameof(CustomerInfo.Domains),
                                new List<string> { delete3, delete1, delete2 },
                                new List<string> { insert2, insert1 }
                            )
                        }
                };
            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void TestDepartment()
        {
            var oldValue = new DepartmentInfo
                {
                    Id = 34,
                    CustomerId = 12,
                    Description = null,
                    IsPublic = true,
                    Name = "Name1",
                    Status = ObjectStatus.Disabled
                };
            var newValue = new DepartmentInfo
                {
                    Id = 35,
                    CustomerId = 714,
                    Description = "Desrc2",
                    IsPublic = false,
                    Name = "Name1 ",
                    Status = ObjectStatus.Active
                };
            var actual = oldValue.Diff(newValue);
            Assert.NotNull(actual, nameof(actual));

            var expected = new FieldChanges
                {
                    BoolChanges = new List<PlainFieldChange<bool>>
                        {
                            new PlainFieldChange<bool>(
                                nameof(DepartmentInfo.IsPublic),
                                oldValue.IsPublic,
                                newValue.IsPublic)
                        },
                    StringChanges = new List<PlainFieldChange<string>>
                        {
                            new PlainFieldChange<string>(
                                nameof(DepartmentInfo.Status),
                                oldValue.Status.ToString(),
                                newValue.Status.ToString()),
                            new PlainFieldChange<string>(
                                nameof(DepartmentInfo.Name),
                                oldValue.Name,
                                newValue.Name),
                            new PlainFieldChange<string>(
                                nameof(DepartmentInfo.Description),
                                oldValue.Description,
                                newValue.Description)
                        }
                };
            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void TestUser()
        {
            var objectResolver = CreateNameResolver();

            const int deleted1 = 20;
            const int deleted2 = 4000;

            const int same1 = 1;
            const int same2 = 300;
            var oldValue = new UserInfo
                {
                    Id = 34,
                    CustomerId = CustomerId,
                    AddTimestampUtc = new DateTime(107, 4, 27).ToUniversalTime(),
                    UpdateTimestampUtc = new DateTime(10, 3, 27).ToUniversalTime(),
                    Status = ObjectStatus.Disabled,
                    FirstName = "FN",
                    LastName = "LN",
                    Email = "ma",
                    IsOwner = false,
                    IsAdmin = false,
                    AgentDepartments = null,
                    SupervisorDepartments = new HashSet<uint> { same1, deleted1, same2, deleted2 },
                    Avatar = "Av"
                };

            const int inserted1 = 120;
            const int inserted2 = 450;
            const int inserted3 = 74001;
            var newValue = new UserInfo
                {
                    Id = oldValue.Id,
                    CustomerId = oldValue.CustomerId,
                    AddTimestampUtc = oldValue.AddTimestampUtc.AddDays(1008),
                    UpdateTimestampUtc = oldValue.UpdateTimestampUtc.AddDays(100),
                    Status = ObjectStatus.Active,
                    FirstName = "FN ",
                    LastName = " LN",
                    Email = "man",
                    IsOwner = true,
                    IsAdmin = true,
                    AgentDepartments = new HashSet<uint> { 50, 600 },
                    SupervisorDepartments = new HashSet<uint> { same1, inserted1, inserted2, inserted3, same2 },
                    Avatar = "Aa"
                };
            var actual = oldValue.Diff(newValue, objectResolver);
            Assert.NotNull(actual, nameof(actual));

            var insertedAgentDepartments = newValue.AgentDepartments
                .Select(IdToPair)
                .OrderByDescending(a => a.Id)
                .ToList();

            var deletedSupervisorDepartments = new List<pair>
                {
                    //Change the order
                    IdToPair(deleted2),
                    IdToPair(deleted1)
                };
            var insertedSupervisorDepartments = new List<pair>
                {
                    IdToPair(inserted3),
                    IdToPair(inserted1),
                    IdToPair(inserted2)
                };

            var expected = new FieldChanges
                {
                    BoolChanges = new List<PlainFieldChange<bool>>
                        {
                            new PlainFieldChange<bool>(
                                nameof(UserInfo.IsAdmin),
                                oldValue.IsAdmin,
                                newValue.IsAdmin),
                            new PlainFieldChange<bool>(
                                nameof(UserInfo.IsOwner),
                                oldValue.IsOwner,
                                newValue.IsOwner)
                        },
                    DateTimeChanges = new List<PlainFieldChange<DateTime>>
                        {
                            new PlainFieldChange<DateTime>(
                                nameof(UserInfo.UpdateTimestampUtc),
                                oldValue.UpdateTimestampUtc,
                                newValue.UpdateTimestampUtc)
                        },
                    StringChanges = new List<PlainFieldChange<string>>
                        {
                            new PlainFieldChange<string>(
                                nameof(UserInfo.Avatar),
                                oldValue.Avatar,
                                newValue.Avatar),
                            new PlainFieldChange<string>(
                                nameof(UserInfo.Email),
                                oldValue.Email,
                                newValue.Email),
                            new PlainFieldChange<string>(
                                nameof(UserInfo.FirstName),
                                oldValue.FirstName,
                                newValue.FirstName),
                            new PlainFieldChange<string>(
                                nameof(UserInfo.LastName),
                                oldValue.LastName,
                                newValue.LastName),
                            new PlainFieldChange<string>(
                                nameof(UserInfo.Status),
                                oldValue.Status.ToString(),
                                newValue.Status.ToString())
                        },
                    IdListChanges = new List<IdListChange>
                        {
                            new IdListChange(nameof(UserInfo.SupervisorDepartments), deletedSupervisorDepartments, insertedSupervisorDepartments),
                            new IdListChange(nameof(UserInfo.AgentDepartments), null, insertedAgentDepartments)
                        }
                };
            actual.Should().BeEquivalentTo(expected);
        }
    }
}