using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace O2.Certificate.API.DTOs.O2Ev
{
    [DataContract]
    public class O2EvMetaDto
    {
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName="id")]
        public Guid Id { get; set; }

        [DataMember(Name = "addedDate")]
        [JsonProperty(PropertyName="addedDate")]
        public long AddedDate { get; set; }

        [DataMember(Name = "modifiedDate")]
        [JsonProperty(PropertyName="modifiedDate")]
        public long ModifiedDate { get; set; }

        [DataMember(Name = "country")]
        [JsonProperty(PropertyName="country")]
        public string Country { get; set; }

        [DataMember(Name = "region")]
        [JsonProperty(PropertyName="region")]
        public string Region { get; set; }
    }
}