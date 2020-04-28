using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zero.NETCore.Converter
{
    public class LongToStringConverter : Newtonsoft.Json.JsonConverter
    {
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

        public override bool CanConvert(Type objectType)
        {
            return typeof(long) == objectType || typeof(long?) == objectType;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value?.ToString());
        }
    }
}
