using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Com.O2Bionics.Elastic;
using Com.O2Bionics.ErrorTracker.Properties;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;
using Jil;

namespace Com.O2Bionics.ErrorTracker
{
    [DebuggerDisplay("{m_applicationName} Index={m_indexName}, Queue={QueueSize}")]
    public sealed class ErrorService : IErrorService
    {
        private readonly IEmergencyWriter m_emergencyWriter;
        private readonly string m_indexName;
        private readonly int m_maxBufferSize;
        private readonly int m_closeTimeoutMilliseconds;
        private readonly string m_applicationName;
        private long m_queueSize, m_alive = 1;

        internal IEsClient Client { get; }

        internal long QueueSize => Interlocked.Read(ref m_queueSize);

        public ErrorService(
            [NotNull] IEmergencyWriter emergencyWriter,
            ErrorTrackerSettings settings,
            [NotNull] IEsClient client,
            string applicationName,
            int maxBufferSize = 1 << 17,
            int closeTimeoutMilliseconds = 30 * 1000)
        {
            m_emergencyWriter = emergencyWriter ?? throw new ArgumentNullException(nameof(emergencyWriter));
            Client = client ?? throw new ArgumentNullException(nameof(client));

            var indexName = settings.Index.Name;
            if (string.IsNullOrEmpty(indexName))
                throw new ArgumentNullException(nameof(indexName));
            if (string.IsNullOrEmpty(applicationName))
                throw new ArgumentNullException(nameof(applicationName));
            if (maxBufferSize <= 0)
                throw new ArgumentException(string.Format(Utils.Properties.Resources.ArgumentMustBePositive2, nameof(maxBufferSize), maxBufferSize));
            if (closeTimeoutMilliseconds <= 0)
                throw new ArgumentException(
                    string.Format(Utils.Properties.Resources.ArgumentMustBePositive2, nameof(closeTimeoutMilliseconds), closeTimeoutMilliseconds));

            m_indexName = indexName;
            m_maxBufferSize = maxBufferSize;
            m_closeTimeoutMilliseconds = closeTimeoutMilliseconds;
            m_applicationName = applicationName;
        }

        ~ErrorService()
        {
            Stop();
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }

        public void Save(params ErrorInfo[] errorInfos)
        {
            if (null == errorInfos || 0 == errorInfos.Length || 0 == Interlocked.Read(ref m_alive))
                return;

            var incremented = Interlocked.Increment(ref m_queueSize);
            if (m_maxBufferSize < incremented)
            {
                Interlocked.Decrement(ref m_queueSize);
                return;
            }

            try
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (int i = 0; i < errorInfos.Length; i++)
                    errorInfos[i].Application = m_applicationName;
                //TODO: p1. task-125. wait for task completion.
                Task.Factory.StartNew(SaveImpl, errorInfos);
            }
            catch (Exception e)
            {
                Interlocked.Decrement(ref m_queueSize);
                try
                {
                    var contents = $"ErrorService.Save error: {e}";
                    m_emergencyWriter.Report(contents);
                }
                catch
                {
//Ignore
                }
            }
        }

        private void SaveImpl(object o)
        {
            ErrorInfo[] errorInfos = null;
            try
            {
                errorInfos = (ErrorInfo[])o;
                Client.IndexMany(m_indexName, errorInfos);
            }
            catch (Exception e)
            {
                try
                {
                    var data = null != errorInfos && 0 < errorInfos.Length
                        ? JSON.Serialize(errorInfos, JsonSerializerBuilder.SkipNullJilOptions)
                        : null;
                    var contents = string.Format(Resources.ErrorSavingErrorInfos3, errorInfos?.Length ?? 0, data, e);
                    m_emergencyWriter.Report(contents);
                }
                catch
                {
                    try
                    {
                        var contents = string.Format(Resources.ErrorSavingErrorInfos3, errorInfos?.Length ?? 0, null, e);
                        m_emergencyWriter.Report(contents);
                    }
                    catch
                    {
                        //Ignore
                    }
                }
            }
            finally
            {
                Interlocked.Decrement(ref m_queueSize);
            }
        }

        private void Stop()
        {
            var exchange = Interlocked.CompareExchange(ref m_alive, 0, 1);
            if (0 == exchange)
                return;
            try
            {
                if (0 == QueueSize)
                    return;
                var stopwatch = Stopwatch.StartNew();
                for (;;)
                {
                    if (0 == QueueSize || m_closeTimeoutMilliseconds < stopwatch.ElapsedMilliseconds)
                        break;
                    Thread.Sleep(1);
                }
            }
            catch (Exception e)
            {
                try
                {
                    Console.WriteLine(e);
                }
                catch
                {
                    //Ignore
                }
            }
        }
    }
}