using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;

using Zero.Core.Inject;
using Zero.Core.Converter;

namespace Zero.Core.Extensions
{
    /// <summary>
    /// Json序列化和反序列化方法
    /// </summary>
    public static class JsonExtensions
    {
        private static readonly JsonSerializerOptions DefaultOptions = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles, // 忽略循环引用
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // 忽略null值
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true, // 不区分大小写
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters = { new EmptyConverter("yyyy-MM-dd HH:mm:ss") }
        };

        /// <summary>
        /// 对象转换为Json字符串
        /// </summary>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="options">Json序列化选项</param>
        /// <returns>Json字符串</returns>
        public static string ToJson<T>(this T obj, JsonSerializerOptions options = null)
        {
            if (EqualityComparer<T>.Default.Equals(obj, default))
            {
                return null;
            }

            options ??= DefaultOptions;
            return JsonSerializer.Serialize(obj, options);
        }

        /// <summary>
        /// Json字符串转换为对象
        /// </summary>
        /// <param name="json">Json字符串</param>
        /// <param name="options">Json反序列化选项</param>
        /// <returns>反序列化后的对象</returns>
        public static T DeserializeJson<T>(this string json, JsonSerializerOptions options = null) where T : class
        {
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            options ??= DefaultOptions;
            return JsonSerializer.Deserialize<T>(json, options);
        }

        /// <summary>
        /// 尝试解析json字符串,不抛出异常
        /// </summary>
        /// <param name="json">Json字符串</param>
        /// <param name="options">Json反序列化选项</param>
        /// <returns>反序列化后的对象或默认值</returns>
        public static T TryDeserializeJson<T>(this string json, JsonSerializerOptions options = null) where T : class
        {
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            try
            {
                return json.DeserializeJson<T>(options);
            }
            catch
            {
                return default;
            }
        }

    }
}
