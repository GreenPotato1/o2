using System;
using System.Linq;
using Com.O2Bionics.ChatService.Impl;
using Com.O2Bionics.ChatService.Impl.Storage;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Properties;
using JetBrains.Annotations;
using LinqToDB;

namespace Com.O2Bionics.Chat.App.Tests.Utilities
{
    public static class ChatDatabaseFactoryExtensions
    {
        public static void ClearWidgetLoadTable([NotNull] this ChatDatabaseFactory chatDatabaseFactory)
        {
            using (var dataContext = chatDatabaseFactory.CreateContext())
            {
                dataContext.Db.WIDGET_LOAD.Delete();
                dataContext.Commit();
            }
        }

        public static long SumWidgetLoads(
            [NotNull] this ChatDatabaseFactory chatDatabaseFactory,
            uint customerId = TestConstants.CustomerId,
            int days = 10)
        {
            if (days <= 0)
                throw new ArgumentOutOfRangeException(string.Format(Resources.ArgumentMustBePositive2, nameof(days), days));

            var storage = new CustomerWidgetLoadStorage();
            var date = DateTime.UtcNow.RemoveTime();

            using (var dataContext = chatDatabaseFactory.CreateContext())
            {
                var loads = storage.GetForCustomer(dataContext.Db, TestConstants.CustomerId, date.AddDays(-days), date.AddDays(days));
                var result = loads.Sum(load => load.Count);
                return result;
            }
        }
    }
}