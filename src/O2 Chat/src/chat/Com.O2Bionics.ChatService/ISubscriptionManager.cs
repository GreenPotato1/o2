using Com.O2Bionics.ChatService.Contract;

namespace Com.O2Bionics.ChatService
{
    public interface ISubscriptionManager
    {
        ISubscriberCollection<IAgentConsoleEventReceiver> AgentEventSubscribers { get; }
        ISubscriberCollection<IVisitorChatEventReceiver> VisitorEventSubscribers { get; }

        void Load(IDataContext dc);
    }
}