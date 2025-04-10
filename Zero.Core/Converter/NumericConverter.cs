using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zero.Core.Converter
{
    /// <summary>
    /// 长数字转字符串类型,避免js精度不足
    /// </summary>
    public class NumericConverter : JsonConverter<object>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeToConvert"></param>
        /// <returns></returns>
        public override bool CanConvert(Type typeToConvert)
        {
            // 支持以下类型：
            return typeToConvert == typeof(long) || typeToConvert == typeof(long?) ||
                   typeToConvert == typeof(decimal) || typeToConvert == typeof(decimal?);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                // 返回可空类型的默认值（null）或值类型的默认值（0）
                return typeToConvert == typeof(long?) || typeToConvert == typeof(decimal?)
                    ? null
                    : (typeToConvert == typeof(long) ? 0L : 0M);
            }

            // 根据目标类型解析字符串
            string strValue = reader.GetString()!;
            if (typeToConvert == typeof(long) || typeToConvert == typeof(long?))
            {
                return long.Parse(strValue);
            }
            else
            {
                return decimal.Parse(strValue, CultureInfo.InvariantCulture); // 避免区域性差异
            }
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                // 统一序列化为字符串
                writer.WriteStringValue(value.ToString()!);
            }
        }
    }
}
