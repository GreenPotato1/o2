using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace O2.ArenaS.Helper
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
