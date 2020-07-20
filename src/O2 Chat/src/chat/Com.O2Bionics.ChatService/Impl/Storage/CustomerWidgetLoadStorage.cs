using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Com.O2Bionics.ChatService.Contract.Widget;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Properties;
using LinqToDB;
using LinqToDB.Data;

namespace Com.O2Bionics.ChatService.Impl.Storage
{
    /// <summary>
    /// Wrapper around Oracle table.
    /// </summary>
    public sealed class CustomerWidgetLoadStorage : ICustomerWidgetLoadStorage
    {
        public IEnumerable<WidgetViewStatisticsEntry> GetForCustomer(
            ChatDatabase db,
            uint customerId,
            DateTime beginDate,
            DateTime endDate)
        {
            Debug.Assert(beginDate < endDate);
            Debug.Assert(beginDate == beginDate.RemoveTime());
            Debug.Assert(endDate == endDate.RemoveTime());

            var result = db.WIDGET_LOAD
                .Where(c => c.CUSTOMER_ID == customerId && beginDate <= c.UPDATED && c.UPDATED < endDate)
                .Select(
                    c => new WidgetViewStatisticsEntry
                        {
                            CustomerId = c.CUSTOMER_ID,
                            Date = c.UPDATED,
                            Count = decimal.ToInt64(c.LOADS),
                            IsViewCountExceeded = 0 != c.ISOVERLOAD
                        }).OrderByDescending(c => c.Date);
            return result;
        }

        public IEnumerable<WidgetViewStatisticsEntry> Get(ChatDatabase db, DateTime date)
        {
            Debug.Assert(date == date.RemoveTime());

            var result = db.WIDGET_LOAD
                .Where(c => c.UPDATED == date)
                .Select(
                    c => new WidgetViewStatisticsEntry
                        {
                            CustomerId = c.CUSTOMER_ID,
                            Date = date,
                            Count = decimal.ToInt64(c.LOADS),
                            IsViewCountExceeded = 0 != c.ISOVERLOAD
                        });
            return result;
        }

        public KeyValuePair<long, bool> Update(ChatDatabase db, uint customerId, DateTime date, long increment, bool isViewCountExceeded)
        {
            if (increment < 0)
                throw new ArgumentException(string.Format(Resources.ArgumentMustBeNonNegative2, nameof(increment), increment));
            Debug.Assert(date == date.RemoveTime());

            const string procedureName = "ADD_UPDATE_WIDGET_LOADS";
            var outLoad = new DataParameter { Name = "pLOADS", Direction = ParameterDirection.Output, DataType = DataType.Int64 };
            var outIsSet = new DataParameter
                {
                    Name = "pISSET",
                    Direction = ParameterDirection.Output,
                    DataType = DataType.Int32,
                };
            var parameters = new[]
                {
                    new DataParameter("pCUSTOMER_ID", customerId),
                    new DataParameter("pUPDATED", date, DataType.Date),
                    new DataParameter("pINCREMENT", increment),
                    new DataParameter("pISOVERLOAD", isViewCountExceeded ? 1 : 0),
                    outLoad,
                    outIsSet,
                };
            db.ExecuteProc(procedureName, parameters);

            var counter = (long)outLoad.Value;
            var isSet = (int)outIsSet.Value;
            return new KeyValuePair<long, bool>(counter, 0 != isSet);
        }
    }
}