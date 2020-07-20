using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using JetBrains.Annotations;
using Com.O2Bionics.Utils.Network;
using log4net;
using pair = System.Collections.Generic.KeyValuePair<string, string>;

namespace Com.O2Bionics.ErrorTracker
{
    public static class WcfHelper
    {
        /// <summary>
        /// Remember to dispose the returned value!
        /// Puts the customer/user/visitor etc. to the log4net context.
        /// To be used by the WCF clients.
        /// </summary>
        [CanBeNull]
        public static IDisposable CreateContext()
        {
            var headers = ReadHeaders();
            if (null == headers)
                return null;

            var result = CreateContext(headers);
            return result;
        }

        [CanBeNull]
        internal static IDisposable CreateContext([CanBeNull] List<pair> pairs)
        {
            if (null == pairs || 0 == pairs.Count)
                return null;

            var array = new IDisposable[pairs.Count];
            for (int i = 0; i < pairs.Count; ++i)
                array[i] = CreateContextImpl(pairs[i].Key, pairs[i].Value);

            var result = new DisposableContainer(array);
            return result;
        }

        private static List<pair> ReadHeaders()
        {
            var headers = OperationContext.Current?.IncomingMessageHeaders;
            if (null == headers)
                return null;

            var result = new List<pair>();
            TryAddLong(headers, result, ServiceConstants.CustomerId);
            TryAddLong(headers, result, ServiceConstants.UserId);
            TryAddLong(headers, result, ServiceConstants.VisitorId);

            TryAddString(headers, result, ServiceConstants.ClientIp);
            TryAddString(headers, result, ServiceConstants.Url);
            TryAddString(headers, result, ServiceConstants.UserAgent);

            return 0 == result.Count ? null : result;
        }

        private static void TryAddLong(MessageHeaders headers, List<pair> list, string key)
        {
            var value = GetLong(headers, key);
            if (0 < value)
                list.Add(new pair(key, value.ToString()));
        }

        private static void TryAddString(MessageHeaders headers, List<pair> list, string key)
        {
            var value = GetString(headers, key);
            if (!string.IsNullOrEmpty(value))
                list.Add(new pair(key, value));
        }

        [CanBeNull]
        private static string GetString(MessageHeaders headers, string name)
        {
            var id = headers.FindHeader(name, ServiceConstants.NamespaceShort);
            if (id < 0)
                return null;

            var reader = headers.GetReaderAtHeader(id);
            var result = reader.ReadElementContentAsString();
            return string.IsNullOrEmpty(result) ? null : result;
        }

        private static long GetLong(MessageHeaders headers, string name)
        {
            var str = GetString(headers, name);
            if (null != str && long.TryParse(str, out var result) && 0 < result)
                return result;
            return 0;
        }

        private static IDisposable CreateContextImpl(string name, string value)
        {
            var result = LogicalThreadContext.Stacks[name].Push(value);
            return result;
        }
    }
}