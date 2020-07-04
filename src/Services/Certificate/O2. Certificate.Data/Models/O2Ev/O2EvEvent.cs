using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using O2.Black.Toolkit.Core.Data;

namespace O2.Business.Data.Models.O2Ev
{
    public class O2EvEvent : BaseDbEntity
    {
        public O2EvEvent():base()
        {
            Meta = new O2EvMeta();
        }
        
        [Column("title")]
        public string Title { get; set; }

        [Column("short_description")]
        public string ShortDescription { get; set; }

        [Column("start_date")]
        public long StartDate { get; set; }

        [Column("end_date")]
        public long EndDate { get; set; }

        [Column("all_day")]
        public bool AllDay { get; set; }

        [Column("meta")]
        public O2EvMeta Meta { get; set; }
        
        [Column("photos")]
        public virtual ICollection<O2EvPhoto> Photos { get; set; } = new List<O2EvPhoto>();

        // public override void UpdateEntity(object entitySource)
        // {
        //     var source = entitySource;
        //     foreach (var p in Photos)
        //     {
        //         Photos.Remove(p);
        //     }
        //     foreach (var photo in (entitySource as O2EvEvent).Photos)
        //     {
        //         Photos.Add(photo);
        //     }
        //     base.UpdateEntity(entitySource);
        // }
    }
}