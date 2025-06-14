using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Zero.Core.Extensions;

namespace Zero.Core.Filter
{
    /// <summary>
    /// 获取枚举类型的描述信息，并将其添加到 OpenApiSchema 的描述中。
    /// </summary>
    public class EnumDescriptionSchemaFilter : ISchemaFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            var type = context.Type;

            // 如果类型不是枚举，则直接返回，不做任何处理。
            if (!type.IsEnum)
            {
                return;
            }

            // 使用 StringBuilder 来高效地构建字符串，特别是在枚举值较多时。
            var descriptionBuilder = new StringBuilder();

            var enumValues = Enum.GetValues(type);

            var line = false;
            if (!schema.Description.IsNullOrEmpty())
            {
                descriptionBuilder.Append(schema.Description);
                line = true;
            }

            foreach (Enum enumValue in enumValues)
            {
                var name = enumValue.ToString();

                // 尝试获取 DescriptionAttribute。
                // 如果存在 DescriptionAttribute，则使用其描述文本；否则，使用枚举成员的名称。
                var description = enumValue.GetDescription();

                if (line)
                {
                    descriptionBuilder.AppendLine("<br/>");
                }
                else
                {
                    line = true;
                }
                // 格式化枚举值和描述，并追加到 StringBuilder。
                descriptionBuilder.Append($"{Convert.ToInt32(enumValue)} = {name}({description})");
            }

            schema.Description = descriptionBuilder.ToString();
        }
    }
}
