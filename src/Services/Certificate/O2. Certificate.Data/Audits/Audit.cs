using System;
using System.ComponentModel.DataAnnotations;

namespace O2.Business.Data.Audits
{
    public class Audit
    {
        [Key]
        public decimal Id { get; set; }

        public string TableName { get; set; }

        public string PropertyName { get; set; }

        public string ChangeType { get; set; }

        public Guid EntityId { get; set; }
   
        public string OriginalValue { get; set; }

        public string NewValue { get; set; }

        public DateTime IsAuditedOn { get; set; }

        public string ModifiedBy { get; set; }
    }
}