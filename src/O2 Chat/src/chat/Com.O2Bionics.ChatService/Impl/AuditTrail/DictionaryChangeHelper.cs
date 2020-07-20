using System.Collections.Generic;
using System.Linq;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Impl.AuditTrail.Names;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService.Impl.AuditTrail
{
    public static class DictionaryChangeHelper
    {
        public static void FetchDictionaries(
            [NotNull] this AuditEvent<UserInfo> auditEvent,
            [NotNull] INameResolver nameResolver)
        {
            var departments = new Dictionary<long, string>();

            if (null != auditEvent.OldValue)
                FillUserDepartments(auditEvent.OldValue, nameResolver, departments);
            if (null != auditEvent.NewValue)
                FillUserDepartments(auditEvent.NewValue, nameResolver, departments);

            if (0 == departments.Count)
                return;

            if (auditEvent.ObjectNames == null)
                auditEvent.ObjectNames = new Dictionary<string, Dictionary<string, string>>();

            auditEvent.ObjectNames[EntityNames.Department] = ToStringDictionary(departments);
        }

        private static void FillUserDepartments(
            [NotNull] UserInfo userInfo,
            [NotNull] INameResolver nameResolver,
            [NotNull] Dictionary<long, string> departments)
        {
            var customerId = userInfo.CustomerId;
            if (null != userInfo.AgentDepartments)
                FillDepartments(customerId, userInfo.AgentDepartments, nameResolver, departments);
            if (null != userInfo.SupervisorDepartments)
                FillDepartments(customerId, userInfo.SupervisorDepartments, nameResolver, departments);
        }

        private static void FillDepartments(
            uint customerId,
            [NotNull] IEnumerable<uint> set,
            [NotNull] INameResolver nameResolver,
            [NotNull] Dictionary<long, string> departments)
        {
            foreach (var id in set)
            {
                if (departments.ContainsKey(id))
                    continue;

                var name = nameResolver.GetDepartmentName(customerId, id);
                if (!string.IsNullOrEmpty(name))
                    departments[id] = name;
            }
        }

        private static Dictionary<string, string> ToStringDictionary(Dictionary<long, string> dictionary)
        {
            var result = dictionary.ToDictionary(p => p.Key.ToString(), p => p.Value);
            return result;
        }
    }
}