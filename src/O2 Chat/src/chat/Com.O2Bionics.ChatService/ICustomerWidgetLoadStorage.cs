using System;
using System.Collections.Generic;
using Com.O2Bionics.ChatService.Contract.Widget;
using Com.O2Bionics.ChatService.DataModel;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService
{
    public interface ICustomerWidgetLoadStorage
    {
        [NotNull]
        IEnumerable<WidgetViewStatisticsEntry> GetForCustomer([NotNull] ChatDatabase db, uint customerId, DateTime beginDate, DateTime endDate);

        [NotNull]
        IEnumerable<WidgetViewStatisticsEntry> Get([NotNull] ChatDatabase db, DateTime date);

        /// <summary>
        /// Return the current load, and whether "is over the daily limit" has
        /// changed.
        /// </summary>
        KeyValuePair<long, bool> Update([NotNull] ChatDatabase db, uint customerId, DateTime date, long increment, bool isViewCountExceeded);
    }
}