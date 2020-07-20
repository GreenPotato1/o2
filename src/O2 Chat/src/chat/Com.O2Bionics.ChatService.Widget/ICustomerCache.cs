using System;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService.Widget
{
    public interface ICustomerCache : IDailyCache
    {
        [Pure]
        bool TryGet(DateTime date, uint skey, [CanBeNull] out CustomerEntry entry);

        [Pure]
        bool IsActive(uint skey);

        /// <summary>
        /// Unlike <seealso cref="SetSoft"/>, manages the active customer list.
        /// </summary>
        bool Set(DateTime date, uint skey, [NotNull] CustomerEntry entry);

        /// <summary>
        /// Manages daily entries.
        /// The <paramref name="action"/> must be fast.
        /// </summary>
        bool SetSoft(DateTime date, uint skey, [NotNull] Action<CustomerEntry> action);

        void Clear();
    }
}