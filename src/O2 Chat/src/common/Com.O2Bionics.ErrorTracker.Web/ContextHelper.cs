using System;
using System.Web;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;
using log4net;
using Microsoft.Owin;

namespace Com.O2Bionics.ErrorTracker.Web
{
    public static class ContextHelper
    {
        private static ILog _log;
        private static ILog GetLog() => _log ?? (_log = LogManager.GetLogger(typeof(ContextHelper)));

        public static void FillFromContext([NotNull] this ErrorInfo errorInfo, [NotNull] HttpContext context)
        {
            errorInfo.ClientIp = ClientIp(context);
            if (string.IsNullOrEmpty(errorInfo.Url))
                errorInfo.Url = context.Request.Url.AbsoluteUri;
            errorInfo.UserAgent = context.Request.UserAgent;
            errorInfo.Timestamp = context.Timestamp.ToUniversalTime();
        }

        public static void FillFromContext([NotNull] this ErrorInfo errorInfo, [NotNull] IOwinContext context)
        {
            errorInfo.ClientIp = context.Request.RemoteIpAddress;
            errorInfo.Url = context.Request.Uri.AbsoluteUri;
            const string userAgent = "User-Agent";
            errorInfo.UserAgent = context.Request.Headers[userAgent];

            try
            {
                var form = context.Request.ReadFormAsync().WaitAndUnwrapException();
                FillUint(form, val => errorInfo.CustomerId = val, "cid");
                FillULong(form, val => errorInfo.VisitorId = val, "vid");
            }
            catch (Exception e)
            {
                try
                {
                    var log = GetLog();
                    log.Error(nameof(ContextHelper), e);
                }
                catch
                {
//Ignore
                }
            }
        }

        public static string ClientIp([NotNull] this HttpContext context)
        {
            var result = string.IsNullOrEmpty(context.Request.UserHostName)
                ? context.Request.UserHostAddress
                : context.Request.UserHostName;
            return result;
        }

        private static void FillULong(IFormCollection form, Action<ulong> setValue, params string[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                var value = form[keys[i]];
                if (null != value && ulong.TryParse(value, out var temp) && 0 < temp)
                {
                    setValue(temp);
                    return;
                }
            }
        }

        private static void FillUint(IFormCollection form, Action<uint> setValue, params string[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                var value = form[keys[i]];
                if (null != value && uint.TryParse(value, out var temp) && 0 < temp)
                {
                    setValue(temp);
                    return;
                }
            }
        }
    }
}