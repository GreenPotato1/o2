using System.Runtime.Serialization;
using Nest;

namespace Com.O2Bionics.PageTracker.Contract
{
    [DataContract]
    public sealed class UriInfo
    {
        [DataMember(IsRequired = true)]
        [Keyword(Index = false)]
        public string Url { get; set; }

        #region Url parts can be used for statistics

        [DataMember]
        [Keyword(IgnoreAbove = 10)]
        public string Scheme { get; set; }

        [DataMember]
        [Keyword]
        public string Host { get; set; }

        [DataMember]
        [Number(NumberType.Short)]
        public int Port { get; set; }

        [DataMember]
        [Keyword]
        public string Path { get; set; }

        [DataMember]
        [Keyword(Index = false)]
        public string Query { get; set; }

        [DataMember]
        [Keyword]
        public string Fragment { get; set; }

        #endregion

        public override string ToString()
        {
            return $"Url={Url}";
        }
    }
}