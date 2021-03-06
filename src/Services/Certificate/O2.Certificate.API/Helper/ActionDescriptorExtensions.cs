using Microsoft.AspNetCore.Mvc.Versioning;
using System.Linq;
using System;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace O2.Certificate.API.Helper
{
    public static class ActionDescriptorExtensions
    {
        public static ApiVersionModel GetApiVersion(this ActionDescriptor actionDescriptor)
        {
            return actionDescriptor?.Properties
              .Where((kvp) => ((Type)kvp.Key).Equals(typeof(ApiVersionModel)))
              .Select(kvp => kvp.Value as ApiVersionModel).FirstOrDefault();
        }
    }
}
