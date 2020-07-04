using System;
using Newtonsoft.Json;

namespace O2.Business.API.DTOs.O2Ev
{
    public class O2EvEventReturnDto
    {
        [JsonProperty(PropertyName="id")]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName="addedDate")]
        public long AddedDate { get; set; }

        [JsonProperty(PropertyName="modifiedDate")]
        public long ModifiedDate { get; set; }
        
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "shortDescription")]
        public string ShortDescription { get; set; }

        [JsonProperty(PropertyName = "startDate")]
        public long StartDate { get; set; }

        [JsonProperty(PropertyName = "endDate")]
        public long EndDate { get; set; }

        [JsonProperty(PropertyName = "allDay")]
        public bool AllDay { get; set; }

        [JsonProperty(PropertyName = "meta")]
        public O2EvMetaDto Meta { get; set; }
        
        [JsonProperty(PropertyName = "photoUrl")]
        public string PhotoUrl { get; set; }
    }
}