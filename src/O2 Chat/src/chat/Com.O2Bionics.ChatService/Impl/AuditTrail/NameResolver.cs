using System;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.ChatService.DataModel;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService.Impl.AuditTrail
{
    public sealed class NameResolver : INameResolver
    {
        private readonly IDepartmentStorage m_departmentStorage;
        private readonly ChatDatabase m_db;

        public NameResolver([NotNull] IDepartmentStorage departmentStorage, [NotNull] ChatDatabase db)
        {
            m_departmentStorage = departmentStorage ?? throw new ArgumentNullException(nameof(departmentStorage));
            m_db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public string GetDepartmentName(uint customerId, uint id)
        {
            var department = m_departmentStorage.Get(m_db, customerId, id);
            var result = department?.Name;
            return result;
        }
    }
}