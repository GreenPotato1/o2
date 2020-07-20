using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Com.O2Bionics.AuditTrail.Client;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService
{
    public interface IUnknownDomainLoader
    {
        [ItemCanBeNull]
        Task<Dictionary<uint, HashSet<string>>> Load([NotNull] IAuditTrailClient auditTrailClient, DateTime date, int maximumUnknownDomains);

        [ItemCanBeNull]
        Task<Dictionary<uint, HashSet<string>>> LoadMany(
            [NotNull] IAuditTrailClient auditTrailClient,
            DateTime date,
            int maximumUnknownDomains,
            [NotNull] uint[] customerIds);
    }
}