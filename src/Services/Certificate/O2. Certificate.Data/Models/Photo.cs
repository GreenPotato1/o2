using System.ComponentModel.DataAnnotations.Schema;
using O2.Black.Toolkit.Core.Data;

namespace O2.Certificate.Data.Models
{
    public class Photo: BaseDbEntity
    {
        [Column("url")]
        public string Url { get; set; }
        
        [Column("isMain")]
        public bool IsMain { get; set; }
        
        [Column("fileName")]
        public string FileName { get; set; }
    }
}