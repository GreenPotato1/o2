using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using O2.Black.Toolkit.Core.Data;

namespace O2.Certificate.Data.Models.O2C
{
    public class O2CCertificate : BaseDbEntity
    {
        [MaxLength(1)] 
        [Column("serial")] 
        public string Serial { get; set; }

        [Column("short_number")] 
        public int ShortNumber { get; set; }

        [MaxLength(10)] [Column("number")] 
        public string Number { get; set; }

        [Column("date_of_cert")] 
        public long? DateOfCert { get; set; }

        [Column("visible")]
        public bool? Visible { get; set; }
        
        [Column("lock")]
        public bool? Lock { get; set; }
        
        [MaxLength(255)] [Column("firstname")] 
        public string Firstname { get; set; }

        [MaxLength(255)] [Column("lastname")] 
        public string Lastname { get; set; }

        [MaxLength(255)]
        [Column("middlename")]
        public string Middlename { get; set; }

        [DataType(DataType.Text)]
        [Column("education")]
        public string Education { get; set; }

        [Column("contacts")] 
        public virtual List<O2CContact> Contacts { get; set; } = new List<O2CContact>();

        [Column("photos")] 
        public virtual List<O2CPhoto> Photos { get; set; } = new List<O2CPhoto>();
        
        [Column("locations")] 
        public virtual List<O2CCertificateLocation> Locations { get; set; } = new List<O2CCertificateLocation>();


    }
}