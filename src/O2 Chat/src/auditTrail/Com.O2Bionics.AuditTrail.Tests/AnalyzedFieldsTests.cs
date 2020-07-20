using System;
using System.Collections.Generic;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.AuditTrail.Tests.Utils;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.WidgetAppearance;
using Com.O2Bionics.ChatService.Impl.AuditTrail;
using NUnit.Framework;
using WidgetAppearance_t = System.ValueTuple<string, Com.O2Bionics.ChatService.Contract.WidgetAppearance.ChatWidgetAppearance,
    Com.O2Bionics.ChatService.Contract.WidgetAppearance.ChatWidgetAppearance, string, string>;
using Department_t = System.ValueTuple<string, Com.O2Bionics.ChatService.Contract.DepartmentInfo,
    Com.O2Bionics.ChatService.Contract.DepartmentInfo, string, string>;
using User_t = System.ValueTuple<string, Com.O2Bionics.ChatService.Contract.UserInfo,
    Com.O2Bionics.ChatService.Contract.UserInfo, string, string>;
using Customer_t = System.ValueTuple<string, Com.O2Bionics.ChatService.Contract.CustomerInfo,
    Com.O2Bionics.ChatService.Contract.CustomerInfo, string, string>;

namespace Com.O2Bionics.AuditTrail.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public sealed class AnalyzedFieldsTests
    {
        private const int CustomerId = 3467901;

        private readonly TestNameResolver m_nameResolver = new TestNameResolver();

        private static readonly ChatWidgetAppearance m_oldWidgetAppearance = new ChatWidgetAppearance
            {
                ThemeId = "ThemOne",
                ThemeMinId = "Min1",
                Location = ChatWidgetLocation.BottomRight,
                OffsetX = 100,
                OffsetY = 67,
                MinimizedStateTitle = "Ra",
                CustomCssUrl = "O key",
                PoweredByVisible = false
            };

        private const string OldWidgetAppearanceAsString = "ThemOne Min1 BottomRight 100 67 Ra O key";

        private static readonly WidgetAppearance_t[] m_widgetAppearances =
            {
                new WidgetAppearance_t(
                    "Few fields",
                    m_oldWidgetAppearance,
                    new ChatWidgetAppearance
                        {
                            ThemeId = "ThemOne",
                            ThemeMinId = "Three Maximum",
                            Location = ChatWidgetLocation.BottomRight,
                            OffsetX = 100,
                            OffsetY = 67,
                            MinimizedStateTitle = "Operation",
                            CustomCssUrl = "Cs max state min",
                            PoweredByVisible = true
                        },
                    OldWidgetAppearanceAsString
                    + " ThemOne Three Maximum BottomRight 100 67 Operation Cs max state min",
                    "Min1 Three Maximum Ra Operation O key Cs max state min"
                ),
                new WidgetAppearance_t(
                    "All fields",
                    m_oldWidgetAppearance,
                    new ChatWidgetAppearance
                        {
                            ThemeId = "Two Them",
                            ThemeMinId = "Three Minimum",
                            Location = ChatWidgetLocation.TopLeft,
                            OffsetX = 6476764,
                            OffsetY = 112794978,
                            MinimizedStateTitle = "Titanic sample",
                            CustomCssUrl = "Cs max state min",
                            PoweredByVisible = true
                        },
                    OldWidgetAppearanceAsString
                    + " Two Them Three Minimum TopLeft 6476764 112794978 Titanic sample Cs max state min",
                    "100 6476764 67 112794978 ThemOne Two Them Min1 Three Minimum BottomRight TopLeft "
                    + "Ra Titanic sample O key Cs max state min"
                )
            };


        private static readonly DepartmentInfo m_oldDepartment = new DepartmentInfo
            {
                CustomerId = CustomerId,
                Id = 50,
                Status = ObjectStatus.Disabled,
                IsPublic = false,
                Name = "Piter",
                Description = "Some thing "
            };

        private const string OldDepartmentAsString = "Disabled Piter Some thing ";

        private static readonly Department_t[] m_departments =
            {
                new Department_t(
                    "Some fields",
                    m_oldDepartment,
                    new DepartmentInfo
                        {
                            CustomerId = CustomerId,
                            Id = m_oldDepartment.Id,
                            Status = ObjectStatus.Active,
                            IsPublic = !m_oldDepartment.IsPublic,
                            Name = "Peter",
                            Description = m_oldDepartment.Description
                        },
                    OldDepartmentAsString
                    + " Active Peter Some thing ",
                    "Disabled Active Piter Peter"
                ),
                new Department_t(
                    "All fields",
                    m_oldDepartment,
                    new DepartmentInfo
                        {
                            CustomerId = CustomerId,
                            Id = m_oldDepartment.Id,
                            Status = ObjectStatus.Active,
                            IsPublic = !m_oldDepartment.IsPublic,
                            Name = "Peter",
                            Description = "Some things"
                        },
                    OldDepartmentAsString
                    + " Active Peter Some things",
                    "Disabled Active Piter Peter Some thing  Some things"
                )
            };


        private static readonly CustomerInfo m_oldCustomer = new CustomerInfo
            {
                Id = CustomerId,
                AddTimestampUtc = new DateTime(2000, 1, 1),
                UpdateTimestampUtc = new DateTime(3005, 1, 1),
                Status = ObjectStatus.Deleted,
                Name = "Pete",
                Domains = new[] { "Dome 2", "dom1" },
                CreateIp = "127.0.0.10"
            };

        private const string OldCustomerAsString = "Deleted Pete Dome 2 dom1 127.0.0.10";

        private static readonly Customer_t[] m_customers =
            {
                new Customer_t(
                    "Some fields",
                    m_oldCustomer,
                    new CustomerInfo
                        {
                            Id = CustomerId,
                            AddTimestampUtc = m_oldCustomer.AddTimestampUtc,
                            UpdateTimestampUtc = m_oldCustomer.UpdateTimestampUtc.AddDays(123),
                            Status = m_oldCustomer.Status,
                            Name = "Potter",
                            Domains = new[] { "Dome 2", "d4", "d5", "Dom1" },
                            CreateIp = m_oldCustomer.CreateIp
                        },
                    OldCustomerAsString
                    + " Deleted Potter Dome 2 d4 d5 Dom1 127.0.0.10",
                    "Pete Potter dom1 d4 d5 Dom1"
                ),
                new Customer_t(
                    "All fields",
                    m_oldCustomer,
                    new CustomerInfo
                        {
                            Id = CustomerId,
                            AddTimestampUtc = m_oldCustomer.AddTimestampUtc,
                            UpdateTimestampUtc = m_oldCustomer.UpdateTimestampUtc.AddDays(123),
                            Status = ObjectStatus.Active,
                            Name = "Potter",
                            Domains = new[] { "home 1, Dome 2", "Dome 2", "Darth 3 4 7" },
                            CreateIp = "27.0.0.250"
                        },
                    OldCustomerAsString
                    + " Active Potter home 1, Dome 2 Dome 2 Darth 3 4 7 27.0.0.250",
                    "Deleted Active Pete Potter 127.0.0.10 27.0.0.250 dom1 home 1, Dome 2 Darth 3 4 7"
                )
            };

        private static readonly UserInfo m_oldUser = new UserInfo
            {
                CustomerId = CustomerId,
                Id = 50,
                AddTimestampUtc = new DateTime(2000, 1, 1),
                UpdateTimestampUtc = new DateTime(3005, 1, 1),
                Status = ObjectStatus.NotConfirmed,
                FirstName = "Piter",
                LastName = "Marv",
                Email = "mail1@aa.bb",
                IsOwner = false,
                IsAdmin = true,
                Avatar = "Aw",
                AgentDepartments = new HashSet<uint> { 123412, 3456546, 98674578, 467846780 },
                SupervisorDepartments = new HashSet<uint> { 3456546, 45, 98, 19 }
            };

        private const string OldUserAsString = "NotConfirmed Piter Marv mail1@aa.bb Aw";

        private static readonly User_t[] m_users =
            {
                new User_t(
                    "All fields",
                    m_oldUser,
                    new UserInfo
                        {
                            CustomerId = CustomerId,
                            Id = m_oldUser.Id,
                            AddTimestampUtc = m_oldCustomer.AddTimestampUtc,
                            UpdateTimestampUtc = m_oldCustomer.UpdateTimestampUtc.AddDays(123),
                            Status = ObjectStatus.Active,
                            FirstName = "Peter",
                            LastName = "Marvel",
                            Email = "mail2@aa.bb",
                            IsOwner = !m_oldUser.IsOwner,
                            IsAdmin = !m_oldUser.IsAdmin,
                            Avatar = "Awe",
                            AgentDepartments = new HashSet<uint>
                                {
                                    //98674578,
                                    50, //Add
                                    123412,
                                    3456546,
                                    467846780
                                },
                            SupervisorDepartments = new HashSet<uint>
                                {
                                    //19 
                                    34576, //Add
                                    3456546,
                                    45,
                                    98
                                }
                        },
                    OldUserAsString
                    + " Active Peter Marvel mail2@aa.bb Awe"
                    + " Depart123412 Depart19 Depart3456546 Depart34576 Depart45 Depart467846780 Depart50 Depart98 Depart98674578",
                    "NotConfirmed Active Piter Peter Marv Marvel mail1@aa.bb mail2@aa.bb Aw Awe Depart98674578 Depart50 Depart19 Depart34576"
                )
            };

        private void TestImpl<T>(
            T oldValue,
            T newValue,
            string allExpected,
            string changedExpected,
            Func<T, T, FieldChanges> diff)
            where T : class
        {
            var auditEvent = new AuditEvent<T>
                {
                    OldValue = oldValue,
                    NewValue = newValue,
                    FieldChanges = diff(oldValue, newValue)
                };
            auditEvent.SetAnalyzedFields(true);
            Assert.AreEqual(allExpected, auditEvent.All, nameof(auditEvent.All));
            Assert.AreEqual(changedExpected, auditEvent.Changed, nameof(auditEvent.Changed));
        }

        private void TestImpl<T>(
            T oldValue,
            T newValue,
            string allExpected,
            string changedExpected,
            Func<T, T, INameResolver, FieldChanges> diff)
            where T : class
        {
            var auditEvent = new AuditEvent<T>
                {
                    OldValue = oldValue,
                    NewValue = newValue,
                    FieldChanges = diff(oldValue, newValue, m_nameResolver)
                };
            if (typeof(UserInfo) == typeof(T))
            {
                var userChangeEvent = auditEvent as AuditEvent<UserInfo>;
                Assert.IsNotNull(userChangeEvent, nameof(userChangeEvent));
                userChangeEvent.FetchDictionaries(m_nameResolver);
            }

            auditEvent.SetAnalyzedFields(true);
            Assert.AreEqual(allExpected, auditEvent.All, nameof(auditEvent.All));
            Assert.AreEqual(changedExpected, auditEvent.Changed, nameof(auditEvent.Changed));
        }

        [Test]
        [TestCaseSource(nameof(m_customers))]
        public void TestCustomer(Customer_t testCase)
        {
            TestImpl(testCase.Item2, testCase.Item3, testCase.Item4, testCase.Item5, SpecificClassDiff.Diff);
        }

        [Test]
        [TestCaseSource(nameof(m_departments))]
        public void TestDepartment(Department_t testCase)
        {
            TestImpl(testCase.Item2, testCase.Item3, testCase.Item4, testCase.Item5, SpecificClassDiff.Diff);
        }

        [Test]
        [TestCaseSource(nameof(m_users))]
        public void TestUser(User_t testCase)
        {
            TestImpl(testCase.Item2, testCase.Item3, testCase.Item4, testCase.Item5, SpecificClassDiff.Diff);
        }

        [Test]
        [TestCaseSource(nameof(m_widgetAppearances))]
        public void TestWidgetAppearance(WidgetAppearance_t testCase)
        {
            TestImpl(testCase.Item2, testCase.Item3, testCase.Item4, testCase.Item5, SpecificClassDiff.Diff);
        }
    }
}