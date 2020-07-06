using System;
using System.ComponentModel.DataAnnotations.Schema;
using O2.Black.Toolkit.Core.Data;

namespace O2.Certificate.Data.Models.O2Ev
{
    public class O2EvMeta : BaseDbEntity
    {
        public O2EvMeta():base()
        {
            
        }
        [Column("country")]
        public string LocationCountry { get; set; }

        [Column("region")]
        public string LocationRegion { get; set; }

        [Column("event_id")]
        public Guid EventId { get; set; }
        
        [Column("event")]
        public O2EvEvent O2EvEvent { get; set; }
    }
}