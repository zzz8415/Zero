using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Zero.Core.Converter
{
    /// <summary>
    /// 时间格式转化
    /// </summary>
    public class DateTimeConverter(string format = "yyyy/MM/dd HH:mm:ss") : JsonConverter<object>
    {

        public override bool CanConvert(Type typeToConvert)
        {
            // 同时匹配 DateTime 和 DateTime?
            return typeToConvert == typeof(DateTime) ||
                   typeToConvert == typeof(DateTime?);
        }

        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return typeToConvert == typeof(DateTime?) ? null : default(DateTime);
            }

            if (reader.TokenType == JsonTokenType.String && string.IsNullOrEmpty(reader.GetString()))
            {
                return typeToConvert == typeof(DateTime?) ? null : default(DateTime);
            }

            return DateTime.Parse(reader.GetString()!);
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStringValue(((DateTime)value).ToString(format, CultureInfo.InvariantCulture));
            }
        }
    }
}
