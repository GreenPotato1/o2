using System;
using System.Threading.Tasks;
using Com.O2Bionics.Utils;

namespace Com.O2Bionics.ChatService
{
    public interface IWidgetLoadCounterStorage : IDisposable, INotifyableStorage, IDailyCache
    {
        /// <summary>
        /// Not to loose the data, the <seealso cref="Save"/> should be called
        /// before.
        /// </summary>
        void LoadMany(uint[] customerIds, bool throwOnException = false);

        /// <summary>
        /// Add the number of page loads for the given customer, which must be active, and date.
        /// </summary>
        /// <returns>Whether the widget can be displayed for the given <paramref name="customerId"/>.</returns>
        Task<bool> Add(uint customerId, DateTime now, long pageLoads);

        /// <summary>
        /// Can be periodically called by a scheduler.
        /// </summary>
        /// <returns>Number of errors encountered.</returns>
        int Save(bool throwOnException = false);

        bool HasEvent(DateTime date, uint customerId);
    }
}