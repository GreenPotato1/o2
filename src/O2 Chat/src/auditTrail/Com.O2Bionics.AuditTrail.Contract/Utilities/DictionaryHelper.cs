using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using pair = Com.O2Bionics.AuditTrail.Contract.IdName<string>;

namespace Com.O2Bionics.AuditTrail.Contract.Utilities
{
    public static class DictionaryHelper
    {
        [CanBeNull]
        public static Dictionary<string, List<pair>> ToListOfPairs([CanBeNull] this Dictionary<string, Dictionary<string, string>> value)
        {
            if (null == value || 0 == value.Count)
                return null;

            var result = new Dictionary<string, List<pair>>();
            foreach (var keyValuePair in value)
            {
                if (null != keyValuePair.Value && 0 < keyValuePair.Value.Count)
                {
                    var values = new List<pair>(keyValuePair.Value.Count);
                    foreach (var p2 in keyValuePair.Value)
                    {
#if DEBUG
                        if (string.IsNullOrEmpty(p2.Key))
                            throw new Exception("The list of values in a dictionary must have not empty Key.");
                        if (string.IsNullOrEmpty(p2.Value))
                            throw new Exception("The list of values in a dictionary must have not empty Value.");
#endif
                        values.Add(new pair(p2.Key, p2.Value));
                    }

                    result[keyValuePair.Key] = values;
                }
#if DEBUG
                else
                    throw new Exception("The list of values in a dictionary must not be empty.");
#endif
            }

            return result;
        }

        [CanBeNull]
        public static Dictionary<string, Dictionary<string, string>> FromListOfPairs([CanBeNull] this Dictionary<string, List<pair>> value)
        {
            if (null == value || 0 == value.Count)
                return null;

            var result = new Dictionary<string, Dictionary<string, string>>();

            foreach (var keyValuePair in value)
            {
                if (null != keyValuePair.Value && 0 < keyValuePair.Value.Count)
                {
                    var values = new Dictionary<string, string>(keyValuePair.Value.Count);
                    foreach (var p2 in keyValuePair.Value)
                    {
#if DEBUG
                        if (string.IsNullOrEmpty(p2.Id))
                            throw new Exception("The list of values must have not empty Id.");

                        if (string.IsNullOrEmpty(p2.Name))
                            throw new Exception("The list of values must have not empty Name.");
#endif
                        values[p2.Id] = p2.Name;
                    }

                    result[keyValuePair.Key] = values;
                }
#if DEBUG
                else
                    throw new Exception("The list of values must not be empty.");
#endif
            }

            return result;
        }
    }
}