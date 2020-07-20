using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;
using Nest;

namespace Com.O2Bionics.Elastic
{
    public static class TypeMappingDescriptorExtensions
    {
        [NotNull]
        public static TypeMappingDescriptor<T> SetLongStringsSize<T>([NotNull] this TypeMappingDescriptor<T> typeMappingDescriptor) where T : class
        {
            var type = typeof(T);
            var names = GetStringPropertiesAndFields(type).ToList();
#if DEBUG
            if (0 == names.Count)
                throw new Exception(
                    $"Debug. Type '{type.FullName}' must have at least one string field or property, marked with {nameof(LongStringAttribute)}.");
#endif
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < names.Count; i++)
                SetKeywordSize(typeMappingDescriptor, names[i]);

            return typeMappingDescriptor;
        }

        [NotNull]
        private static IEnumerable<string> GetStringPropertiesAndFields([NotNull] Type type)
        {
            var stringType = typeof(string);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var info in properties)
            {
                if (info.PropertyType.Name != stringType.Name)
                    continue;

                var attribute = info.GetCustomAttribute<LongStringAttribute>();
                if (null == attribute)
                    continue;

                yield return info.Name;
            }

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var info in fields)
            {
                if (info.FieldType.Name != stringType.Name)
                    continue;

                var attribute = info.GetCustomAttribute<LongStringAttribute>();
                if (null == attribute)
                    continue;

                yield return info.Name;
            }
        }

        private static void SetKeywordSize<T>(
            [NotNull] TypeMappingDescriptor<T> typeMappingDescriptor,
            [NotNull] string fieldName) where T : class
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException(nameof(fieldName));

            typeMappingDescriptor.Properties(
                p => p.Text(
                    t => t.Name(fieldName).Fields(
                        f => f.Keyword(
                            k => k.Name(FieldConstants.Keyword).IgnoreAbove(FieldConstants.IgnoreAbove)))));
        }
    }
}