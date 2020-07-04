using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace O2.Business.API.DTOs
{
    public class ImportDto
    {
        
        [JsonProperty(PropertyName = "file")] 
        public IFormFile File { get; set; }

        [JsonProperty(PropertyName = "cleanData")]
        public bool CleanData { get; set; } = false;
    }
}