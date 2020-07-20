using Com.O2Bionics.ChatService.Contract;

namespace Com.O2Bionics.ChatService.Impl
{
    public class SubscriptionManager : ISubscriptionManager
    {
        public SubscriptionManager(ISettingsStorage settingsStorage)
        {
            AgentEventSubscribers = new SubscriberCollection<IAgentConsoleEventReceiver>(
                settingsStorage,
                s => s.AgentEventSubscriptions,
                (s, v) => s.AgentEventSubscriptions = v);
            VisitorEventSubscribers = new SubscriberCollection<IVisitorChatEventReceiver>(
                settingsStorage,
                s => s.VisitorEventSubscriptions,
                (s, v) => s.VisitorEventSubscriptions = v);
        }

        public SubscriptionManager()
        {
        }

        public void Load(IDataContext dc)
        {
            AgentEventSubscribers.Load(dc);
            VisitorEventSubscribers.Load(dc);
        }


        public ISubscriberCollection<IVisitorChatEventReceiver> VisitorEventSubscribers { get; private set; }
        public ISubscriberCollection<IAgentConsoleEventReceiver> AgentEventSubscribers { get; private set; }
    }
}