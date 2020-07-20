namespace Com.O2Bionics.ChatService.Contract.AuditTrail
{
    public static class OperationStatus
    {
        public const string AccessDeniedKey = "AccessDenied";
        public const string AccessDeniedValue = "Access denied";

        /// <summary>
        ///     E.g. invalid email, required field is null, too long pair, such
        ///     user doesn't exist.
        /// </summary>
        public const string ValidationFailedKey = "ValidationFailed";

        public const string ValidationFailedValue = "Validation failed";

        /// <summary>
        ///     Unexpected error e.g. Oracle throws an exception.
        /// </summary>
        public const string OperationFailedKey = "OperationFailed";

        public const string OperationFailedValue = "Operation failed";

        public const string SuccessKey = "Success";
        public const string SuccessValue = "Success";


        #region AccountLookupStatus

        public const string NotFoundKey = "NotFound";
        public const string NotFoundValue = "Account not found";

        public const string NotActiveKey = "NotActive";
        public const string NotActiveValue = "Account not active";

        public const string CustomerNotActiveKey = "CustomerNotActive";
        public const string CustomerNotActiveValue = "Customer not active";

        /// <summary>
        /// The user account has been locked because the last allowed user's login attempt has failed.
        /// </summary>
        public const string LoginFailedKey = "LoginFailed";

        public const string LoginFailedValue = "Login Failed";

        #endregion
    }
}