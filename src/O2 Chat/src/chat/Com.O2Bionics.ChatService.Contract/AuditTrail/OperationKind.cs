namespace Com.O2Bionics.ChatService.Contract.AuditTrail
{
    public static class OperationKind
    {
        public const string UserChangePasswordKey = "UserChangePassword";
        public const string UserChangePasswordValue = "Change user password";

        public const string UserDeleteKey = "UserDelete";
        public const string UserDeleteValue = "Delete user";

        public const string UserInsertKey = "UserInsert";
        public const string UserInsertValue = "Insert user";

        public const string UserUpdateKey = "UserUpdate";
        public const string UserUpdateValue = "Update user";

        public const string DepartmentDeleteKey = "DepartmentDelete";
        public const string DepartmentDeleteValue = "Delete department";

        public const string DepartmentInsertKey = "DepartmentInsert";
        public const string DepartmentInsertValue = "Insert department";

        public const string DepartmentUpdateKey = "DepartmentUpdate";
        public const string DepartmentUpdateValue = "Update department";

        public const string CustomerInsertKey = "CustomerInsert";
        public const string CustomerInsertValue = "Insert customer";

        public const string CustomerUpdateKey = "CustomerUpdate";
        public const string CustomerUpdateValue = "Update customer";

        public const string WidgetAppearanceUpdateKey = "WidgetAppearanceUpdate";
        public const string WidgetAppearanceUpdateValue = "Update widget appearance";

        public const string UserLoginKey = "Login";
        public const string UserLoginValue = "User login";

        public const string WidgetDailyOverloadKey = "WidgetOverload";
        public const string WidgetDailyOverloadValue = "Too many Widget loads";

        public const string WidgetUnknownDomainKey = "WidgetUnknownDomain";
        public const string WidgetUnknownDomainValue = "Unknown domain";

        public const string WidgetUnknownDomainTooManyKey = "WidgetUnknownDomainTooManyEvent";
        public const string WidgetUnknownDomainTooManyValue = "Too many unknown domains";
    }
}