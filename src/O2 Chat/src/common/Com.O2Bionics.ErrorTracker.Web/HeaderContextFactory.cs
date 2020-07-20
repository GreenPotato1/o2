using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Web;
using Com.O2Bionics.Utils.Network;
using JetBrains.Annotations;
using pair = System.Collections.Generic.KeyValuePair<string, string>;

namespace Com.O2Bionics.ErrorTracker.Web
{
    public sealed class HeaderContextFactory : IHeaderContextFactory
    {
        [NotNull] private readonly IIdentifierReader m_identifierReader;

        public HeaderContextFactory([NotNull] IIdentifierReader identifierReader)
        {
            m_identifierReader = identifierReader ?? throw new ArgumentNullException(nameof(identifierReader));
        }

        public IDisposable Create(IClientChannel channel)
        {
            var (customerId, userId, visitorId) = m_identifierReader.Read();
            var contextValues = ContextValues();
            if (null == contextValues && customerId <= 0)
                return null;

            var result = new OperationContextScope(channel);

            if (0 < customerId)
                AddHeader(ServiceConstants.CustomerId, customerId);
            if (0 < userId)
                AddHeader(ServiceConstants.UserId, userId);
            if (0 < visitorId)
                AddHeader(ServiceConstants.VisitorId, visitorId);

            if (null != contextValues)
                for (int i = 0; i < contextValues.Count; i++)
                    AddHeader(contextValues[i].Key, contextValues[i].Value);

            return result;
        }

        private static void AddHeader<T>(string name, T value)
        {
            OperationContext.Current.OutgoingMessageHeaders.Add(
                MessageHeader.CreateHeader(name, ServiceConstants.NamespaceShort, value));
        }

        private static List<pair> ContextValues()
        {
            var context = HttpContext.Current;
            if (null == context)
                return null;

            var list = new List<pair>();

            AddIfNotEmpty(list, ServiceConstants.ClientIp, context.ClientIp());
            AddIfNotEmpty(list, ServiceConstants.Url, context.Request.Url.AbsoluteUri);
            AddIfNotEmpty(list, ServiceConstants.UserAgent, context.Request.UserAgent);

            return 0 == list.Count ? null : list;
        }

        private static void AddIfNotEmpty(List<pair> list, string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                list.Add(new pair(key, value));
        }
    }
}