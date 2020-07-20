using Com.O2Bionics.ChatService.Contract.Settings;
using Com.O2Bionics.Utils.JsonSettings;

namespace Com.Customer.Web.Code
{
    [SettingsRoot("testCustomerSite")]
    public class TestCustomerSiteSettings
    {
        [SettingsRoot("chatServiceClient")]
        public ChatServiceClientSettings ChatServiceClient { get; set; }
    }
}