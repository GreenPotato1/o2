using System;
using System.Collections.Generic;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Objects;
using Com.O2Bionics.ChatService.Objects.ChatEvents;

namespace Com.O2Bionics.ChatService.Impl
{
    public class ChatSessionManager : IChatSessionManager
    {
        private readonly ISubscriptionManager m_subscriptionManager;

        private readonly IChatSessionStorage m_chatSessionStorage;
        private readonly IVisitorStorage m_visitorStorage;
        private readonly IObjectResolver m_objectResolver;

        public ChatSessionManager(
            IChatSessionStorage chatSessionStorage,
            IVisitorStorage visitorStorage,
            IObjectResolver objectResolver,
            ISubscriptionManager subscriptionManager)
        {
            m_chatSessionStorage = chatSessionStorage;
            m_subscriptionManager = subscriptionManager;
            m_visitorStorage = visitorStorage;
            m_objectResolver = objectResolver;
        }

        public ChatSession GetVisitorOnlineSession(uint customerId, ulong visitorId)
        {
            return m_chatSessionStorage.GetVisitorOnlineSession(customerId, visitorId);
        }

        public List<ChatSessionMessageInfo> GetVisitorOnlineSessionMessages(uint customerId, ulong visitorId)
        {
            var session = m_chatSessionStorage.GetVisitorOnlineSession(customerId, visitorId);
            if (session == null) return new List<ChatSessionMessageInfo>();

            var previousSessionEnd = session.Events.FindLast(x => x.IsEndSessionEvent);
            var previousSessionEndId = previousSessionEnd?.Id ?? -1;
            var messages = session.Messages
                .Where(x => x.EventId > previousSessionEndId)
                .Where(x => !x.IsToAgentsOnly)
                .Select(x => x.AsInfo())
                .ToList();

            return messages;
        }

        public bool HasActiveOnlineSession(List<ChatSessionMessageInfo> sessionMessages)
        {
            return sessionMessages != null && sessionMessages.Any();
        }


        public ChatSession GetSession(uint customerId, long sessionSkey)
        {
            return m_chatSessionStorage.Get(customerId, sessionSkey);
        }

        public FullChatSessionInfo GetFullSessionInfo(uint customerId, long sessionSkey)
        {
            // TODO: check agent access

            var session = m_chatSessionStorage.Get(customerId, sessionSkey);
            return session?.AsFullInfo();
        }

        public List<ChatSessionInfo> GetAgentVisibleSessions(uint customerId, uint agentId, HashSet<uint> departmentIds)
        {
            var sessions = m_chatSessionStorage.GetAgentVisibleSessions(customerId, agentId, departmentIds);
            return sessions
                .Select(x => x.AsInfo())
                .ToList();
        }

        public List<ChatSessionInfo> GetAgentSessions(uint customerId, uint agentId)
        {
            return m_chatSessionStorage.GetAgentSessions(customerId, agentId)
                .Select(x => x.AsInfo())
                .ToList();
        }

        public void CreateNewSession(uint customerId, ChatEventBase chatEvent, bool isOffline = false, ulong? visitorId = null)
        {
            if (visitorId.HasValue)
            {
                var visitor = m_visitorStorage.Get(visitorId.Value);
                if (visitor == null)
                    throw new InvalidOperationException("Visitor not found guid=" + visitorId);
            }

            var session = m_chatSessionStorage.CreateNew(customerId, chatEvent, isOffline, visitorId);
            chatEvent.Notify(session, m_objectResolver, m_subscriptionManager);
        }

        public void AddEvent(uint customerId, long chatSessionSkey, ChatEventBase chatEvent)
        {
            var session = m_chatSessionStorage.AddEvent(customerId, chatSessionSkey, chatEvent);
            chatEvent.Notify(session, m_objectResolver, m_subscriptionManager);
        }

        public void AddVisitorOnlineEvent(ulong visitorId, ChatEventBase chatEvent)
        {
            var visitor = m_visitorStorage.Get(visitorId);
            if (visitor == null)
                throw new InvalidOperationException("Visitor not found id=" + visitorId);

            var session = m_chatSessionStorage.AddVisitorOnlineEvent(visitor.CustomerId, visitorId, chatEvent);
            chatEvent.Notify(session, m_objectResolver, m_subscriptionManager);
        }
    }
}