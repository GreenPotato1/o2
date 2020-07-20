using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using JetBrains.Annotations;
using log4net;

namespace Com.O2Bionics.Utils
{
    public abstract class BackgroundQueueProcessor<T> : IDisposable
    {
        protected readonly ILog Log;

        private readonly int m_addBufferSize;
        private readonly TimeSpan m_addBufferFlushTimeout;
        private readonly Timer m_timer;
        private readonly object m_lock = new object();
        private List<T> m_buffer = new List<T>();
        private int m_queueSize;
        private bool m_isTimerArmed;

        protected BackgroundQueueProcessor(int addBufferSize, TimeSpan addBufferFlushTimeout)
        {
            Log = LogManager.GetLogger(GetType());

            m_addBufferSize = addBufferSize;
            m_addBufferFlushTimeout = addBufferFlushTimeout;

            m_timer = new Timer(_ => TimerCall(), null, Timeout.Infinite, Timeout.Infinite);
        }

        public int QueueSize
        {
            [DebuggerStepThrough]
            get
            {
                lock (m_lock) return m_queueSize;
            }
        }

        public void Dispose()
        {
            Flush(true);
            m_timer.Dispose();
        }

        private void Flush(bool isDisposing)
        {
            List<T> buffer = null;
            lock (m_lock)
            {
                if (m_buffer == null)
                    throw new InvalidOperationException("Queue is disposed already");

                UnarmTimer();

                if (m_buffer.Count > 0)
                    buffer = m_buffer;
                m_buffer = isDisposing ? null : CreateBuffer();
            }

            if (null != buffer && 0 < buffer.Count)
                SaveBuffer(buffer);

            while (QueueSize > 0)
                Thread.Sleep(10);
        }

        public void Flush()
        {
            Flush(false);
        }

        protected abstract void Save([NotNull] List<T> buffer);

        protected void Add([NotNull] T instance)
        {
            lock (m_lock)
            {
                if (m_buffer == null)
                    throw new InvalidOperationException("Queue is disposed already");

                m_buffer.Add(instance);
                m_queueSize++;
                Log.DebugFormat("item added, buffer size={0}, queueSize={1}", m_buffer.Count, m_queueSize);

                if (m_buffer.Count >= m_addBufferSize)
                {
                    Log.DebugFormat("adding a buffer save task");
                    EnqueueBuffer();
                }
                else
                {
                    ArmTimer();
                }
            }
        }

        private void TimerCall()
        {
            lock (m_lock)
            {
                Log.DebugFormat("timer call, bufferSize={0}, queueSize={1}", m_buffer.Count, m_queueSize);

                m_isTimerArmed = false;

                if (m_buffer.Count > 0)
                    EnqueueBuffer();
            }
        }

        private void ArmTimer()
        {
            if (!m_isTimerArmed)
            {
                m_timer.Change(m_addBufferFlushTimeout, Timeout.InfiniteTimeSpan);
                m_isTimerArmed = true;
                Log.DebugFormat("timer is armed for {0}", m_addBufferFlushTimeout);
            }
        }

        private void UnarmTimer()
        {
            if (m_isTimerArmed)
            {
                m_timer.Change(Timeout.Infinite, Timeout.Infinite);
                m_isTimerArmed = false;
            }
        }

        private void EnqueueBuffer()
        {
            var buffer = m_buffer;
            m_buffer = CreateBuffer();
            ThreadPool.QueueUserWorkItem(_ => SaveBuffer(buffer));
        }

        private List<T> CreateBuffer()
        {
            return new List<T>(m_addBufferSize);
        }

        private void SaveBuffer(List<T> buffer)
        {
            Log.DebugFormat("saving buffer of {0} items", buffer.Count);
            var sw = Stopwatch.StartNew();
            try
            {
                Save(buffer);
            }
            catch (Exception e)
            {
                Log.WarnFormat("save failed {0}", e);
            }
            finally
            {
                sw.Stop();
                lock (m_lock) m_queueSize -= buffer.Count;
                Log.InfoFormat("buffer saved in {0}ms.", sw.ElapsedMilliseconds);
            }
        }
    }
}