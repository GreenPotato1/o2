using System;
using System.Collections.Generic;
using Com.O2Bionics.ChatService.Contract.Widget;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using NUnit.Framework;
using IsViewCountExceeded = System.Collections.Generic.KeyValuePair<long, bool>;

namespace Com.O2Bionics.ChatService.Tests.WidgetLoadLimiter.Mocks
{
    public sealed class CustomerWidgetLoadStorageMock : ICustomerWidgetLoadStorage
    {
        private readonly object m_locker = new object();
        private readonly Dictionary<DateTime, IsViewCountExceeded> m_data = new Dictionary<DateTime, IsViewCountExceeded>();
        private int m_updateCalls;

        public int UpdateCalls
        {
            get
            {
                lock (m_locker)
                {
                    return m_updateCalls;
                }
            }
        }

        public Dictionary<DateTime, IsViewCountExceeded> Data
        {
            get
            {
                lock (m_locker)
                {
                    var result = new Dictionary<DateTime, IsViewCountExceeded>(m_data);
                    return result;
                }
            }
        }

        public IEnumerable<WidgetViewStatisticsEntry> GetForCustomer(ChatDatabase db, uint customerId, DateTime beginDate, DateTime endDate)
        {
            yield break;
        }

        public IEnumerable<WidgetViewStatisticsEntry> Get(ChatDatabase db, DateTime date)
        {
            yield break;
        }

        public IsViewCountExceeded Update(ChatDatabase db, uint customerId, DateTime date, long increment, bool isViewCountExceeded)
        {
            Assert.AreEqual(date, date.RemoveTime(), "Date must be without seconds.");
            Assert.AreEqual(TestConstants.CustomerId, customerId, nameof(customerId));
            Assert.Greater(increment, 0, nameof(increment));

            lock (m_locker)
            {
                ++m_updateCalls;

                if (m_data.TryGetValue(date, out var value))
                {
                    increment += value.Key;
                    isViewCountExceeded = isViewCountExceeded || value.Value; // Cannot change from true to false.
                }

                var result = new IsViewCountExceeded(increment, isViewCountExceeded);
                m_data[date] = result;
                return result;
            }
        }
    }
}