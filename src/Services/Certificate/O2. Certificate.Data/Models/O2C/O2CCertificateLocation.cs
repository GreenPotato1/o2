using System;
using System.ComponentModel.DataAnnotations;

namespace O2.Certificate.Data.Models.O2C
{
    public class O2CCertificateLocation
    {
        [Key]
        public Guid O2CCertificateId { get; set; }
        public O2CCertificate O2CCertificate { get; set; }
        [Key]
        public Guid O2CLocationId { get; set; }
        public O2CLocation O2CLocation { get; set; }
    }
}