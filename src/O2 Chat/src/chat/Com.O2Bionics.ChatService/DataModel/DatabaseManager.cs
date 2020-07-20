using System;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Impl;
using Com.O2Bionics.ChatService.Properties;
using Com.O2Bionics.Utils;
using log4net;
using OH = Com.O2Bionics.ChatService.DataModel.DatabaseObjectHelper;

namespace Com.O2Bionics.ChatService.DataModel
{
    public class DatabaseManager : DatabaseManagerBase<LinqToDB.IDataContext>
    {
        //TODO. Delete these two and use "Com.O2Bionics.Tests.Common.TestConstants".
        private const string CustomerMainDomain = "o2bionics.com";

        //allows multiple values with CustomerConstants.DomainSeparator as delimiter
        private static string CustomerDomains => CustomerMainDomain + DomainUtilities.DomainSeparator + "chat.o2bionics.com";

        public DatabaseManager(string connectionString, bool logQueries = true)
            : base(connectionString, logQueries, LogManager.GetLogger(typeof(DatabaseManager)))
        {
        }

        protected override string[] Tables { get; } =
            {
                "CUSTOMER",
                "CUSTOMER_USER",
                "PROPERTY_BAG",
                "FORGOT_PASSWORD",
                "AGENT_SESSION",
                "DEPARTMENT",
                "USER_ROLE",
                "VISITOR",
                "CHAT_SESSION",
                "CHAT_EVENT",
                "CANNED_MESSAGE",
                "WIDGET_LOAD",
            };

        protected override string SchemaScript { get; } = Resources.database;
        protected override string ProceduresScript { get; } = Resources.procedures;

        protected override void ExecuteInContext(Action<LinqToDB.IDataContext> action)
        {
            new ChatDatabaseFactory(ConnectionString, LogQueries).Query(action);
        }

        protected override void InsertInitialData(LinqToDB.IDataContext db, DateTime now)
        {
            //Customer1(db, now);
            //Customer2(db, now);
            CustomerO2Bionics(db, now);
            CustomerAntonMarkov(db, now);
        }

        private void CustomerAntonMarkov(LinqToDB.IDataContext db, DateTime now)
        {
            var customer = OH.Customer(now, 2, "Центр гипноза Антона Маркова", CustomerDomains);
            InsertCustomer(db, customer, "antonmarkov.com;hypnosdoma.com");

            var departments = new[]
                {
                    OH.Department(now, 3, customer, "Центр гипноза Антона Маркова", "Offline Department Description", false),
                    OH.Department(now, 4, customer,"Отдел продаж", "Описание отдела продаж", true),
                    OH.Department(now, 5, customer, "Отдел дистанционного обучения", "Описание дистанционного обучения", true),
                    OH.Department(now, 6, customer, "Закрытая группа Центра Обучения", "Описание закрытой группы Центра Обучения", false),
                    
                };
            Insert(db, departments);
            var users = new[]
                {
                    OH.User(now, 3, customer, "nicile@antonmarkov.com", "p1", "Антон", "Марков"),
                    OH.User(now, 4, customer, "bav@antonmarkov.com", "bandrei1","Андрей", "Бобров"),
                    //OH.User(now, 3, customer, "bobrik_va@o2bionics.com", "bobrik_va", "Victor", "Bobrik"),
                    //OH.User(now, 4, customer, "matushevskay_da@o2bionics.com", "matushevskay_da", "Diana", "Matushevskay"),
                    //OH.User(now, 5, customer, "agent5@test.o2bionics.com", "p5", "Steve", "Jobs"),
                    //OH.User(now, 6, customer, "prusskya_td@o2bionics.com", "prusskya_td", "Tatyana", "Prusskya"),
                    //OH.User(now, 7, customer, "rabushko_as@o2bionics.com", "rabushko_as", "rabushko_as", "rabushko_as"),
                    //OH.User(now, 8, customer, "sturlis_us@o2bionics.com", "sturlis_us", "Ullia", "Sturlis"),
                    //OH.User(now, 9, customer, "agent9@test.o2bionics.com", "p9", "Denis", "Prox")
                };
            Insert(db, users);

            var noDepartments = new DEPARTMENT[0];
            var roles = Enumerable.Empty<USER_ROLE>()
                .Concat(OH.CreateUserRoles(now, users[0], true, true, departments, noDepartments))
                .Concat(OH.CreateUserRoles(now, users[1], false, false, new[] { departments[0],departments[1], departments[2] }, noDepartments))
                //.Concat(OH.CreateUserRoles(now, users[2], false, false, new[] { departments[1] }, noDepartments))
                //.Concat(OH.CreateUserRoles(now, users[5], true, true, departments, noDepartments))
                //.Concat(OH.CreateUserRoles(now, users[6], true, true, departments, noDepartments))
                //.Concat(OH.CreateUserRoles(now, users[7], true, true, departments, noDepartments))
                //.Concat(OH.CreateUserRoles(now, users[8], true, true, departments, noDepartments))
                .ToArray();
            Insert(db, roles);
        }

        private void CustomerO2Bionics(LinqToDB.IDataContext db, DateTime now)
        {
            var customer = OH.Customer(now, 1, "O2 Bionics Incorporation", CustomerDomains);
            InsertCustomer(db, customer, "o2bionics.com");

            var departments = new[]
                {
                    OH.Department(now, 1, customer, "O2 Bionics Команда", "Все сотрудники O2 Bionics Inc", false),
                    OH.Department(now, 2, customer, "Отдел продаж", "Описание отдела продаж", true),
                    //OH.Department(now, 3, customer, "Отдел поддержки", "Описание отдела поддержки", true),
                    //OH.Department(now, 1, customer,"Отдел продаж", "Описание отдела продаж", true),
                    //OH.Department(now, 4, customer, "Отдел SEO продвижения", "Описание отдела SEO  продвижения", false),
                    
                    //OH.Department(now, 3, customer, "Отдел юридический", "Описание юридического отдела", false),
                    
                    //OH.Department(now, 5, customer, "Отдел контекстной рекламы", "Описание отдела контекстной рекламы", false),
                    //OH.Department(now, 6, customer, "Отдел дизайнерских услуг", "Описание отдела дизайнерских услуг", false),
                    //OH.Department(now, 7, customer, "Отдел дизайнерских услуг", "Описание отдела дизайнерских услуг", false),
                    //OH.Department(now, 8, customer, "Отдел контента", "Описание отдела контента", false),
                    
                };
            Insert(db, departments);

            var users = new[]
                {
                    OH.User(now, 1, customer, "prokhorchik_da@o2bionics.com", "#89_DangerSnake?", "Denis", "Prokhorchik"),
                    OH.User(now, 2, customer, "vorobey_av@o2bionics.com", "5072605","Alexander", "Vorobey"),
                    //OH.User(now, 3, customer, "bobrik_va@o2bionics.com", "bobrik_va", "Victor", "Bobrik"),
                    //OH.User(now, 4, customer, "matushevskay_da@o2bionics.com", "matushevskay_da", "Diana", "Matushevskay"),
                    //OH.User(now, 5, customer, "prusskya_td@o2bionics.com", "prusskya_td", "Tatyana", "Prusskya"),
                    //OH.User(now, 6, customer, "rabushko_as@o2bionics.com", "rabushko_as", "rabushko_as", "rabushko_as"),
                    //OH.User(now, 7, customer, "sturlis_us@o2bionics.com", "sturlis_us", "Ullia", "Sturlis"),
                };
            Insert(db, users);

            var noDepartments = new DEPARTMENT[0];
            var roles = Enumerable.Empty<USER_ROLE>()
                .Concat(OH.CreateUserRoles(now, users[0], true, true, departments, noDepartments))
                .Concat(OH.CreateUserRoles(now, users[1], false, false, new[] {  departments[0]}, noDepartments))
                //.Concat(OH.CreateUserRoles(now, users[2], false, false, new[] { departments[8], departments[2] }, noDepartments))
                //.Concat(OH.CreateUserRoles(now, users[3], true, true, new[] { departments[8], departments[7] }, noDepartments))
                //.Concat(OH.CreateUserRoles(now, users[4], true, true, new[] { departments[8], departments[7] }, noDepartments))
                //.Concat(OH.CreateUserRoles(now, users[5], true, true, new[] { departments[8], departments[7] }, noDepartments))
                //.Concat(OH.CreateUserRoles(now, users[6], true, true, new[] { departments[8] }, noDepartments))
                //.Concat(OH.CreateUserRoles(now, users[7], true, true, departments, noDepartments))
                //.Concat(OH.CreateUserRoles(now, users[8], true, true, departments, noDepartments))
                .ToArray();
            Insert(db, roles);
        }

        private void InsertCustomer(LinqToDB.IDataContext db, CUSTOMER customer, string domains)
        {
            Insert(db, customer);
            Log.InfoFormat("created customer id={0}", customer.ID);

            Insert(db, OH.Property(customer, "IsEnabled", "True"));
            Insert(db, OH.Property(customer, "Domains", domains));

        }

        private void Customer1(LinqToDB.IDataContext db, DateTime now)
        {
            var customer = OH.Customer(now, 1, "Chat Test Customer", CustomerDomains);
            InsertCustomer(db, customer, CustomerDomains);

            var departments = new[]
                {
                    OH.Department(now, 1, customer, "Sales", "Sales Department Description", true),
                    OH.Department(now, 2, customer, "Support", "Support Department Description", true),
                    OH.Department(now, 3, customer, "Private", "Private Department Description", false),
                    OH.Department(now, 4, customer, "Some Offline Dept", "Offline Department Description", true),
                };
            Insert(db, departments);

            var users = new[]
                {
                    OH.User(now, 1, customer, "agent1@test.o2bionics.com", "p1", "Vasily", "Pupkin"),
                    OH.User(now, 2, customer, "agent2@test.o2bionics.com", "p2", "James", "Bond"),
                    OH.User(now, 3, customer, "agent3@test.o2bionics.com", "p3", "Georg", "Banned", ObjectStatus.Disabled),
                    OH.User(now, 4, customer, "agent4@test.o2bionics.com", "p4", "Bill", "Gates"),
                    OH.User(now, 5, customer, "agent5@test.o2bionics.com", "p5", "Steve", "Jobs"),
                    OH.User(now, 6, customer, "agent6@test.o2bionics.com", "p6", "Mike", "Wazowski"),
                    OH.User(now, 7, customer, "agent7@test.o2bionics.com", "p7", "Freddy", "Krueger"),
                    OH.User(now, 8, customer, "agent8@test.o2bionics.com", "p8", "Max", "Snake"),
                    OH.User(now, 9, customer, "agent9@test.o2bionics.com", "p9", "Denis", "Prox")
                };
            Insert(db, users);

            var noDepartments = new DEPARTMENT[0];
            var roles = Enumerable.Empty<USER_ROLE>()
                .Concat(OH.CreateUserRoles(now, users[0], true, true, departments, noDepartments))
                .Concat(OH.CreateUserRoles(now, users[1], false, false, new[] { departments[0] }, noDepartments))
                .Concat(OH.CreateUserRoles(now, users[2], false, false, new[] { departments[1] }, noDepartments))
                .Concat(OH.CreateUserRoles(now, users[5], true, true, departments, noDepartments))
                .Concat(OH.CreateUserRoles(now, users[6], true, true, departments, noDepartments))
                .Concat(OH.CreateUserRoles(now, users[7], true, true, departments, noDepartments))
                .Concat(OH.CreateUserRoles(now, users[8], true, true, departments, noDepartments))
                .ToArray();
            Insert(db, roles);
        }

        private void Customer2(LinqToDB.IDataContext db, DateTime now)
        {
            const string domain = "customer2.o2bionics.com";
            var customer = OH.Customer(now, 2, "Second Customer", domain);
            InsertCustomer(db, customer, domain);

            var departments = new[]
                {
                    OH.Department(now, 5, customer, "DoE", "Department of Energy", true),
                };
            Insert(db, departments);

            var users = new[]
                {
                    OH.User(now, 10, customer, "john@" + domain, "jo", "John", "Doe"),
                };
            Insert(db, users);

            var roles = Enumerable.Empty<USER_ROLE>()
                .Concat(OH.CreateUserRoles(now, users[0], true, true, departments, departments))
                .ToArray();
            Insert(db, roles);
        }
    }
}