using System.Collections.Generic;

namespace Com.O2Bionics.FeatureService.Impl
{
    public interface IFeaturesManager
    {
        Dictionary<string, string> GetFeatureValue(
            string productCode,
            int userId,
            HashSet<string> featureCodes,
            bool? ignoreCache = null,
            int? timeToLiveSeconds = null,
            bool? logProcessing = null,
            bool? logSqlQuery = null);

        void ResetCache();

        Dictionary<string, string> Ping();
    }
}