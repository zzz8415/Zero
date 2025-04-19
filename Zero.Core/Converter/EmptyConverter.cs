using Microsoft.AspNetCore.Http;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Zero.Core.Converter
{
    public sealed class EmptyConverter(string dateFormat = null) : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsValueType || // 值类型
                   typeToConvert.IsArray || // 数组
                   typeToConvert.IsGenericType; // 泛型
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(EmptyStringToDefaultConverter<>).MakeGenericType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(converterType, [options, dateFormat]);
        }
    }

    public class EmptyStringToDefaultConverter<T> : JsonConverter<T>
    {
        private readonly JsonSerializerOptions _safeOptions;

        private readonly string _dateFormat;

        public EmptyStringToDefaultConverter(JsonSerializerOptions parentOptions, string dateFormat = null)
        {
            // 克隆父级配置
            _safeOptions = new JsonSerializerOptions(parentOptions);

            _dateFormat = dateFormat;

            // 移除所有 EmptyConverter 实例
            var convertersToRemove = _safeOptions.Converters
                .Where(c => c is EmptyConverter)
                .ToList();

            foreach (var converter in convertersToRemove)
            {
                _safeOptions.Converters.Remove(converter);
            }
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var dateString = reader.GetString();
                if (string.IsNullOrEmpty(dateString))
                {
                    return default;
                }

                if (!string.IsNullOrEmpty(_dateFormat) && (typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?)))
                {
                    if (DateTime.TryParseExact(dateString, _dateFormat, null, DateTimeStyles.None, out var date))
                    {
                        return (T)(object)date;
                    }
                    else
                    {
                        return default;
                    }
                }
            }

            return JsonSerializer.Deserialize<T>(ref reader, _safeOptions);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (!string.IsNullOrEmpty(_dateFormat) && (typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?)))
            {
                if (value != null)
                {
                    writer.WriteStringValue(((DateTime)(object)value).ToString(_dateFormat));
                    return;
                }
            }

            JsonSerializer.Serialize(writer, value, _safeOptions);
        }
    }
}
