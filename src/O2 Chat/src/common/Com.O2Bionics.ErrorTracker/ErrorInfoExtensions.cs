using System;
using System.Net;
using Com.O2Bionics.Utils.Network;
using JetBrains.Annotations;
using log4net.Core;
using log4net.Util;

namespace Com.O2Bionics.ErrorTracker
{
    public static class ErrorInfoExtensions
    {
        public static ErrorInfo ToErrorInfo(this LoggingEvent loggingEvent)
        {
            var result = new ErrorInfo
                {
                    Timestamp = loggingEvent.TimeStamp.ToUniversalTime(),
                    Message = loggingEvent.RenderedMessage,
                    LoggerName = loggingEvent.LoggerName,
                    HostName = Dns.GetHostName()
                };

            var properties = loggingEvent.Properties;
            result.CustomerId = GetUint(properties, ServiceConstants.CustomerId);
            result.UserId = GetUint(properties, ServiceConstants.UserId);
            result.VisitorId = GetULong(properties, ServiceConstants.VisitorId);

            result.ClientIp = GetString(properties, ServiceConstants.ClientIp);
            result.Url = GetString(properties, ServiceConstants.Url);
            result.UserAgent = GetString(properties, ServiceConstants.UserAgent);

            var exception = GetException(loggingEvent);
            FillExceptionFields(result, exception);

            return result;
        }

        private static ulong GetULong(PropertiesDictionary properties, string key)
        {
            if (properties[key] is string str
                && ulong.TryParse(str, out var result) && 0 < result)
                return result;
            return 0;
        }

        private static uint GetUint(PropertiesDictionary properties, string key)
        {
            if (properties[key] is string str
                && uint.TryParse(str, out var result) && 0 < result)
                return result;
            return 0;
        }

        [CanBeNull]
        private static string GetString(PropertiesDictionary properties, string key)
        {
            if (properties[key] is string str && 0 < str.Length)
                return str;
            return null;
        }

        public static void FillExceptionFields(this ErrorInfo errorInfo, [CanBeNull] Exception exception)
        {
            if (null == exception)
                return;
            errorInfo.ExceptionMessage = exception.Message;
            errorInfo.ExceptionSource = exception.Source;
            errorInfo.ExceptionStack = exception.ToString();
            errorInfo.ExceptionType = exception.GetType().FullName;
        }

        public static void FillIdentifiers(this ErrorInfo errorInfo, [NotNull] IIdentifierReader identifierReader)
        {
            var (customer, user, visitor) = identifierReader.Read();
            if (0 < customer)
                errorInfo.CustomerId = customer;
            if (0 < user)
                errorInfo.UserId = user;
            if (0 < visitor)
                errorInfo.VisitorId = visitor;
        }

        private static Exception GetException(LoggingEvent loggingEvent)
        {
            if (null != loggingEvent.ExceptionObject)
                return loggingEvent.ExceptionObject;

            // TODO: Check? Special handling of the MessageObject since it may be an exception.
            return loggingEvent.MessageObject as Exception;
        }
    }
}