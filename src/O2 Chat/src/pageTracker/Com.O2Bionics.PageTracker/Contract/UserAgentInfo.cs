using System.Runtime.Serialization;
using Nest;

namespace Com.O2Bionics.PageTracker.Contract
{
    [DataContract]
    public sealed class UserAgentInfo
    {
        [DataMember]
        [Keyword(IgnoreAbove = 64)]
        public string Device { get; set; }

        [DataMember]
        [Keyword(IgnoreAbove = 64)]
        public string Os { get; set; }

        [DataMember]
        [Keyword(IgnoreAbove = 64)]
        public string UserAgent { get; set; }

        [DataMember]
        [Keyword(IgnoreAbove = PageTrackerConstants.UserAgentStringMaxLength, Index = false)]
        public string UserAgentString { get; set; }

        public override string ToString()
        {
            return $"Device='{Device}', OS='{Os}', UserAgent='{UserAgent}', UserAgentString='{UserAgentString}'";
        }
    }
}