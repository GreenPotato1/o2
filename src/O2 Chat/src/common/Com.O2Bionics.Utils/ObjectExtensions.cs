using System.IO;
using System.Runtime.CompilerServices;
using Jil;
using Newtonsoft.Json;

namespace Com.O2Bionics.Utils
{
    public static class ObjectExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string JsonStringify(this object x)
        {
            if (ReferenceEquals(x, null)) return "null";

            var serializer = JsonSerializerBuilder.Default;
            var sw = new StringWriter();
            using (var wr = new JsonTextWriter(sw))
                serializer.Serialize(wr, x);
            return sw.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string JsonStringify2<T>(this T x)
        {
            return ReferenceEquals(x, null)
                ? "null"
                : JSON.Serialize(x, JsonSerializerBuilder.DefaultJilOptions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T JsonUnstringify2<T>(this string s) where T : class
        {
            return string.IsNullOrEmpty(s)
                ? null
                : JSON.Deserialize<T>(s, JsonSerializerBuilder.DefaultJilOptions);
        }
    }
}