using Com.O2Bionics.AuditTrail.Contract;

namespace Com.O2Bionics.AuditTrail.Tests.Utils
{
    public sealed class TestNameResolver : INameResolver
    {
        public string GetDepartmentName(uint customerId, uint id)
        {
            return $"Depart{id}";
        }
    }
}