using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using log4net;

namespace Com.O2Bionics.Utils
{
    public interface IDailyCache
    {
        Task Load();
    }

    public abstract class DailyCache<T> : IDailyCache
    {
        [NotNull] protected readonly ILog Log;
        [NotNull] private DailyInfo<T> m_dailyInfo = new DailyInfo<T>(0);

        protected DailyCache()
        {
            Log = LogManager.GetLogger(GetType());
        }

        public abstract Task Load();

        public void Clear()
        {
            Set(new DailyInfo<T>(0));
            if (Log.IsDebugEnabled)
                Log.Debug(nameof(Clear));
        }

        public override string ToString()
        {
            var dailyInfo = GetDailyInfo();
            return $"{dailyInfo}";
        }

        [NotNull]
        protected DailyInfo<T> GetDailyInfo()
        {
            Thread.MemoryBarrier();
            var result = m_dailyInfo;
            Thread.MemoryBarrier();
            return result;
        }

        protected void Set([NotNull] DailyInfo<T> dailyInfo)
        {
            if (null == dailyInfo)
                throw new ArgumentNullException(nameof(dailyInfo));

            Thread.MemoryBarrier();
            m_dailyInfo = dailyInfo;
            Thread.MemoryBarrier();
        }

        protected bool CompareSet([NotNull] DailyInfo<T> oldDailyInfo, [NotNull] DailyInfo<T> newDailyInfo)
        {
            if (null == oldDailyInfo)
                throw new ArgumentNullException(nameof(oldDailyInfo));
            if (null == newDailyInfo)
                throw new ArgumentNullException(nameof(newDailyInfo));
            Debug.Assert(oldDailyInfo != newDailyInfo);

            var compare = Interlocked.CompareExchange(ref m_dailyInfo, newDailyInfo, oldDailyInfo);
            var result = compare == oldDailyInfo;
            return result;
        }

#if DEBUG
        protected bool IsReady;
        protected void CheckReady()
        {
            if (!IsReady)
                throw new Exception($"The {nameof(Load)} method must have been called.");
        }
#endif
    }
}