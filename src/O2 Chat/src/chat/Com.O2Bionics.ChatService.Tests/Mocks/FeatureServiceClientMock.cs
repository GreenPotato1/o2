using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Com.O2Bionics.FeatureService.Client;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService.Tests.Mocks
{
    using TFunc = Func<uint, List<string>, Dictionary<string, string>>;

    public sealed class FeatureServiceClientMock : IFeatureServiceClient
    {
        private readonly TFunc m_func;

        public FeatureServiceClientMock([NotNull] TFunc func)
        {
            m_func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public FeatureServiceClientMock([CanBeNull] Dictionary<string, string> value)
        {
            m_func = (i, list) => value;
        }

        public void Dispose()
        {
        }

        public Task<Dictionary<string, string>> GetValue(
            uint userId,
            List<string> featureCodes)
        {
            var result = m_func(userId, featureCodes);
            return Task.FromResult(result);
        }
    }
}