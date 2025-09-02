using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zero.Core.Filter
{
    public class RequestBodyToParametersFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.RequestBody == null) return;

            var requestBody = operation.RequestBody;
            var schema = requestBody.Content.FirstOrDefault().Value?.Schema;

            if (schema?.Properties != null)
            {
                foreach (var property in schema.Properties)
                {
                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = property.Key,
                        In = ParameterLocation.Query, // 你也可以改成 Header / Path
                        Schema = property.Value,
                        Required = schema.Required.Contains(property.Key)
                    });
                }
            }

            // 移除原本的 requestBody，避免重复
            operation.RequestBody = null;
        }
    }
}
