using System.Threading.Tasks;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.FeatureService.Constants;
using Com.O2Bionics.Utils;

namespace Com.O2Bionics.ChatService.Web.Console
{
    public static class FeatureServiceHelper
    {
        public static async Task<int> FetchVisibleDays(bool isLogin, uint customerId)
        {
            var featureClient = GlobalContainer.Resolve<IFeatureServiceClient>();
            var featureName = isLogin ? FeatureCodes.LoginVisibleDays : FeatureCodes.AuditVisibleDays;

            return await featureClient.GetInt32(customerId, featureName).ConfigureAwait(false);
        }

        public static (int, int) AuditAndLoginVisibleDays(uint customerId)
        {
            var featureClient = GlobalContainer.Resolve<IFeatureServiceClient>();

            var d = featureClient.GetInt32(customerId, new[] { FeatureCodes.AuditVisibleDays, FeatureCodes.LoginVisibleDays })
                .WaitAndUnwrapException();
            return(
                d.TryGetValue(FeatureCodes.AuditVisibleDays, out var avd) ? avd : 0,
                d.TryGetValue(FeatureCodes.LoginVisibleDays, out var lvd) ? lvd : 0
                );
        }
    }
}