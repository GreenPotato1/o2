using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Com.O2Bionics.AuditTrail.Client.Settings;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.AuditTrail.Contract.Names;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.JsonSettings;
using Com.O2Bionics.Utils.Network;
using JetBrains.Annotations;
using Jil;
using log4net;

namespace Com.O2Bionics.AuditTrail.Client
{
    /// <inheritdoc />
    public sealed class AuditTrailClient : IAuditTrailClient
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(AuditTrailClient));
        private readonly INowProvider m_nowProvider;

        private readonly string[] m_uris;
        private readonly HttpClient m_httpClient;

        public AuditTrailClient([NotNull] AuditTrailClientSettings settings, [NotNull] INowProvider nowProvider, [NotNull] string productCode)
        {
            if (null == settings)
                throw new ArgumentNullException(nameof(settings));
            if (null == nowProvider)
                throw new ArgumentNullException(nameof(nowProvider));
            if (string.IsNullOrEmpty(productCode))
                throw new ArgumentNullException(nameof(productCode));
            {
                var error = IdentifierHelper.LowerCase(productCode);
                if (!string.IsNullOrEmpty(error))
                    throw new ArgumentException($"Bad {nameof(productCode)}({productCode}): {error}");
            }

            m_uris = settings.Urls.Select(u => u.AbsoluteUri).ToArray();
            m_nowProvider = nowProvider;

            m_httpClient = new HttpClient();
            m_httpClient.DefaultRequestHeaders.Add(HeaderNames.ProductCode, productCode);
        }

        public void Dispose()
        {
            m_httpClient.Dispose();
        }

        /// <summary>
        ///     Set the time stamp, send to the micro-service.
        /// </summary>
        public async Task Save<T>(AuditEvent<T> auditEvent) where T : class
        {
            if (null == auditEvent)
                throw new ArgumentNullException(nameof(auditEvent));
            if (string.IsNullOrEmpty(auditEvent.Operation))
                throw new ArgumentException($"{nameof(auditEvent)}.{nameof(auditEvent.Operation)} must be not empty string.");
            if (string.IsNullOrEmpty(auditEvent.Status))
                throw new ArgumentException($"{nameof(auditEvent)}.{nameof(auditEvent.Status)} must be not empty string.");

            if (Guid.Empty.Equals(auditEvent.Id))
                auditEvent.Id = Guid.NewGuid();
            auditEvent.Timestamp = m_nowProvider.UtcNow.RemoveMilliseconds();

            var serializedJson = JSON.Serialize(auditEvent, JsonSerializerBuilder.SkipNullJilOptions);
            var formatName = $"{nameof(Save)}.{typeof(T).FullName}";
            if (m_log.IsDebugEnabled)
                m_log.Debug(
                    $"{formatName} '{serializedJson}'.");

            await HttpHelper.PostFirstSuccessfulString(
                m_httpClient,
                m_uris,
                ActionNames.SaveAudit,
                serializedJson,
                (url, exception) => m_log.Error($"{formatName} at '{url}'.", exception),
                "{0} attempts to save the document have failed.");
        }

        public async Task<FacetResponse> SelectFacets(Filter filter)
        {
            var raw = await HttpHelper.PostFirstSuccessfulString(
                m_httpClient,
                m_uris,
                ActionNames.GetFacets,
                filter,
                (url, exception) => m_log.Error($"{ActionNames.GetFacets} at '{url}'.", exception),
                "{0} attempts to select from Audit Trail middle-ware have failed.");
            var result = string.IsNullOrEmpty(raw) ? null : raw.JsonUnstringify2<FacetResponse>();
            return result;
        }
    }
}