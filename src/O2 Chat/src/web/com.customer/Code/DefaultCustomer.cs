using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.JsonSettings;
using Com.Customer.Web.Code;

namespace Com.Customer.Web
{
    public static class DefaultCustomer
    {
        public static decimal Id
        {
            get
            {
                var settings = new JsonSettingsReader().ReadFromFile<TestCustomerSiteSettings>();
                var client = new TcpServiceClient<IManagementService>(settings.ChatServiceClient.Host, settings.ChatServiceClient.Port);
                return client.Call(s => s.GetCustomerIds()).First();
            }
        }
    }
}