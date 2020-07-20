using System;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService.Contract
{
    public interface ICustomerCacheNotifier : IDisposable
    {
        /// <summary>
        /// Notify all IISs, hosting the Widget "com.o2bionics.chat.c", by sending
        /// the latest counters and unknown domains. It is assumed the customers
        /// exist in the database.
        /// </summary>
        void NotifyMany([NotNull] uint[] customerIds);
    }

    public static class CustomerCacheNotifierExtensions
    {
        public static void Notify([NotNull] this ICustomerCacheNotifier customerCacheNotifier, uint customerId)
        {
            customerCacheNotifier.NotifyMany(new[] { customerId });
        }
    }
}