using System;
using System.Diagnostics;
using System.Threading;

namespace Com.O2Bionics.Utils
{
    public sealed class LockFreeDateTime
    {
        private long m_ticks;

        /// <summary>
        /// UTC date time.
        /// </summary>
        public DateTime Value
        {
            [DebuggerStepThrough]
            get
            {
                var value = Interlocked.Read(ref m_ticks);
                var result = new DateTime(value, DateTimeKind.Utc);
                return result;
            }
            [DebuggerStepThrough]
            set
            {
                var t = value.Ticks;
                Interlocked.Exchange(ref m_ticks, t);
            }
        }

        public LockFreeDateTime(DateTime dateTime = new DateTime())
        {
            m_ticks = dateTime.Ticks;
        }

        public bool Set(DateTime oldDateTime, DateTime newDateTime)
        {
            var compare = Interlocked.CompareExchange(ref m_ticks, newDateTime.Ticks, oldDateTime.Ticks);
            var result = compare == oldDateTime.Ticks;
            return result;
        }

        public override string ToString()
        {
            var value = Value;
            return $"{value}";
        }
    }
}