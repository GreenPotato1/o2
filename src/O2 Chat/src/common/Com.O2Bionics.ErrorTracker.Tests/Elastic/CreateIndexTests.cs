using System;
using System.Linq;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Network;
using Nest;
using NUnit.Framework;
using pair = System.Collections.Generic.KeyValuePair<string, string>;

namespace Com.O2Bionics.ErrorTracker.Tests.Elastic
{
    [TestFixture]
    public sealed class CreateIndexTests : BaseElasticTest
    {
        [Test]
        public void ValidateMapping()
        {
            var client = GetClient();
            var response = client.GetMapping<ErrorInfo>(IndexName);
            Assert.IsTrue(null != response && response.IsValid && null != response.Indices, "response.IsValid");
            Assert.AreEqual(1, response.Indices.Count, "response.Indices.Count");

            var index = response.Indices.Values.First();
            var mappings = index.Mappings;
            Assert.NotNull(mappings.Keys, "mappings.Keys");

            var keys = mappings.Keys.Select(k => k.Name).ToList();
            Assert.Contains(FieldConstants.PreferredTypeName, keys, "keys");
            var properties = mappings.Values.First().Properties;

            var expected = new[]
                {
                    new pair(ErrorInfo.TimeZoneOffsetPropertyName, "integer"),
                    new pair(ErrorInfo.ApplicationPropertyName, "keyword"),
                    new pair(ErrorInfo.TimeZonePropertyName, "text"),
                    new pair(ServiceConstants.CustomerId, "long"),
                    new pair(ErrorInfo.TimestampPropertyName, "date")
                };
            foreach (var p in expected)
            {
                var value = GetValue(properties, p.Key);
                Assert.AreEqual(p.Value, value, p.Key);
            }
        }

        private static string GetValue(IProperties properties, string key)
        {
            if (properties.TryGetValue(key, out var value) && null != value)
            {
                var result = value.Type;
                if (!string.IsNullOrEmpty(result))
                    return result;
            }

            throw new Exception(
                $@"The property '{key}' is not found in the dictionary.
The index might have not been created.");
        }
    }
}