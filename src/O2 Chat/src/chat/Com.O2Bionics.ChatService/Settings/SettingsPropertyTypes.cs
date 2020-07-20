using System;
using System.Collections.Generic;
using System.Globalization;

namespace Com.O2Bionics.ChatService.Settings
{
    public static class SettingsPropertyTypes
    {
        public static readonly Dictionary<Type, SettingsPropertyTypeInfo> Types =
            new Dictionary<Type, SettingsPropertyTypeInfo>
                {
                    { typeof(bool), new SettingsPropertyTypeInfo("Bool", v => v.ToString(), s => bool.Parse(s)) },
                    {
                        typeof(bool?),
                        new SettingsPropertyTypeInfo("Bool?", v => v == null ? null : v.ToString(), s => ParseNullable(s, bool.Parse))
                    },
                    { typeof(int), new SettingsPropertyTypeInfo("Int32", v => v.ToString(), s => Int32.Parse(s)) },
                    {
                        typeof(int?),
                        new SettingsPropertyTypeInfo("Int32?", v => v == null ? null : v.ToString(), s => ParseNullable(s, Int32.Parse))
                    },
                    { typeof(float), new SettingsPropertyTypeInfo("Single", v => v.ToString(), s => Single.Parse(s)) },
                    {
                        typeof(float?),
                        new SettingsPropertyTypeInfo("Single?", v => v == null ? null : v.ToString(), s => ParseNullable(s, Single.Parse))
                    },
                    { typeof(double), new SettingsPropertyTypeInfo("Double", v => v.ToString(), s => Double.Parse(s)) },
                    {
                        typeof(double?),
                        new SettingsPropertyTypeInfo("Double?", v => v == null ? null : v.ToString(), s => ParseNullable(s, Double.Parse))
                    },
                    { typeof(decimal), new SettingsPropertyTypeInfo("Decimal", v => v.ToString(), s => Decimal.Parse(s)) },
                    {
                        typeof(decimal?),
                        new SettingsPropertyTypeInfo("Decimal?", v => v == null ? null : v.ToString(), s => ParseNullable(s, Decimal.Parse))
                    },
                    { typeof(string), new SettingsPropertyTypeInfo("String", v => v == null ? null : v.ToString(), s => s) },
                    { typeof(DateTime), new SettingsPropertyTypeInfo("DateTime", v => ((DateTime)v).ToString("O"), s => ParseDateTime(s)) },
                    {
                        typeof(DateTime?),
                        new SettingsPropertyTypeInfo(
                            "DateTime?",
                            v => v == null ? null : ((DateTime)v).ToString("O"),
                            s => ParseNullable(s, ParseDateTime))
                    },
                    { typeof(TimeSpan), new SettingsPropertyTypeInfo("TimeSpan", v => ((TimeSpan)v).ToString("c"), s => ParseTimeSpan(s)) },
                    {
                        typeof(TimeSpan?),
                        new SettingsPropertyTypeInfo(
                            "TimeSpan?",
                            v => v == null ? null : ((TimeSpan)v).ToString("c"),
                            s => ParseNullable(s, ParseTimeSpan))
                    },
                };

        private static TimeSpan ParseTimeSpan(string s)
        {
            return TimeSpan.Parse(s);
        }

        private static DateTime ParseDateTime(string s)
        {
            return DateTime.Parse(s, CultureInfo.InvariantCulture);
        }

        private static T? ParseNullable<T>(string s, Func<string, T> parse) where T : struct
        {
            return string.IsNullOrWhiteSpace(s) ? new T?() : parse(s);
        }

        public static SettingsPropertyTypeInfo GetTypeInfo<T>()
        {
            var type = typeof(T);
            SettingsPropertyTypeInfo typeInfo;
            if (!Types.TryGetValue(type, out typeInfo))
                throw new ArgumentException("Unsupported property type " + type.FullName);
            return typeInfo;
        }
    }

    public class SettingsPropertyTypeInfo
    {
        public SettingsPropertyTypeInfo(string name, Func<object, string> serialize, Func<string, object> deserialize)
        {
            Name = name;
            Serialize = serialize;
            Deserialize = deserialize;
        }

        public string Name { get; private set; }
        public Func<object, string> Serialize { get; private set; }
        public Func<string, object> Deserialize { get; private set; }
    }
}