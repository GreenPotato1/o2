using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Com.O2Bionics.AuditTrail.Client;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.ChatService.Contract.Widget;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Properties;
using JetBrains.Annotations;
using Jil;
using log4net;

namespace Com.O2Bionics.ChatService.Impl.Storage
{
    /// <summary>
    /// Load the data from Elastic <seealso cref="IAuditTrailClient"/>.
    /// </summary>
    public sealed class UnknownDomainLoader : IUnknownDomainLoader
    {
        private int m_chunkSize = 1024;
        private static readonly ILog m_log = LogManager.GetLogger(typeof(UnknownDomainLoader));

        internal void SetChunkSize(int value)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(string.Format(Resources.ArgumentMustBePositive2, nameof(SetChunkSize), value));
            m_chunkSize = value;
        }

        public async Task<Dictionary<uint, HashSet<string>>> Load(IAuditTrailClient auditTrailClient, DateTime date, int maximumUnknownDomains)
        {
            if (null == auditTrailClient)
                throw new ArgumentNullException(nameof(auditTrailClient));
            if (maximumUnknownDomains <= 0)
                throw new ArgumentOutOfRangeException(
                    string.Format(Resources.ArgumentMustBePositive2, nameof(maximumUnknownDomains), maximumUnknownDomains));

            date = date.RemoveTime();
            if (m_log.IsDebugEnabled)
                m_log.Debug($"Start loading unknown domains for {date}.");

            var result = new Dictionary<uint, HashSet<string>>();
            var filter = CreateFilter(date);
            for (;;)
            {
                if (!await RunStep(auditTrailClient, maximumUnknownDomains, filter, result))
                    break;
            }

            return 0 < result.Count ? result : null;
        }

        public async Task<Dictionary<uint, HashSet<string>>> LoadMany(
            IAuditTrailClient auditTrailClient,
            DateTime date,
            int maximumUnknownDomains,
            uint[] customerIds)
        {
            if (null == auditTrailClient)
                throw new ArgumentNullException(nameof(auditTrailClient));
            if (maximumUnknownDomains <= 0)
                throw new ArgumentOutOfRangeException(
                    string.Format(Resources.ArgumentMustBePositive2, nameof(maximumUnknownDomains), maximumUnknownDomains));
            if (null == customerIds || 0 == customerIds.Length)
                throw new ArgumentNullException(nameof(customerIds));

            date = date.RemoveTime();
            if (m_log.IsDebugEnabled)
                m_log.Debug($"Start loading unknown domains for {date} for {customerIds.Length} customerIds.");

            var result = new Dictionary<uint, HashSet<string>>();
            var filter = CreateFilter(date);
            foreach (var customerId in customerIds)
            {
                filter.CustomerId = customerId.ToString();
                for (;;)
                {
                    if (!await RunStep(auditTrailClient, maximumUnknownDomains, filter, result))
                        break;
                }
            }

            return 0 < result.Count ? result : null;
        }

        [NotNull]
        private Filter CreateFilter(DateTime date)
        {
            var filter = new Filter(ProductCodes.Chat, m_chunkSize)
                {
                    FromTime = date,
                    ToTime = date.AddDays(1),
                    Operations = new List<string> { OperationKind.WidgetUnknownDomainKey },
                };
            return filter;
        }

        private static async Task<bool> RunStep(
            IAuditTrailClient auditTrailClient,
            int maximumUnknownDomains,
            Filter filter,
            Dictionary<uint, HashSet<string>> result)
        {
            var facets = await auditTrailClient.SelectFacets(filter);
            var rawDocuments = facets?.RawDocuments;
            if (null == rawDocuments || 0 == rawDocuments.Count)
                return false;

            AuditEvent<WidgetUnknownDomain> document = null;
            string raw = null;
            try
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < rawDocuments.Count; i++)
                {
                    raw = rawDocuments[i];
                    document = JSON.Deserialize<AuditEvent<WidgetUnknownDomain>>(raw, JsonSerializerBuilder.DefaultJilOptions);
                    if (!uint.TryParse(document.CustomerId, out var customerId))
                        throw new Exception($"The CustomerId({document.CustomerId}) must be uint.");

                    if (!result.TryGetValue(customerId, out var names))
                        result[customerId] = names = new HashSet<string>();

                    if (names.Count < maximumUnknownDomains)
                        names.Add(document.NewValue.Name);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error parsing event '{raw}'.", e);
            }

            Debug.Assert(null != document);
            // TODO: actually should take hits[last].sort property here
            filter.SearchPosition = new SearchPositionInfo(
                new[]
                    {
                        document.Timestamp.ToUnixTimeMilliseconds().ToString("#"),
                        document.Id.ToString()
                    });

            return true;
        }
    }
}