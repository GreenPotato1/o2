using System;

namespace Com.O2Bionics.FeatureService.Impl
{
    public interface IFeatureServiceCacheManager : IDisposable
    {
        string GetKey(string productCode, int userId, string featureCode);
        string GetValue(string cacheKey);
        void SetValue(string cacheKey, string value, int? timeToLiveSeconds);
        void ResetCache();
    }
}