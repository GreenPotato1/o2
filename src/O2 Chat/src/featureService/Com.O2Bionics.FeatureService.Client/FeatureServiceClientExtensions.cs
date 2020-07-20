using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Com.O2Bionics.FeatureService.Constants;

namespace Com.O2Bionics.FeatureService.Client
{
    public static class FeatureServiceClientExtensions
    {
        public static async Task<string> GetString(this IFeatureServiceClient client, uint customerId, string featureCode)
        {
            var dictionary = await client.GetValue(customerId, new List<string> { featureCode }).ConfigureAwait(false);
            Debug.Assert(null != dictionary);
            return dictionary.Values.FirstOrDefault();
        }

        public static async Task<int> GetInt32(this IFeatureServiceClient client, uint customerId, string featureCode, int defaultValue = 0)
        {
            var value = await GetString(client, customerId, featureCode).ConfigureAwait(false);
            return !int.TryParse(value, out var result) ? defaultValue : result;
        }

        public static async Task<Dictionary<string, int>> GetInt32(
            this IFeatureServiceClient client,
            uint customerId,
            IEnumerable<string> featureCodes)
        {
            var dictionary = await client.GetValue(customerId, featureCodes.ToList()).ConfigureAwait(false);
            Debug.Assert(null != dictionary);
            return dictionary.ToDictionary(x => x.Key, x => !int.TryParse(x.Value, out var v) ? 0 : v);
        }

        public static async Task<bool> GetBool(this IFeatureServiceClient client, uint customerId, string featureCode, bool defaultValue = false)
        {
            var value = await GetString(client, customerId, featureCode).ConfigureAwait(false);
            return string.IsNullOrEmpty(value) ? defaultValue : value == FeatureValues.True;
        }

        public static async Task<Dictionary<string, bool>> GetBool(
            this IFeatureServiceClient client,
            uint customerId,
            IEnumerable<string> featureCodes)
        {
            var dictionary = await client.GetValue(customerId, featureCodes.ToList()).ConfigureAwait(false);
            Debug.Assert(null != dictionary);
            return dictionary.ToDictionary(x => x.Key, x => x.Value == FeatureValues.True);
        }

        public static async Task<HashSet<string>> GetBoolSet(
            this IFeatureServiceClient client,
            uint customerId,
            IEnumerable<string> featureCodes)
        {
            var features = await client.GetBool(customerId, featureCodes).ConfigureAwait(false);
            Debug.Assert(null != features);
            return new HashSet<string>(features.Where(x => x.Value).Select(x => x.Key));
        }
    }
}