using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.Utils;
using log4net;
using ObjectExtensions.Copy;

namespace Com.O2Bionics.ChatService.Settings
{
    public abstract class SettingsBase
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(SettingsBase));

        protected class SettingsValue
        {
            public object Value { get; set; }
            public PROPERTY_BAG Record { get; set; }
        }

        public abstract class SettingsPropertyDescriptor
        {
            public object Default { get; protected set; }
            public SettingsPropertyTypeInfo TypeInfo { get; protected set; }

            public string TypeName
            {
                get { return TypeInfo.Name; }
            }

            public object Deserialize(string s)
            {
                return TypeInfo.Deserialize(s);
            }

            public string Serialize(object value)
            {
                return TypeInfo.Serialize(value);
            }
        }

        public class SettingsPropertyDescriptor<T> : SettingsPropertyDescriptor
        {
            public SettingsPropertyDescriptor(T defaultValue)
            {
                Default = defaultValue;
                TypeInfo = SettingsPropertyTypes.GetTypeInfo<T>();
            }
        }

        private readonly ReadOnlyDictionary<string, SettingsPropertyDescriptor> m_propertyDescriptors;
        protected readonly Dictionary<string, SettingsValue> Values;

        protected SettingsBase(ICollection<PROPERTY_BAG> records, ReadOnlyDictionary<string, SettingsPropertyDescriptor> propertyDescriptors)
        {
            
            m_propertyDescriptors = propertyDescriptors;
            var knownRecords = records
                .Where(x => m_propertyDescriptors.Keys.Contains(x.PROPERTY_NAME))
                .GroupBy(x => x.PROPERTY_NAME)
                .ToList();
            var duplicates = knownRecords.Where(x => x.Count() > 1).ToList();
            if (duplicates.Any())
            {
                var message = string.Join(
                    ", ",
                    duplicates
                        .Select(x => x.Key + ": [" + string.Join(", ", x.Select(y => y.PROPERTY_BAG_SKEY + ": " + y.PROPERTY_VALUE)) + "]"));
                m_log.WarnFormat("duplicate properties found: {0}", message);
            }

            Values = knownRecords
                .Select(x => x.OrderByDescending(y => y.PROPERTY_BAG_SKEY).First())
                .ToDictionary(
                    x => x.PROPERTY_NAME,
                    x => new SettingsValue { Record = x, Value = DeserializeValue(x) });
            var unknownProperties = records
                .Where(x => !m_propertyDescriptors.Keys.Contains(x.PROPERTY_NAME))
                .Select(x => x.PROPERTY_NAME)
                .ToList();
            if (unknownProperties.Any())
                m_log.WarnFormat(
                    "Unknown propertyDescriptors in db for type {0}: {1}",
                    GetType().Name,
                    string.Join(", ", unknownProperties));
        }

        protected SettingsBase(SettingsBase settings)
        {
            m_propertyDescriptors = settings.m_propertyDescriptors;
            Values = settings.Values.Copy();
        }


        protected T Get<T>(string name)
        {
            SettingsValue current;
            return !Values.TryGetValue(name, out current) ? (T)GetPropertyDescriptor(name).Default : (T)current.Value;
        }

        protected bool Has(string name)
        {
            return Values.ContainsKey(name);
        }

        protected T Default<T>(string name)
        {
            return (T)GetPropertyDescriptor(name).Default;
        }


        protected bool HasProperty(string name)
        {
            return m_propertyDescriptors.ContainsKey(name);
        }

        protected string GetPropertyTypeName(string name)
        {
            return GetPropertyDescriptor(name).TypeName;
        }

        private object DeserializeValue(PROPERTY_BAG p)
        {
            var descriptor = GetPropertyDescriptor(p.PROPERTY_NAME);
            try
            {
                return descriptor.Deserialize(p.PROPERTY_VALUE);
            }
            catch (Exception e)
            {
                m_log.Error(
                    $"Can't deserialize property {p.PROPERTY_NAME}, skey={p.PROPERTY_BAG_SKEY}, value='{p.PROPERTY_VALUE}' as {descriptor.TypeName}. Default value is used.",
                    e);
                return descriptor.Default;
            }
        }

        protected string SerializeValue(string name, object value)
        {
            return GetPropertyDescriptor(name).Serialize(value);
        }

        private SettingsPropertyDescriptor GetPropertyDescriptor(string name)
        {
            SettingsPropertyDescriptor property;
            if (!m_propertyDescriptors.TryGetValue(name, out property))
                throw new ArgumentException("Unknown property name " + name, "name");
            return property;
        }
    }

    public class WritableSettingsBase : SettingsBase
    {
        private readonly HashSet<string> m_dirty = new HashSet<string>();

        protected WritableSettingsBase(SettingsBase settings) : base(settings)
        {
        }

        protected WritableSettingsBase(ReadOnlyDictionary<string, SettingsPropertyDescriptor> propertyDescriptors)
            : base(new PROPERTY_BAG[0], propertyDescriptors)
        {
        }


        protected void Set<T>(string name, T value)
        {
            if (!HasProperty(name))
                throw new ArgumentException("Unknown property name " + name, "name");

            SettingsValue current;
            if (Values.TryGetValue(name, out current))
            {
                current.Value = value;
            }
            else
            {
                Values[name] = new SettingsValue { Value = value };
            }

            m_dirty.Add(name);
        }

        public IEnumerable<PROPERTY_BAG> GetDirtyRecords()
        {
            var dirty = Values.Where(x => m_dirty.Contains(x.Key)).ToList();
            foreach (var x in dirty)
            {
                var serializedValue = SerializeValue(x.Key, x.Value.Value);
                if (x.Value.Record == null)
                    x.Value.Record = CreateRecord(x.Key, serializedValue);
                else
                    x.Value.Record.PROPERTY_VALUE = serializedValue;
            }

            return dirty.Select(x => x.Value.Record);
        }

        private PROPERTY_BAG CreateRecord(string name, string serializedValue)
        {
            return new PROPERTY_BAG
                {
                    PROPERTY_BAG_SKEY = IdentityGenerator.GetNext(),
                    PROPERTY_NAME = name,
                    PROPERTY_TYPE = GetPropertyTypeName(name),
                    PROPERTY_VALUE = serializedValue,
                };
        }
    }
}