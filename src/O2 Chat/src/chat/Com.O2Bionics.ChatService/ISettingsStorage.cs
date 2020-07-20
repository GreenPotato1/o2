using Com.O2Bionics.ChatService.Settings;

namespace Com.O2Bionics.ChatService
{
    public interface ISettingsStorage
    {
        void Load(IDataContext dc);

        ServiceSettings GetServiceSettings();
        WritableServiceSettings GetWritableServiceSettings();
        void SaveServiceSettings(IDataContext dc, WritableServiceSettings settings);

        CustomerSettings GetCustomerSettings(uint customerId);
        WritableCustomerSettings GetWritableCustomerSettings(uint customerId);
        void SaveCustomerSettings(IDataContext dc, uint customerId, WritableCustomerSettings settings);

        AgentSettings GetAgentSettings(IDataContext dc, uint agentId);
        WritableAgentSettings GetWritableAgentSettings(IDataContext dc, uint agentId);
        void SaveAgentSettings(IDataContext dc, uint agentId, WritableAgentSettings settings);
    }
}