using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zero.Core.Converter
{
    /// <summary>
    /// int64转字符串
    /// </summary>
    public class LongToStringConverter : Newtonsoft.Json.JsonConverter
    {
        /// <summary>
        /// 读取json
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var jt = JToken.ReadFrom(reader);

            var val = jt.Value<string>();

            if (string.IsNullOrEmpty(val))
            {
                return null;
            }

            return jt.Value<long>();
        }

        /// <summary>
        /// 可以转换
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return typeof(long) == objectType || typeof(long?) == objectType;
        }

        /// <summary>
        /// 转json
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value?.ToString());
        }
    }
}
