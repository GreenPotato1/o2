using Com.O2Bionics.AuditTrail.Contract;
using JetBrains.Annotations;

namespace Com.O2Bionics.AuditTrail.Tests.Utils
{
    public static class AuditEventHelper
    {
        public static void ClearAnalyzedFields<T>([NotNull] this AuditEvent<T> auditEvent)
        {
            auditEvent.All = auditEvent.Changed = null;
        }
    }
}