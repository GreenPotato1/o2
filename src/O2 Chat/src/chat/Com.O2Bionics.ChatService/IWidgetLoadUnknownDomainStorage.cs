using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService
{
    public interface IWidgetLoadUnknownDomainStorage : INotifyableStorage, IDailyCache
    {
        Task LoadMany([NotNull] uint[] customerIds);

        /// <summary>
        /// Return whether too many unknown domains for the given date and
        /// customer.
        /// </summary>
        Task<bool> Add(DateTime date, uint customerId, string domains, [NotNull] string unknownDomain, bool throwOnException = false);

        bool HasEvent(DateTime date, uint customerId, [CanBeNull] out HashSet<string> unknownDomains);
    }
}