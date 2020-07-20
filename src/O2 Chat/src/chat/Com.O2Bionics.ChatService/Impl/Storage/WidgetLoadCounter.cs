using System.Diagnostics;
using System.Threading;

namespace Com.O2Bionics.ChatService.Impl.Storage
{
    public sealed class WidgetLoadCounter
    {
        public long Total;
        public long Increment;
        public long Limit;

        private int m_status;

        public WidgetLoadStatus Status
        {
            [DebuggerStepThrough]
            get
            {
                Thread.MemoryBarrier();
                var value = m_status;
                Thread.MemoryBarrier();
                return (WidgetLoadStatus)value;
            }
            [DebuggerStepThrough]
            set
            {
                Thread.MemoryBarrier();
                m_status = (int)value;
                Thread.MemoryBarrier();
            }
        }

        public bool AboutExceeded()
        {
            var compare = Interlocked.CompareExchange(ref m_status, (int)WidgetLoadStatus.AboutExceeded, (int)WidgetLoadStatus.None);
            var result = compare == (int)WidgetLoadStatus.None;
            return result;
        }

        public override string ToString()
        {
            return $"{Total}, Inc={Increment}, Limit={Limit}, Status={Status}";
        }
    }
}