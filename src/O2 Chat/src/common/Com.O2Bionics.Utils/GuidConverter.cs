using System;
using Newtonsoft.Json;

namespace Com.O2Bionics.Utils
{
    public class GuidConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Guid);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var v = (Guid)value;
            writer.WriteValue(v.AsWebString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    return Guid.Empty;
                case JsonToken.String:
                    var s = reader.Value as string;
                    return string.IsNullOrEmpty(s) ? Guid.Empty : s.FromWebGuid();
                default:
                    throw new ArgumentException("Invalid token type");
            }
        }
    }

    public class NullableGuidConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Guid?);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var v = (Guid?)value;
            if (v.HasValue)
                writer.WriteValue(v.Value.AsWebString());
            else
                writer.WriteNull();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    return null;
                case JsonToken.String:
                    var s = reader.Value as string;
                    return string.IsNullOrEmpty(s) ? (Guid?)null : s.FromWebGuid();
                default:
                    throw new ArgumentException("Invalid token type");
            }
        }
    }
}