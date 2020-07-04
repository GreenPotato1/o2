using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace O2.Business.API.Helper
{
    public class RemoveVersionFromParameter : IOperationFilter
    {

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var versionParameter = operation.Parameters.Single(p => p.Name == "v");
            operation.Parameters.Remove(versionParameter);
        }
    }
}
