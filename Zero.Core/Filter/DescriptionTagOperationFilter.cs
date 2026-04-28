using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;

using Swashbuckle.AspNetCore.Annotations;
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
            // 1. 构建控制器 tag 名称和描述映射
            var oldToNewTagName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var tagDescriptionByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var controllerNameToFullName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(GetLoadableTypes)
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Controller", StringComparison.Ordinal));

            foreach (var type in allTypes)
            {
                var descAttr = type.GetCustomAttribute<DescriptionAttribute>();
                var swaggerTagAttr = type.GetCustomAttribute<SwaggerTagAttribute>();
                var oldName = type.Name.Replace("Controller", string.Empty, StringComparison.Ordinal);
                var nsKey = string.IsNullOrEmpty(type.Namespace) ? oldName : ($"{type.Namespace}.{oldName}");
                var fullKey = type.FullName ?? nsKey;

                var newName = string.IsNullOrWhiteSpace(descAttr?.Description)
                    ? oldName
                    : descAttr.Description.Trim();

                oldToNewTagName[nsKey] = newName;
                controllerNameToFullName[nsKey] = fullKey;

                if (!string.IsNullOrWhiteSpace(swaggerTagAttr?.Description))
                {
                    var desc = swaggerTagAttr.Description.Trim();
                    tagDescriptionByName[nsKey] = desc;
                    tagDescriptionByName[newName] = desc;
                }
            }

            // 2. 先重写 operation.Tags，使其与 Description 映射后的名称一致。
            foreach (var path in swaggerDoc.Paths.Values)
            {
                if (path.Operations == null)
                {
                    continue;
                }

                foreach (var op in path.Operations.Values)
                {
                    if (op.Tags == null || op.Tags.Count == 0)
                    {
                        continue;
                    }

                    var rewrittenTags = new HashSet<OpenApiTagReference>();
                    foreach (var tag in op.Tags)
                    {
                        if (string.IsNullOrWhiteSpace(tag.Name))
                        {
                            continue;
                        }

                        var rawName = tag.Name;
                        var normalizedName = rawName.StartsWith("#/components/tags/", StringComparison.Ordinal)
                            ? rawName.Substring("#/components/tags/".Length)
                            : rawName;

                        // 优先用 命名空间+ControllerName 匹配
                        var nsKey = context.ApiDescriptions
                            .Select(d => d.ActionDescriptor is ControllerActionDescriptor cad ? $"{cad.ControllerTypeInfo.Namespace}.{cad.ControllerName}" : null)
                            .FirstOrDefault(k => k != null && (k.EndsWith($".{normalizedName}") || k == normalizedName)) ?? normalizedName;

                        if (controllerNameToFullName.TryGetValue(nsKey, out var fullKey) && oldToNewTagName.TryGetValue(nsKey, out var renamed))
                        {
                            normalizedName = renamed;
                        }
                        else if (oldToNewTagName.TryGetValue(nsKey, out var renamed2))
                        {
                            normalizedName = renamed2;
                        }

                        // 使用 null externalResource，序列化为普通 tag 字符串，而不是 ref 路径。
                        rewrittenTags.Add(new OpenApiTagReference(normalizedName, swaggerDoc, null));
                    }

                    if (rewrittenTags.Count > 0)
                    {
                        op.Tags = rewrittenTags;
                    }
                }
            }

            // 3. 重建 swaggerDoc.Tags，确保和 operation.Tags 同名同描述。
            var usedTagNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var path in swaggerDoc.Paths.Values)
            {
                if (path.Operations == null)
                {
                    continue;
                }

                foreach (var op in path.Operations.Values)
                {
                    if (op.Tags == null)
                    {
                        continue;
                    }

                    foreach (var tag in op.Tags)
                    {
                        if (string.IsNullOrWhiteSpace(tag.Name))
                        {
                            continue;
                        }

                        // 兼容历史错误数据：把 #/components/tags/xxx 还原成 xxx
                        var tagName = tag.Name.StartsWith("#/components/tags/", StringComparison.Ordinal)
                            ? tag.Name.Substring("#/components/tags/".Length)
                            : tag.Name;

                        // 优先用 命名空间+ControllerName 匹配
                        var nsKey = context.ApiDescriptions
                            .Select(d => d.ActionDescriptor is ControllerActionDescriptor cad ? $"{cad.ControllerTypeInfo.Namespace}.{cad.ControllerName}" : null)
                            .FirstOrDefault(k => k != null && (k.EndsWith($".{tagName}") || k == tagName)) ?? tagName;

                        if (controllerNameToFullName.TryGetValue(nsKey, out var fullKey) && oldToNewTagName.TryGetValue(nsKey, out var renamed))
                        {
                            tagName = renamed;
                        }
                        else if (oldToNewTagName.TryGetValue(nsKey, out var renamed2))
                        {
                            tagName = renamed2;
                        }

                        usedTagNames.Add(tagName);
                    }
                }
            }

            var docTags = new HashSet<OpenApiTag>();
            foreach (var tagName in usedTagNames.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                tagDescriptionByName.TryGetValue(tagName, out var description);

                docTags.Add(new OpenApiTag
                {
                    Name = tagName,
                    Description = description
                });
            }

            swaggerDoc.Tags = docTags;
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null)!;
            }
        }
    }
}
