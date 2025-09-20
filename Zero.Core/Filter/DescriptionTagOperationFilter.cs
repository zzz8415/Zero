using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Zero.Core.Filter
{
    public class DescriptionTagOperationFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // 1. 先构建 tagName 映射表，避免多次反射
            var tagNameMap = new Dictionary<string, string>(); // oldName -> newName
            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && t.Name.EndsWith("Controller"));
            foreach (var type in allTypes)
            {
                var descAttr = type.GetCustomAttribute<DescriptionAttribute>();
                if (descAttr != null && !string.IsNullOrWhiteSpace(descAttr.Description))
                {
                    var oldName = type.Name.Replace("Controller", string.Empty);
                    tagNameMap[oldName] = descAttr.Description;
                }
            }

            // 2. 批量同步 swaggerDoc.Tags
            foreach (var tag in swaggerDoc.Tags)
            {
                if (tagNameMap.TryGetValue(tag.Name, out var newName))
                {
                    tag.Name = newName;
                }
            }

            // 3. 批量同步 operation.Tags
            foreach (var path in swaggerDoc.Paths.Values)
                foreach (var op in path.Operations.Values)
                {
                    for (int i = 0; i < op.Tags.Count; i++)
                    {
                        if (tagNameMap.TryGetValue(op.Tags[i].Name, out var newName))
                        {
                            op.Tags[i].Name = newName;
                        }
                    }
                }
        }
    }
}
