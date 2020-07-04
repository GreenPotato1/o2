using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace O2.Business.API.DTOs.O2C
{
    [DataContract]
    public class O2CContactDto
    {
        [DataMember(Name="key")] 
        [JsonProperty(PropertyName = "key")]
        public string Key { get; set; }

        [DataMember(Name="value")]
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }
}