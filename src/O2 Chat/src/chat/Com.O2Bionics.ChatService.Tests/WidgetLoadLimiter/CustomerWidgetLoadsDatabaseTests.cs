using System.Collections.Generic;
using System.Linq;
using Com.O2Bionics.ChatService.Contract.Widget;
using Com.O2Bionics.ChatService.Impl.Storage;
using Com.O2Bionics.Tests.Common;
using FluentAssertions;
using NUnit.Framework;

namespace Com.O2Bionics.ChatService.Tests.WidgetLoadLimiter
{
    [TestFixture]
    public sealed class CustomerWidgetLoadsDatabaseTests : WidgetTestBase
    {
        private readonly CustomerWidgetLoadStorage m_dataLogic = new CustomerWidgetLoadStorage();

        [Test]
        public void OneDay()
        {
            const int attempts = 5;
            const long increment = 3;

            var endDate = Date.AddDays(1);

            for (var attempt = 0; attempt < attempts; attempt++)
            {
                using (var dataContext = DatabaseFactory.CreateContext())
                {
                    var old = m_dataLogic.GetForCustomer(dataContext.Db, TestConstants.CustomerId, Date, endDate).ToList();
                    if (0 == attempt)
                        Assert.AreEqual(0, old.Count, "Initially there must be no record.");
                    else
                    {
                        Assert.AreEqual(1, old.Count, $"There must be 1 record, attempt={attempt}.");
                        Assert.AreEqual(increment * attempt, old[0].Count, $"Get, attempt={attempt}.");
                    }

                    var actual = m_dataLogic.Update(dataContext.Db, TestConstants.CustomerId, Date, increment, false);
                    var count = actual.Key;
                    Assert.AreEqual((attempt + 1) * increment, count, $"After update, attempt={attempt}.");
                    dataContext.Commit();
                }
            }
        }

        [Test]
        public void SeveralDays()
        {
            const int attempts = 5;
            const long increment = 3;

            var expected = new List<WidgetViewStatisticsEntry>();

            for (var attempt = 0; attempt < attempts; attempt++)
            {
                using (var dataContext = DatabaseFactory.CreateContext())
                {
                    var date = Date.AddDays(attempt);
                    var endDate = date.AddDays(1);
                    {
                        var loads = m_dataLogic.GetForCustomer(dataContext.Db, TestConstants.CustomerId, date, endDate).ToList();
                        Assert.AreEqual(0, loads.Count, $"Initially there must be no record, attempt={attempt}");
                    }

                    var actual = m_dataLogic.Update(dataContext.Db, TestConstants.CustomerId, date, increment, false);
                    var count = actual.Key;
                    Assert.AreEqual(increment, count, $"After update, attempt={attempt}.");
                    expected.Add(new WidgetViewStatisticsEntry { Count = increment, Date = date, CustomerId = TestConstants.CustomerId });
                    {
                        var loads = m_dataLogic.GetForCustomer(dataContext.Db, TestConstants.CustomerId, Date, endDate);
                        expected.Should().BeEquivalentTo(loads, $"Get after update, attempt={attempt}.");
                    }

                    dataContext.Commit();
                }
            }
        }
    }
}