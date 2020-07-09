using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using O2.Black.Toolkit.Core;

namespace O2.Certificate.API.DTOs.O2Ev
{
    public class O2EvEventPhotoDto
    {
        public IFormFile File { get; set; }
    }
}