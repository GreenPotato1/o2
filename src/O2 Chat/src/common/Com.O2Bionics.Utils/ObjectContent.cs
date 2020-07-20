using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using pair = System.Collections.Generic.KeyValuePair<string, string>;

namespace Com.O2Bionics.Utils
{
    /// <inheritdoc />
    /// <summary>
    /// Automatically fill in the <seealso cref="T:System.Net.Http.FormUrlEncodedContent" /> from an instance of <typeparamref name="T" />.
    /// </summary>
    public sealed class ObjectContent<T> : FormUrlEncodedContent
        where T : class
    {
        public ObjectContent([NotNull] T value) : base(Format(value))
        {
        }

        [NotNull]
        private static List<pair> Format([NotNull] T instance)
        {
            if (null == instance)
                throw new ArgumentNullException();

            var type = instance.GetType();
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var result = new List<pair>(properties.Length);

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < properties.Length; i++)
            {
                var value = GetValue(instance, properties[i]);
                if (!string.IsNullOrEmpty(value))
                    result.Add(new pair(properties[i].Name, value));
            }

            return result;
        }

        private static string GetValue(T instance, PropertyInfo property)
        {
            string value = null;

            if (typeof(string) == property.PropertyType)
                value = property.GetValue(instance) as string;
            //numbers
            else if (typeof(decimal) == property.PropertyType)
            {
                var raw = property.GetValue(instance);
                if (raw is decimal dec)
                    value = dec.ToString(CultureInfo.InvariantCulture);
            }
            else if (typeof(int) == property.PropertyType)
            {
                var raw = property.GetValue(instance);
                if (raw is int dec)
                    value = dec.ToString(CultureInfo.InvariantCulture);
            }
            else if (typeof(long) == property.PropertyType)
            {
                var raw = property.GetValue(instance);
                if (raw is long dec)
                    value = dec.ToString(CultureInfo.InvariantCulture);
            }
            else if (typeof(ulong) == property.PropertyType)
            {
                var raw = property.GetValue(instance);
                if (raw is ulong dec)
                    value = dec.ToString(CultureInfo.InvariantCulture);
            }
            else if (typeof(uint) == property.PropertyType)
            {
                var raw = property.GetValue(instance);
                if (raw is uint dec)
                    value = dec.ToString(CultureInfo.InvariantCulture);
            }
            else if (typeof(Enum) == property.PropertyType.BaseType)
            {
                var raw = property.GetValue(instance);
                if (null != raw)
                    value = raw.ToString();
            }
            else if (typeof(DateTime) == property.PropertyType)
            {
                var raw = property.GetValue(instance);
                if (raw is DateTime date)
                    value = date.ToString(CultureInfo.InvariantCulture);
            }
            else if (typeof(bool) == property.PropertyType)
            {
                var raw = property.GetValue(instance);
                if (raw is bool b)
                    value = b.ToString();
            }
            else if (typeof(HashSet<decimal>) == property.PropertyType)
            {
                var raw = property.GetValue(instance);
                if (raw is HashSet<decimal> h && 0 < h.Count)
                    value = SerializeHashset(h);
            }
            else if (typeof(HashSet<uint>) == property.PropertyType)
            {
                var raw = property.GetValue(instance);
                if (raw is HashSet<uint> h && 0 < h.Count)
                    value = SerializeHashset(h);
            }
            else
            {
                throw new Exception("Unknown property.PropertyType=" + property.PropertyType.FullName);
            }

            return value;
        }

        private static string SerializeHashset<TU>(HashSet<TU> instance)
        {
            var builder = new StringBuilder();
            foreach (var id in instance)
            {
                if (0 < builder.Length)
                    builder.Append(',');
                builder.Append(id);
            }

            return builder.ToString();
        }
    }
}