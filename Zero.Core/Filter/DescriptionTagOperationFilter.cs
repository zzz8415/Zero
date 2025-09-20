using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zero.Core.Filter
{
    public class DescriptionTagOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ActionDescriptor is not ControllerActionDescriptor cad) return;

            var controllerType = cad.ControllerTypeInfo;
            var controllerName = cad.ControllerName;

            var descriptionAttr = controllerType
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .Cast<DescriptionAttribute>()
                .FirstOrDefault();

            var tagName = descriptionAttr?.Description ?? controllerName;
            operation.Tags.Add(new OpenApiTag { Name = tagName, Description = tagName });
        }
    }
}
