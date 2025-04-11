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
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(long) || typeToConvert == typeof(long?) ||
                   typeToConvert == typeof(decimal) || typeToConvert == typeof(decimal?);
        }

        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // 处理 null
            if (reader.TokenType == JsonTokenType.Null)
            {
                return Nullable.GetUnderlyingType(typeToConvert) == null ? null : GetDefaultValue(typeToConvert);
            }

            // 处理数字（直接读取）
            if (reader.TokenType == JsonTokenType.Number)
            {
                return typeToConvert == typeof(long) || typeToConvert == typeof(long?)
                    ? reader.GetInt64()
                    : reader.GetDecimal();
            }

            // 处理字符串（尝试解析为数字）
            if (reader.TokenType == JsonTokenType.String)
            {
                string strValue = reader.GetString()!;
                if (!string.IsNullOrEmpty(strValue))
                {
                    if (typeToConvert == typeof(long) || typeToConvert == typeof(long?))
                    {
                        if (long.TryParse(strValue, NumberStyles.Any, CultureInfo.InvariantCulture, out long longResult))
                        {
                            return longResult;
                        }
                    }
                    else
                    {
                        if (decimal.TryParse(strValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decimalResult))
                        {
                            return decimalResult;
                        }
                    }
                }
            }

            // 其他所有情况（true/false/{}等）强制返回 0 或 null
            return Nullable.GetUnderlyingType(typeToConvert) == null ? null : GetDefaultValue(typeToConvert);
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStringValue(value.ToString()!);
            }
        }

        private static object GetDefaultValue(Type type)
        {
            return type == typeof(long) ? 0L : 0M;
        }
    }
}
