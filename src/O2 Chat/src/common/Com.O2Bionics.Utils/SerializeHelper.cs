using JetBrains.Annotations;

namespace Com.O2Bionics.Utils
{
    public static class SerializeHelper
    {
        [NotNull]
        public static string AsJavaScriptString(this bool value)
        {
            var result = value ? "true" : "false";
            return result;
        }

        /// <summary>
        /// The returned value must be disposed.
        /// </summary>
        public static ObjectContent<T> CreateContent<T>([NotNull] this T instance)
            where T : class
        {
            var result = new ObjectContent<T>(instance);
            return result;
        }
    }
}