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
    public sealed class EmptyConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsValueType ||
                   Nullable.GetUnderlyingType(typeToConvert) != null ||
                   typeToConvert == typeof(string);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(EmptyStringToDefaultConverter<>).MakeGenericType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(converterType, [options]);
        }
    }

    public class EmptyStringToDefaultConverter<T> : JsonConverter<T>
    {
        private readonly JsonSerializerOptions _safeOptions;

        public EmptyStringToDefaultConverter(JsonSerializerOptions parentOptions)
        {
            // 克隆父级配置
            _safeOptions = new JsonSerializerOptions(parentOptions);

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
            try
            {
                if (reader.TokenType == JsonTokenType.Null ||
                    (reader.TokenType == JsonTokenType.String && reader.GetString() == string.Empty))
                {
                    return GetDefaultValue(typeToConvert);
                }

                return JsonSerializer.Deserialize<T>(ref reader, _safeOptions);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"反序列化失败: {ex.Message}");
                return GetDefaultValue(typeToConvert);
            }
        }

        private static T GetDefaultValue(Type typeToConvert)
        {
            if (typeToConvert == typeof(string))
                return default;

            var underlyingType = Nullable.GetUnderlyingType(typeToConvert);
            if (underlyingType != null)
                return default;

            return Activator.CreateInstance<T>();
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, _safeOptions);
        }
    }
}
