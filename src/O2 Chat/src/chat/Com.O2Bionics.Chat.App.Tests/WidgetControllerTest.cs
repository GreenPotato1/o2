using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Com.O2Bionics.Chat.App.Tests.Utilities;
using Com.O2Bionics.ChatService.Contract.Widget;
using Com.O2Bionics.ChatService.Impl;
using Com.O2Bionics.ChatService.Impl.Storage;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Network;
using Com.O2Bionics.Utils.Web.Filters;
using NUnit.Framework;
using FluentAssertions;

namespace Com.O2Bionics.Chat.App.Tests
{
    [TestFixture]
    public sealed class WidgetControllerTest : BaseControllerTest
    {
        private readonly DateTime m_date = DateTime.UtcNow.RemoveTime();
        private readonly ChatDatabaseFactory m_databaseFactory;

        public WidgetControllerTest()
        {
            m_databaseFactory = new ChatDatabaseFactory(Settings);
        }

        [SetUp]
        public void SetUp()
        {
            m_databaseFactory.ClearWidgetLoadTable();
            AddData();
        }

        [TearDown]
        public void TearDown()
        {
            m_databaseFactory.ClearWidgetLoadTable();
        }

        private void AddData()
        {
            var storage = new CustomerWidgetLoadStorage();
            using (var dataContext = m_databaseFactory.CreateContext())
            {
                storage.Update(dataContext.Db, TestConstants.CustomerId, m_date, 1, false);
                storage.Update(dataContext.Db, TestConstants.CustomerId, m_date, 9, false);
                dataContext.Commit();
            }
        }

        [Test]
        public async Task PostValid()
        {
            await Run(true);
        }

        [Test]
        public void PostWithoutToken()
        {
            var e = Assert.ThrowsAsync<PostException>(async () => { await Run(true, false); });
            Assert.AreEqual(e.HttpCode, (int)HttpStatusCode.Forbidden, nameof(e.HttpCode) + ", e=" + e);
        }

        /// <summary>
        /// Tests that <seealso cref="AntiForgeryTokenCheckAttribute"/> will reject a get request.
        /// </summary>
        [Test]
        public void GetMustThrow()
        {
            var e = Assert.ThrowsAsync<PostException>(async () => { await Run(false); });
            Assert.AreEqual(e.HttpCode, (int)HttpStatusCode.NotFound, nameof(e.HttpCode) + ", e=" + e);
        }

        private async Task Run(bool isPost, bool setToken = true)
        {
            var cookiesAndToken = await Login();

            var widgetLoadRequest = new WidgetLoadRequest { BeginDate = m_date, EndDate = m_date.AddDays(10) };
            widgetLoadRequest.SetStrings();
            var expected = new List<WidgetViewStatisticsEntry>
                {
                    new WidgetViewStatisticsEntry
                        {
                            CustomerId = TestConstants.CustomerId,
                            Count = 10,
                            IsViewCountExceeded = false,
                            Date = m_date
                        }
                };
            var loads = await ControllerClient.GetWidgetLoads(Server, cookiesAndToken, widgetLoadRequest, isPost, setToken);
            loads.Should().BeEquivalentTo(expected, "Loads");
        }
    }
}