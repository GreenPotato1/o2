using Jil;
using Newtonsoft.Json;

namespace Com.O2Bionics.Utils
{
    public static class JsonSerializerBuilder
    {
        // JsonSerializer is threadsafe for Serialize and Deserialize calls
        // see http://stackoverflow.com/questions/36186276/is-the-json-net-jsonserializer-threadsafe
        public static readonly JsonSerializer Default;

        static JsonSerializerBuilder()
        {
            Default = new JsonSerializer
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                };
            Default.Converters.Add(new IpAddressConverter());
            Default.Converters.Add(new GuidConverter());
            Default.Converters.Add(new NullableGuidConverter());
        }

        public static readonly Options DefaultJilOptions =
            new Options(
                false,
                false,
                false,
                DateTimeFormat.ISO8601,
                true,
                UnspecifiedDateTimeKindBehavior.IsUTC,
                SerializationNameFormat.Verbatim);

        /// <summary>
        /// The null values won't be serialized.
        /// </summary>
        public static readonly Options SkipNullJilOptions =
            new Options(
                false,
                true,
                false,
                DateTimeFormat.ISO8601,
                true,
                UnspecifiedDateTimeKindBehavior.IsUTC,
                SerializationNameFormat.Verbatim);
    }
}