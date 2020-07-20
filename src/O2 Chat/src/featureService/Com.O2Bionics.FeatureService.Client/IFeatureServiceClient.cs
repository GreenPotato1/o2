using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Com.O2Bionics.FeatureService.Client
{
    public interface IFeatureServiceClient : IDisposable
    {
        [ItemNotNull]
        Task<Dictionary<string, string>> GetValue(uint userId, [NotNull] List<string> featureCodes);
    }
}