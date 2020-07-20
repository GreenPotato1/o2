using System;
using System.Threading.Tasks;
using Com.O2Bionics.AuditTrail.Contract;
using JetBrains.Annotations;

namespace Com.O2Bionics.AuditTrail.Client
{
    /// <summary>
    ///     Proxy to the HTTPS micro-service "AuditTrailServiceMiddleware".
    /// </summary>
    public interface IAuditTrailClient : IDisposable
    {
        /// <summary>
        ///     Save the <paramref name="auditEvent" /> to the server.
        /// </summary>
        Task Save<T>([NotNull] AuditEvent<T> auditEvent) where T : class;

        Task<FacetResponse> SelectFacets([NotNull] Filter filter);
    }
}