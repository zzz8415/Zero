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
    public sealed class EmptyConverter(string[] dateFormatArray = null) : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert.IsValueType;

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert.IsEnum)
            {
                var enumConverterType = typeof(EnumConverter<>).MakeGenericType(typeToConvert);
                return (JsonConverter)Activator.CreateInstance(enumConverterType);
            }

            var converterType = typeof(EmptyStringConverter<>).MakeGenericType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(converterType, [options, dateFormatArray]);
        }
    }

    public class EmptyStringConverter<T> : JsonConverter<T>
    {
        private readonly JsonSerializerOptions _safeOptions;

        private readonly string[] _dateFormatArray;

        public EmptyStringConverter(JsonSerializerOptions parentOptions, string[] dateFormatArray = null)
        {
            // 克隆父级配置
            _safeOptions = new JsonSerializerOptions(parentOptions);

            _dateFormatArray = dateFormatArray;

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

                if (_dateFormatArray?.Length > 0 && (typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?)))
                {
                    foreach (var format in _dateFormatArray)
                    {
                        if (DateTime.TryParseExact(dateString, format, null, DateTimeStyles.None, out var date))
                        {
                            return (T)(object)date;
                        }
                    }
                    return default;
                }

                if (_dateFormatArray?.Length > 0 && (typeof(T) == typeof(DateTimeOffset) || typeof(T) == typeof(DateTimeOffset?)))
                {
                    foreach (var format in _dateFormatArray) 
                    {
                        if (DateTimeOffset.TryParseExact(dateString, format, null, DateTimeStyles.None, out var date))
                        {
                            return (T)(object)date;
                        }
                    }
                    return default;
                }
            }

            return JsonSerializer.Deserialize<T>(ref reader, _safeOptions);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, _safeOptions);
        }
    }
    public class EnumConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
    {
        public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    var stringValue = reader.GetString();
                    if (Enum.TryParse<TEnum>(stringValue, ignoreCase: true, out var enumValue))
                        return enumValue;
                    break;

                case JsonTokenType.Number:
                    var underlyingType = Enum.GetUnderlyingType(typeof(TEnum));

                    if (underlyingType == typeof(long))
                    {
                        if (reader.TryGetInt64(out var longValue))
                            return (TEnum)Enum.ToObject(typeof(TEnum), longValue);
                    }
                    else if (underlyingType == typeof(ulong))
                    {
                        if (reader.TryGetUInt64(out var ulongValue))
                            return (TEnum)Enum.ToObject(typeof(TEnum), ulongValue);
                    }
                    else if (underlyingType == typeof(int))
                    {
                        if (reader.TryGetInt32(out var intValue))
                            return (TEnum)Enum.ToObject(typeof(TEnum), intValue);
                    }
                    else if (underlyingType == typeof(uint))
                    {
                        if (reader.TryGetUInt32(out var uintValue))
                            return (TEnum)Enum.ToObject(typeof(TEnum), uintValue);
                    }
                    else if (underlyingType == typeof(short))
                    {
                        if (reader.TryGetInt16(out var shortValue))
                            return (TEnum)Enum.ToObject(typeof(TEnum), shortValue);
                    }
                    else if (underlyingType == typeof(ushort))
                    {
                        if (reader.TryGetUInt16(out var ushortValue))
                            return (TEnum)Enum.ToObject(typeof(TEnum), ushortValue);
                    }
                    else if (underlyingType == typeof(byte))
                    {
                        if (reader.TryGetByte(out var byteValue))
                            return (TEnum)Enum.ToObject(typeof(TEnum), byteValue);
                    }
                    else if (underlyingType == typeof(sbyte))
                    {
                        if (reader.TryGetSByte(out var sbyteValue))
                            return (TEnum)Enum.ToObject(typeof(TEnum), sbyteValue);
                    }
                    break;
            }
            return default;
        }

        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        {
            var underlyingType = Enum.GetUnderlyingType(typeof(TEnum));

            switch (underlyingType)
            {
                case Type t when t == typeof(long):
                    writer.WriteNumberValue(Convert.ToInt64(value));
                    break;
                case Type t when t == typeof(ulong):
                    writer.WriteNumberValue(Convert.ToUInt64(value));
                    break;
                case Type t when t == typeof(int):
                    writer.WriteNumberValue(Convert.ToInt32(value));
                    break;
                case Type t when t == typeof(uint):
                    writer.WriteNumberValue(Convert.ToUInt32(value));
                    break;
                case Type t when t == typeof(short):
                    writer.WriteNumberValue(Convert.ToInt16(value));
                    break;
                case Type t when t == typeof(ushort):
                    writer.WriteNumberValue(Convert.ToUInt16(value));
                    break;
                case Type t when t == typeof(byte):
                    writer.WriteNumberValue(Convert.ToByte(value));
                    break;
                case Type t when t == typeof(sbyte):
                    writer.WriteNumberValue(Convert.ToSByte(value));
                    break;
                default:
                    writer.WriteStringValue(value.ToString());
                    break;
            }
        }
    }

}
