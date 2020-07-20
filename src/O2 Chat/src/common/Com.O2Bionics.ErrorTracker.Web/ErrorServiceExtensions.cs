using System;
using System.Net;
using JetBrains.Annotations;
using log4net;

namespace Com.O2Bionics.ErrorTracker.Web
{
    public static class ErrorServiceExtensions
    {
        public static void LogExceptionSafe(
            this IErrorService errorService,
            IIdentifierReader identifierReader,
            ILog log,
            [NotNull] Exception exception,
            [NotNull] string loggerName,
            [NotNull] Action<ErrorInfo> fillFromContext)
        {
            try
            {
                var errorInfo = new ErrorInfo
                    {
                        Message = loggerName,
                        LoggerName = loggerName,
                        HostName = Dns.GetHostName()
                    };
                errorInfo.FillExceptionFields(exception);
                errorInfo.FillIdentifiers(identifierReader);
                fillFromContext(errorInfo);

                errorService.Save(errorInfo);
            }
            catch (Exception e2)
            {
                try
                {
                    log.Error(nameof(LogExceptionSafe) + "." + loggerName, e2);
                }
                catch
                {
//Ignore
                }
            }
        }
    }
}