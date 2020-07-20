using JetBrains.Annotations;

namespace Com.O2Bionics.AuditTrail.Contract
{
    public interface INameResolver
    {
        [CanBeNull]
        string GetDepartmentName(uint customerId, uint id);
    }
}