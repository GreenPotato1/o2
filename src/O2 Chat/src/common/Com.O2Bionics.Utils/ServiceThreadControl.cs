using System;
using System.Diagnostics;
using System.Threading;
using log4net;
using Topshelf;

namespace Com.O2Bionics.Utils
{
    public class ServiceThreadControl
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(ServiceThreadControl));
    }

    public class ServiceThreadControl<TService> : ServiceThreadControl, ServiceControl
        where TService : IDisposable
    {
        private Thread m_hostThread;
        private Exception m_hostThreadException;
        private readonly ManualResetEvent m_hostThreadStartCompletedEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent m_hostThreadStopEvent = new ManualResetEvent(false);

        private readonly Func<ILog, TService> m_createService;
        private readonly Action<TService> m_startService;
        private readonly Action<TService> m_stopService;

        public ServiceThreadControl(
            Func<ILog, TService> createService,
            Action<TService> startService = null,
            Action<TService> stopService = null)
        {
            m_createService = createService ?? throw new ArgumentNullException(nameof(createService));
            m_startService = startService;
            m_stopService = stopService;
        }

        public bool Start(HostControl hostControl)
        {
            Log.Info("Starting host thread");

            m_hostThreadException = null;
            m_hostThread = new Thread(HostThreadMain) { IsBackground = true, Name = "host" };
            m_hostThread.Start();
            m_hostThreadStartCompletedEvent.WaitOne();
            return m_hostThreadException == null;
        }

        public bool Stop(HostControl hostControl)
        {
            if (m_hostThread != null && m_hostThread.IsAlive)
            {
                Log.Info("Stopping the host thread");
                m_hostThreadStopEvent.Set();
                m_hostThread.Join();
                Log.Info("The host thread stopped");
            }
            else
            {
                Log.Warn("The host thread is already completed or not started.");
            }

            return true;
        }

        private void HostThreadMain()
        {
            Log.Info("Host thread started, initializing service");

            var stopwatch = Stopwatch.StartNew();

            TService service;
            try
            {
                service = m_createService(Log);
                m_startService?.Invoke(service);
            }
            catch (Exception e)
            {
                m_hostThreadException = e;
                Log.Error("Can't start the service", e);
                return;
            }
            finally
            {
                m_hostThreadStartCompletedEvent.Set();
            }

            Safe.Do(
                () =>
                    {
                        stopwatch.Stop();
                        Log.Info($"Service started in {stopwatch.Elapsed.TotalSeconds} seconds.");
                    });

            m_hostThreadStopEvent.WaitOne();
            Log.Info("Stop service event received.");

            if (m_stopService != null)
                Safe.Do(
                    // ReSharper disable once AccessToDisposedClosure
                    () => m_stopService(service),
                    e => Log.Error("Error stopping the service.", e));
            Safe.Do(
                () => service.Dispose(),
                e => Log.Error("Error disposing the service.", e));
            Log.Info("Service stopped");
        }
    }
}