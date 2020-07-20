using System;
using System.Net;
using Com.O2Bionics.Utils.JsonSettings;
using Com.O2Bionics.Utils;
using log4net.Appender;
using log4net.Core;

namespace Com.O2Bionics.ErrorTracker
{
    /// <summary>
    /// Write log messages from log4net to the Elastic Search.
    /// </summary>
    /// <inheritdoc />
    public sealed class ElasticSearchAppender : BufferingAppenderSkeleton
    {
        private IErrorService m_errorService;
        private IEmergencyWriter m_emergencyWriter;

        public ElasticSearchAppender()
        {
            Build();
        }

        public string ConnectionString { get; set; }

        public override void ActivateOptions()
        {
            base.ActivateOptions();
            ServicePointManager.Expect100Continue = false;
            if (string.IsNullOrEmpty(ConnectionString))
            {
                ErrorHandler.Error($"The connection string must be set in {typeof(ElasticSearchAppender).Name}.ActivateOptions, name=[{Name}].");
            }
        }

        protected override void SendBuffer(LoggingEvent[] events)
        {
            try
            {
                var errorInfos = new ErrorInfo[events.Length];
                for (int i = 0; i < events.Length; i++)
                    errorInfos[i] = events[i].ToErrorInfo();

                m_errorService.Save(errorInfos);
            }
            catch (Exception e)
            {
                try
                {
                    if (null == m_emergencyWriter)
                        m_emergencyWriter = GlobalContainer.Resolve<IEmergencyWriter>();

                    var contents = $"ElasticSearchAppender error: {e}";
                    m_emergencyWriter.Report(contents);
                }
                catch
                {
                    //Ignore
                }
            }
        }

        private void Build()
        {
            var settings = new JsonSettingsReader().ReadFromFile<ErrorTrackerSettings>();
            m_errorService = GlobalContainer.Resolve<IErrorService>();
            ConnectionString = $"ES=[{settings.ElasticConnection.Uris.JoinAsString()}];Index={settings.Index.Name};";
        }
    }
}