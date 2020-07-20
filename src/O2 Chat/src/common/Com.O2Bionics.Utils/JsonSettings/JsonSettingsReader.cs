using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Com.O2Bionics.Utils.JsonSettings
{
    public class JsonSettingsReader
    {
        private const string FilePathAppSettingsName = "ConfigFilePath";

        private static string GetAppSettingConfigFilePath()
        {
            var path = ConfigurationManager.AppSettings[FilePathAppSettingsName];
            if (string.IsNullOrWhiteSpace(path))
                throw new ConfigurationErrorsException($"AppSettings[{FilePathAppSettingsName}] is empty or not defined");
            return path;
        }

        public string ConfigFilePath => GetAppSettingConfigFilePath();

        public T ReadFromFile<T>(string fileName = null)
        {
            fileName = fileName ?? GetAppSettingConfigFilePath();

            string json;
            try
            {
                json = File.ReadAllText(fileName);
            }
            catch (IOException e)
            {
                throw new JsonSettingsErrorsException("Can't read file", fileName, null, null, e);
            }

            return ReadFromString<T>(json, fileName);
        }

        public T ReadFromString<T>(string json, string fileName = null)
        {
            var type = typeof(T);

            JObject parsed;
            try
            {
                parsed = JObject.Parse(json);
            }
            catch (Exception e)
            {
                throw new JsonSettingsErrorsException("Can't parse json", fileName, json, null, e);
            }


            var rootName = type.GetCustomAttribute<SettingsRootAttribute>()?.Name ?? CamelCase(type.Name);
            var root = parsed[rootName];
            if (root == null)
                throw new JsonSettingsErrorsException($"Can't find root entry '{rootName}'", fileName, json, null, null);

            var errors = new List<string>();
            var result = (T)ReadObject(parsed, root, typeof(T), errors);

            if (errors.Count > 0)
                throw new JsonSettingsErrorsException("Problems were detected when reading settings", fileName, json, errors, null);

            return result;
        }

        private object ReadObject(JToken root, JToken entry, Type type, List<string> errors)
        {
            var instance = Activator.CreateInstance(type);

            foreach (var prop in type.GetProperties())
            {
                var propType = prop.PropertyType;

                var setterMethod = prop.GetSetMethod(true);
                if (setterMethod == null) continue;

                var jsonPropName = CamelCase(prop.Name);

                var sra = prop.GetCustomAttribute<SettingsRootAttribute>();
                var jsonEntry = sra != null ? root[sra.Name] : entry?[jsonPropName];

                if (sra != null && jsonEntry == null)
                    errors.Add($"{type.Name}.{prop.Name} uses [SettingsRoot('{sra.Name}')] but the corresponding root level entry was not found");

                object value;

                if (jsonEntry == null)
                {
                    value = null;
                }
                else if (IsSettingsClass(propType))
                {
                    value = ReadObject(root, jsonEntry, propType, errors);
                }
                else
                {
                    var propertyReader = GetPropertyReader(propType);
                    if (propertyReader == null)
                    {
                        errors.Add($"No readers for property {type.Name}.{prop.Name} of type {propType}");
                        continue;
                    }

                    value = propertyReader(this, jsonEntry, errors);
                }

                value = value ?? GetDefaultValue(type, prop, errors);

                Validate(type, prop, value, errors);

                if (value == null && propType.IsValueType && !IsRequired(prop))
                {
                    errors.Add($"{type.Name}.{prop.Name} is of a value type but value is not provided nor default value is specified");
                    continue;
                }

                setterMethod.Invoke(instance, new[] { value });
            }

            return instance;
        }

        private static void Validate(Type type, PropertyInfo prop, object value, List<string> errors)
        {
            foreach (var validator in prop.GetCustomAttributes<ValidationAttribute>())
            {
                var errs = validator.Validate(value);
                errors.AddRange(errs.Select(x => $"{type.Name}.{prop.Name} {x}"));
            }
        }

        private static object GetDefaultValue(Type type, PropertyInfo prop, List<string> errors)
        {
            var v = prop.GetCustomAttribute<DefaultAttribute>()?.Value;
            if (v == null) return null;
            if (v is string s)
            {
                if (prop.PropertyType == typeof(TimeSpan))
                {
                    if (TimeSpan.TryParse(s, out var ts)) return ts;
                    errors.Add($"Can't parse {type.Name}.{prop.Name} default value {v} as a TimeSpan");
                    return null;
                }

                if (prop.PropertyType == typeof(Uri))
                {
                    if (Uri.TryCreate(s, UriKind.Absolute, out var uri)) return uri;
                    errors.Add($"Can't parse {type.Name}.{prop.Name} default value {v} as an Uri");
                    return null;
                }
            }

            return v;
        }

        private static Func<JsonSettingsReader, JToken, List<string>, object> GetPropertyReader(Type type)
        {
            return m_readers
                .Where(x => type.IsAssignableFrom(x.Key))
                .Select(x => x.Value)
                .FirstOrDefault();
        }

        #region readers

        private static readonly Dictionary<Type, Func<JsonSettingsReader, JToken, List<string>, object>> m_readers
            = new Dictionary<Type, Func<JsonSettingsReader, JToken, List<string>, object>>
                {
                    { typeof(string), GetString },
                    { typeof(int), GetInt32 },
                    { typeof(bool), GetBool },
                    { typeof(TimeSpan), GetTimeSpan },
                    { typeof(Uri), GetUri },
                    { typeof(IReadOnlyCollection<Uri>), GetUriList },
                    { typeof(IReadOnlyCollection<int>), GetIntList },
                    { typeof(IReadOnlyCollection<string>), GetStringList },
                    { typeof(IReadOnlyDictionary<string, string>), GetStringDictionary },
                    { typeof(EsConnectionSettings), GetEsConnectionSettings },
                    { typeof(EsIndexSettings), GetEsIndexSettings },
                };

        private static object GetString(JsonSettingsReader self, JToken tok, List<string> errors)
        {
            return tok?.ToString();
        }

        private static object GetInt32(JsonSettingsReader self, JToken tok, List<string> errors)
        {
            if (tok.Type == JTokenType.Integer) return tok.ToObject<int>();
            errors.Add($"[{tok.Path}] value '{tok}' is not an integer value");
            return default(int);
        }

        private static object GetBool(JsonSettingsReader self, JToken tok, List<string> errors)
        {
            if (tok.Type == JTokenType.Boolean) return tok.Value<bool>();
            errors.Add($"[{tok.Path}] value '{tok}' is not a boolean value");
            return default(bool);
        }

        private static object GetTimeSpan(JsonSettingsReader self, JToken tok, List<string> errors)
        {
            if (tok.Type == JTokenType.String)
            {
                if (TimeSpan.TryParse(tok.Value<string>(), out var v)) return v;
            }

            errors.Add($"[{tok.Path}] value '{tok}' is not a TimeSpan value");
            return default(TimeSpan);
        }

        private static object GetUri(JsonSettingsReader self, JToken tok, List<string> errors)
        {
            if (tok.Type == JTokenType.String)
            {
                if (Uri.TryCreate(tok.Value<string>(), UriKind.Absolute, out var v)) return v;
            }

            errors.Add($"[{tok.Path}] value '{tok}' is not an Uri value");
            return null;
        }

        private static object GetUriList(JsonSettingsReader self, JToken tok, List<string> errors)
        {
            if (tok.Type == JTokenType.Array && ((JArray)tok).All(x => x.Type == JTokenType.String))
            {
                return ((JArray)tok)
                    .SelectMany(x => StringToMany(x.Value<string>()))
                    .Select(x => (Uri)GetUri(self, x, errors))
                    .ToList()
                    .AsReadOnly();
            }

            errors.Add($"[{tok.Path}] value '{tok}' is not an Uri array");
            return null;
        }


        /// <summary>
        /// "asd[1,2,3,4]qwe" -> { "asd1qwe", "asd2qwe", "asd3qwe", "asd4qwe", }
        /// 
        /// "asdqwe" -> { "asdqwe" } 
        /// 
        /// "asd[asd" -> fail
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static IEnumerable<string> StringToMany(string s)
        {
            s.NotNullOrWhitespace(nameof(s));

            var startIndex = s.IndexOf('[');
            if (startIndex < 0)
            {
                yield return s;
                yield break;
            }

            var endIndex = s.IndexOf(']', startIndex);
            if (endIndex < 0)
            {
                throw new Exception($"Invalid format: ] is expected after [ in '{s}'");
            }

            var start = s.Substring(0, startIndex);
            var end = s.Substring(endIndex + 1);
            var parts = s.Substring(startIndex + 1, endIndex - startIndex - 1)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
                yield return start + part + end;
        }

        private static object GetIntList(JsonSettingsReader self, JToken tok, List<string> errors)
        {
            if (tok.Type == JTokenType.Array)
            {
                return ((JArray)tok).Select(x => (Uri)GetInt32(self, x, errors)).ToList().AsReadOnly();
            }

            errors.Add($"[{tok.Path}] value '{tok}' is not an Uri array");
            return null;
        }

        private static object GetStringList(JsonSettingsReader self, JToken tok, List<string> errors)
        {
            if (tok.Type == JTokenType.Array)
            {
                return ((JArray)tok).Select(x => GetString(self, x, errors)).Cast<string>().ToList().AsReadOnly();
            }

            errors.Add($"[{tok.Path}] value '{tok}' is not an Uri array");
            return null;
        }

        private static object GetStringDictionary(JsonSettingsReader self, JToken tok, List<string> errors)
        {
            if (tok.Type == JTokenType.Object)
            {
                var props = ((JObject)tok).Children<JProperty>();
                if (props.All(x => x.Value.Type == JTokenType.String || x.Value.Type == JTokenType.Null))
                {
                    return new ReadOnlyDictionary<string, string>(props.ToDictionary(x => x.Name, x => x.Value.Value<string>()));
                }
            }

            errors.Add($"[{tok.Path}] value '{tok}' is not a string map");
            return null;
        }

        private static object GetEsConnectionSettings(JsonSettingsReader self, JToken tok, List<string> errors)
        {
            if (tok.Type == JTokenType.Object)
            {
                return self.ReadObject(null, tok, typeof(EsConnectionSettings), errors);
            }

            errors.Add($"[{tok.Path}] value '{tok}' is not a object");
            return null;
        }

        private static object GetEsIndexSettings(JsonSettingsReader self, JToken tok, List<string> errors)
        {
            if (tok.Type == JTokenType.Object)
            {
                return self.ReadObject(null, tok, typeof(EsIndexSettings), errors);
            }

            errors.Add($"[{tok.Path}] value '{tok}' is not a object");
            return null;
        }

        #endregion

        private static bool IsSettingsClass(Type propertyType)
        {
            return propertyType.GetCustomAttribute<SettingsClassAttribute>() != null;
        }

        private static bool IsRequired(PropertyInfo prop)
        {
            return prop.GetCustomAttribute<RequiredAttribute>() != null;
        }

        private static string CamelCase(string s)
        {
            return char.ToLowerInvariant(s[0]) + s.Substring(1);
        }
    }
}