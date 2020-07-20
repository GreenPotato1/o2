using System.Runtime.Serialization;

namespace Com.O2Bionics.PageTracker.Contract
{
    [DataContract]
    public sealed class Point
    {
        //Elastic requires such names.

        [DataMember]
        // ReSharper disable InconsistentNaming
        public double lat { get; set; }

        [DataMember]
        public double lon { get; set; }
        // ReSharper restore InconsistentNaming
    }
}