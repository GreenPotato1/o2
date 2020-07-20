using Com.O2Bionics.Utils;

namespace Com.O2Bionics.AuditTrail.Contract.Names
{
    public static class FieldNames
    {
        public const string AuthorId = "Author.Id";
        public const string AuthorIdKeyword = AuthorId + FieldConstants.KeywordSuffix;

        public const string IdPropertyName = nameof(AuditEvent<object>.Id);
        public const string TimeStampPropertyName = nameof(AuditEvent<object>.Timestamp);

        public const string OperationPropertyName = nameof(AuditEvent<object>.Operation);
        public const string OperationPropertyNameKeyword = OperationPropertyName + FieldConstants.KeywordSuffix;

        public const string StatusPropertyName = nameof(AuditEvent<object>.Status);
        public const string StatusPropertyNameKeyword = StatusPropertyName + FieldConstants.KeywordSuffix;

        public const string AllPropertyName = nameof(AuditEvent<object>.All);
        public const string ChangedPropertyName = nameof(AuditEvent<object>.Changed);
    }
}