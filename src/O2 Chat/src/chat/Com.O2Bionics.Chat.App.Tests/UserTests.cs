using System;
using System.Threading.Tasks;
using Com.O2Bionics.Chat.App.Tests.Utilities;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Impl.Storage;
using Com.O2Bionics.ChatService.Objects;
using Com.O2Bionics.Tests.Common;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Com.O2Bionics.Chat.App.Tests
{
    [TestFixture]
    public sealed class UserTests : BaseControllerTest
    {
        [SetUp]
        [TearDown]
        public void TearDown()
        {
            if (!m_isNameReset)
                ResetFirstName();
        }

        private const string NewName = "NewName" + TestConstants.TestUserFirstName2 + "Two";
        private bool m_isNameReset;

        /// <summary>
        /// Return old name.
        /// </summary>
        private string ResetFirstName()
        {
            var userStorage = new UserStorage(new TestNowProvider());
            using (var db = new ChatDatabase(Settings.Database))
            {
                var user = userStorage.GetByEmail(db, TestConstants.TestUserEmail2, false);
                Assert.IsNotNull(user, TestConstants.TestUserEmail2);
                Assert.AreEqual(TestConstants.CustomerId, user.CustomerId, nameof(user.CustomerId));

                var result = user.FirstName;
                if (TestConstants.TestUserFirstName2 != result)
                {
                    var update = new User.UpdateInfo { FirstName = TestConstants.TestUserFirstName2 };
                    var updated = userStorage.Update(db, TestConstants.CustomerId, user.Id, update, true);
                    db.CommitTransaction();

                    Assert.IsNotNull(updated, nameof(updated));
                    Assert.AreEqual(TestConstants.TestUserFirstName2, updated.FirstName, nameof(updated.FirstName));
                }

                m_isNameReset = true;
                return result;
            }
        }

        [ItemNotNull]
        private async Task<UserInfo> GetUser([NotNull] CookiesAndToken cookiesAndToken)
        {
            var result = await ControllerClient.GetUsers(Server, cookiesAndToken);
            Assert.IsNotNull(result.Status, nameof(result.Status));
            Assert.AreEqual(CallResultStatusCode.Success, result.Status.StatusCode, nameof(result.Status));

            Assert.IsNotNull(result.Users, nameof(result.Users));
            Assert.IsNotNull(result.Departments, nameof(result.Departments));

            foreach (var user in result.Users)
                if (TestConstants.TestUserEmail2 == user.Email)
                    return user;

            throw new Exception($"The user '{TestConstants.TestUserEmail2}' must have been returned.");
        }

        private async Task UpdateUser(CookiesAndToken cookiesAndToken, [NotNull] UserInfo user)
        {
            m_isNameReset = false;

            user.FirstName = NewName;

            var result = await ControllerClient.UpdateUser(Server, cookiesAndToken, user);
            Assert.IsNotNull(result.Status, nameof(result.Status));
            Assert.AreEqual(CallResultStatusCode.Success, result.Status.StatusCode, nameof(result.Status));

            Assert.IsNotNull(result.User, "result.User");
            Assert.AreEqual(NewName, result.User.FirstName, $"Updated {nameof(result.User.FirstName)}");
        }

        [Test]
        public async Task UpdateUserTest()
        {
            var cookiesAndToken = await Login();
            var userOld = await GetUser(cookiesAndToken);

            await UpdateUser(cookiesAndToken, userOld);

            var userNew = await GetUser(cookiesAndToken);
            Assert.AreEqual(NewName, userNew.FirstName, $"{nameof(ControllerClient.GetUsers)} returned {userNew.FirstName}.");

            var currentName = ResetFirstName();
            Assert.AreEqual(NewName, currentName, $"{userNew.FirstName} selected from the database.");
        }
    }
}