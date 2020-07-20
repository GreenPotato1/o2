using System;
using System.Threading.Tasks;
using Com.O2Bionics.Elastic;
using Com.O2Bionics.Utils;
using Com.O2Bionics.ErrorTracker.Properties;
using JetBrains.Annotations;
using log4net;
using log4net.Config;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace Com.O2Bionics.ErrorTracker
{
    public static class LogConfigurator
    {
        private static ILog _log;
        private static IEmergencyWriter _emergencyWriter;

#if DEBUG
        private static bool _initialized;
#endif

        /// <summary>
        /// Run once in main() to register the application kind in <seealso cref="log4net"/>.
        /// Errors will be written to Elastic server.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="applicationKind">The application name had better be an identifier.</param>
        public static void Configure([NotNull] ErrorTrackerSettings settings, [NotNull] string applicationKind)
        {
            //Ideally, the following line must be only here.
            XmlConfigurator.Configure();

            if (string.IsNullOrEmpty(applicationKind))
                throw new ArgumentNullException(nameof(applicationKind));
#if DEBUG
            if (_initialized)
                throw new Exception($"The application '{applicationKind}' might be initialized for a second time.");
            _initialized = true;
#endif
            _log = LogManager.GetLogger(typeof(LogConfigurator));

            var emergencyWriter = EmergencyWriterFactory.CreateAndRegister(settings, applicationKind);
            var client = new EsClient(settings.ElasticConnection);
            var service = new ErrorService(emergencyWriter, settings, client, applicationKind);
            GlobalContainer.RegisterInstance<IErrorService>(service);

            SetupElastic();
            SetupUnhandledException();

            _log.InfoFormat(Resources.StartApplication, applicationKind);
        }

        private static void SetupElastic()
        {
            var appender = new ElasticSearchAppender
                {
                    Threshold = Level.Error,
                    Evaluator = new LevelEvaluator(Level.Error),
                    Name = "ElasticAppender",
                };
            appender.ActivateOptions();

            var repository = (Hierarchy)LogManager.GetRepository();
            repository.Root.AddAppender(appender);

            BasicConfigurator.Configure(repository, appender);
            repository.Configured = true;
        }

        private static void SetupUnhandledException()
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            Subscribe(true);
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Subscribe(false);
            try
            {
                _log.Info(Resources.ExitApplication);
            }
            catch
            {
//Ignore
            }
        }

        private static void Subscribe(bool shallAdd)
        {
            if (shallAdd)
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            else
                AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;

            if (shallAdd)
                TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            else
                TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                if (e.ExceptionObject is Exception exception)
                    HandleException(exception, Resources.AppDomainUnhandledException);
            }
            catch
            {
//Ignore
            }
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                HandleException(e.Exception, Resources.ThreadUnhandledException);
            }
            catch (Exception ex)
            {
                try
                {
                    if (null == _emergencyWriter)
                        _emergencyWriter = GlobalContainer.Resolve<IEmergencyWriter>();

                    var contents = $"TaskScheduler_UnobservedTaskException error: {ex}";
                    _emergencyWriter.Report(contents);
                }
                catch
                {
//Ignore
                }
            }
        }

        private static void HandleException(Exception exception, string message)
        {
            _log?.Fatal(message, exception);
        }
    }
}