using System;

namespace O2.Certificate.Data.Models.O2Ev
{
   
    public class O2EvPhoto : Photo
    {
        public O2EvEvent O2EvEvent { get; set; }
        public Guid O2EvEventId { get; set; }

    }
}