using System.Collections.Generic;

namespace Com.O2Bionics.ChatService.Contract.AuditTrail
{
    public static class OperationKindGroups
    {
        public static readonly List<string> AuditTrails = new List<string>
            {
                OperationKind.UserChangePasswordKey,
                OperationKind.UserDeleteKey,
                OperationKind.UserInsertKey,
                OperationKind.UserUpdateKey,
                OperationKind.DepartmentDeleteKey,
                OperationKind.DepartmentInsertKey,
                OperationKind.DepartmentUpdateKey,
                OperationKind.CustomerInsertKey,
                OperationKind.CustomerUpdateKey,
                OperationKind.WidgetAppearanceUpdateKey,
                OperationKind.WidgetDailyOverloadKey,
                OperationKind.WidgetUnknownDomainKey,
                OperationKind.WidgetUnknownDomainTooManyKey
            };

        public static readonly List<string> Logins = new List<string> { OperationKind.UserLoginKey };

        static OperationKindGroups()
        {
            AuditTrails.Sort();
        }
    }
}