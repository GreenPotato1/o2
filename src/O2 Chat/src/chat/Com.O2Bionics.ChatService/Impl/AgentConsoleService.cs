using System;
using System.Linq;
using System.ServiceModel;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Objects.ChatEvents;
using Com.O2Bionics.MailerService.Client;
using Com.O2Bionics.Utils;

namespace Com.O2Bionics.ChatService.Impl
{
    [ServiceBehavior(
        ConcurrencyMode = ConcurrencyMode.Multiple,
        InstanceContextMode = InstanceContextMode.Single,
        AddressFilterMode = AddressFilterMode.Any)]
    public class AgentConsoleService : ChatServiceBase, IAgentConsoleService
    {
        private readonly ISubscriptionManager m_subscriptionManager;
        private readonly IAgentManager m_agentManager;
        private readonly IChatSessionManager m_chatSessionManager;
        private readonly IVisitorStorage m_visitorStorage;
        private readonly ISettingsStorage m_settingsStorage;
        private readonly IChatDatabaseFactory m_databaseFactory;
        private readonly INowProvider m_nowProvider;
        private readonly IMailerServiceClient m_emailSender;
        private readonly IUserManager m_userManager;

        public AgentConsoleService()
        {
        }

        public AgentConsoleService(
            INowProvider nowProvider,
            IMailerServiceClient emailSender,
            IChatDatabaseFactory databaseFactory,
            IVisitorStorage visitorStorage,
            ISettingsStorage settingsStorage,
            IAgentManager agentManager,
            IChatSessionManager chatSessionManager,
            ISubscriptionManager subscriptionManager,
            IUserManager userManager)
        {
            m_nowProvider = nowProvider;
            m_databaseFactory = databaseFactory;
            m_visitorStorage = visitorStorage;
            m_settingsStorage = settingsStorage;
            m_agentManager = agentManager;
            m_chatSessionManager = chatSessionManager;
            m_subscriptionManager = subscriptionManager;
            m_userManager = userManager;
            m_emailSender = emailSender;
        }

        public void Subscribe(string endpoint)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { endpoint, });

                        using (var dc = m_databaseFactory.CreateContext())
                        {
                            m_subscriptionManager.AgentEventSubscribers.Add(dc, new Subscriber(endpoint));
                            dc.Commit();
                        }
                    });
        }

        public void Unsubscribe(string endpoint)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { endpoint, });

                        m_agentManager.DisconnectAll();

                        using (var dc = m_databaseFactory.CreateContext())
                        {
                            m_subscriptionManager.AgentEventSubscribers.Remove(dc, new Subscriber(endpoint));
                            dc.Commit();
                        }
                    });
        }

        public AgentSessionConnectResult Connected(
            uint customerId,
            Guid agentSessionGuid,
            uint agentId,
            string connectionId)
        {
            return HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(
                                new
                                    {
                                        customerId,
                                        agentSessionGuid,
                                        agentId,
                                        connectionId,
                                    });

                        return m_databaseFactory.Query(
                            db => m_agentManager.Connect(db, agentSessionGuid, customerId, agentId, connectionId));
                    });
        }

        public void Disconnected(
            uint customerId,
            Guid agentSessionGuid,
            uint agentId,
            string connectionId)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(
                                new
                                    {
                                        customerId,
                                        agentSessionGuid,
                                        agentId,
                                        connectionId,
                                    });

                        m_agentManager.Disconnect(customerId, agentSessionGuid, agentId, connectionId);
                    });
        }

        public void SessionSetStatus(
            uint customerId,
            Guid agentSessionGuid,
            bool isOnline)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(
                                new
                                    {
                                        customerId,
                                        agentSessionGuid,
                                        isOnline,
                                    });

                        m_databaseFactory.Query(
                            db => m_agentManager.SetUserOnlineStatus(db, customerId, agentSessionGuid, isOnline));
                    });
        }


        public AgentConsoleInfo GetConsoleInfo(
            uint customerId,
            Guid agentSessionGuid)
        {
            return HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(
                                new
                                    {
                                        customerId,
                                        agentSessionGuid,
                                    });

                        using (var c = m_databaseFactory.CreateContext())
                        {
                            var agentSession = m_agentManager.GetAgentSession(agentSessionGuid);
                            var agentDepartments = m_agentManager.GetAgentDepartmentIds(c.Db, customerId, agentSession.AgentId);

                            var users = m_userManager.GetUsers(c.Db, customerId);
                            var onlineAgents = m_agentManager.GetCustomerOnlineAgents(customerId);
                            var departments = m_agentManager.GetCustomerDepartmentsInfo(c.Db, customerId, false);
                            var onlineDepartments = m_agentManager.GetCustomerOnlineDepartments(customerId);

                            // new visitor sessions to agent departments online and offline
                            // new agent sessions to agent
                            // new agent sessions to agent department
                            // active sessions where agent is participating
                            var sessions = m_chatSessionManager.GetAgentVisibleSessions(customerId, agentSession.AgentId, agentDepartments);

                            var visitors = sessions
                                .Where(x => x.VisitorId.HasValue)
                                .Select(x => x.VisitorId.Value)
                                .Distinct()
                                .Select(x => m_visitorStorage.Get(x))
                                .Where(x => x != null)
                                .Select(x => x.AsInfo())
                                .ToList();

                            var agentSettings = m_settingsStorage.GetAgentSettings(c, agentSession.AgentId);

                            var settings = new AgentConsoleSettingsInfo
                                {
                                    MediaCallProposalTimeoutMs = (int)agentSettings.MediaCallProposalTimeout.TotalMilliseconds,
                                    MediaCallConnectTimeoutMs = (int)agentSettings.MediaCallConnectTimeout.TotalMilliseconds,
                                };

                            return new AgentConsoleInfo
                                {
                                    AgentId = agentSession.AgentId,
                                    AgentDepartments = agentDepartments.ToList(),
                                    Users = users,
                                    OnlineAgents = onlineAgents.ToList(),
                                    Departments = departments,
                                    OnlineDepartments = onlineDepartments.ToList(),
                                    Sessions = sessions,
                                    Visitors = visitors,
                                    Settings = settings,
                                    CustomerId = agentSession.CustomerId
                                };
                        }
                    });
        }

        public FullChatSessionInfo GetFullChatSessionInfo(
            uint customerId,
            Guid agentSessionGuid,
            long chatSessionSkey)
        {
            return HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(
                                new
                                    {
                                        customerId,
                                        agentSessionGuid,
                                        chatSessionSkey,
                                    });

                        return m_chatSessionManager.GetFullSessionInfo(customerId, chatSessionSkey);
                    });
        }

        public VisitorInfo GetVisitorInfo(
            uint customerId,
            Guid agentSessionGuid,
            ulong visitorId)
        {
            return HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(
                                new
                                    {
                                        customerId,
                                        agentSessionGuid,
                                        visitorId,
                                    });

                        var visitor = m_visitorStorage.Get(visitorId);
                        if (visitor == null)
                            throw new InvalidOperationException("Visitor not found vid=" + visitorId);

                        return visitor.AsInfo();
                    });
        }

        public void StartChatSessionToAgent(
            uint customerId,
            Guid agentSessionGuid,
            uint targetAgentId,
            string messageText)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(
                                new
                                    {
                                        agentSessionGuid,
                                        targetAgentId,
                                        messageText,
                                    });

                        var agentSession = m_agentManager.GetAgentSession(agentSessionGuid);
                        if (agentSession == null)
                            throw new InvalidOperationException("Agent session not found asid=" + agentSessionGuid);

                        var chatEvent = new AgentCreatesSessionToAgentChatEvent(
                            m_nowProvider.UtcNow,
                            messageText,
                            agentSession.AgentId,
                            targetAgentId);
                        m_chatSessionManager.CreateNewSession(agentSession.CustomerId, chatEvent);
                    });
        }

        public void StartChatSessionToDepartment(
            uint customerId,
            Guid agentSessionGuid,
            uint targetDepartmentId,
            string messageText)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(
                                new
                                    {
                                        customerId,
                                        agentSessionGuid,
                                        targetDepartmentId,
                                        messageText,
                                    });

                        var agentSession = m_agentManager.GetAgentSession(agentSessionGuid);
                        if (agentSession == null)
                            throw new InvalidOperationException("Agent session not found asid=" + agentSessionGuid);

                        var chatEvent = new AgentCreatesSessionToDepartmentChatEvent(
                            m_nowProvider.UtcNow,
                            messageText,
                            agentSession.AgentId,
                            targetDepartmentId);
                        m_chatSessionManager.CreateNewSession(agentSession.CustomerId, chatEvent);
                    });
        }

        public void InviteAgentToChatSession(
            uint customerId,
            Guid agentSessionGuid,
            long chatSessionSkey,
            uint invitedAgentId,
            bool actOnBehalfOfInvitor,
            string messageText)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(
                                new
                                    {
                                        customerId,
                                        agentSessionGuid,
                                        chatSessionSkey,
                                        invitedAgentId,
                                        actOnBehalfOfInvitor,
                                        messageText,
                                    });

                        var agentSession = m_agentManager.GetAgentSession(agentSessionGuid);
                        if (agentSession == null)
                            throw new InvalidOperationException("Agent session not found asid=" + agentSessionGuid);

                        var chatEvent = new AgentInvitesAgentChatEvent(
                            m_nowProvider.UtcNow,
                            messageText,
                            agentSession.AgentId,
                            invitedAgentId,
                            actOnBehalfOfInvitor);
                        m_chatSessionManager.AddEvent(customerId, chatSessionSkey, chatEvent);
                    });
        }

        public void CancelAgentInvitationToChatSession(
            uint customerId,
            Guid agentSessionGuid,
            long chatSessionSkey,
            uint invitedAgentId,
            string messageText)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(
                                new
                                    {
                                        customerId,
                                        agentSessionGuid,
                                        chatSessionSkey,
                                        invitedAgentId,
                                        messageText,
                                    });

                        var agentSession = m_agentManager.GetAgentSession(agentSessionGuid);
                        if (agentSession == null)
                            throw new InvalidOperationException("Agent session not found asid=" + agentSessionGuid);

                        var chatEvent = new AgentCancelsInviteAgentChatEvent(
                            m_nowProvider.UtcNow,
                            messageText,
                            agentSession.AgentId,
                            invitedAgentId);
                        m_chatSessionManager.AddEvent(customerId, chatSessionSkey, chatEvent);
                    });
        }

        public void AcceptAgentChatSession(
            uint customerId,
            Guid agentSessionGuid,
            long chatSessionSkey)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(
                                new
                                    {
                                        customerId,
                                        agentSessionGuid,
                                        chatSessionSkey,
                                    });

                        var agentSession = m_agentManager.GetAgentSession(agentSessionGuid);
                        if (agentSession == null)
                            throw new InvalidOperationException("Agent session not found asid=" + agentSessionGuid);

                        var chatEvent = new AgentAcceptsAgentSessionChatEvent(
                            m_nowProvider.UtcNow,
                            agentSession.AgentId);
                        m_chatSessionManager.AddEvent(customerId, chatSessionSkey, chatEvent);
                    });
        }

        public void AcceptDepartmentChatSession(
            uint customerId,
            Guid agentSessionGuid,
            long chatSessionSkey,
            uint departmentId)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(
                                new
                                    {
                                        customerId,
                                        agentSessionGuid,
                                        chatSessionSkey,
                                        departmentId,
                                    });

                        var agentSession = m_agentManager.GetAgentSession(agentSessionGuid);
                        if (agentSession == null)
                            throw new InvalidOperationException("Agent session not found asid=" + agentSessionGuid);

                        var chatEvent = new AgentAcceptsDepartmentSessionChatEvent(
                            m_nowProvider.UtcNow,
                            agentSession.AgentId,
                            departmentId);
                        m_chatSessionManager.AddEvent(customerId, chatSessionSkey, chatEvent);
                    });
        }

        public void RejectAgentChatSession(
            uint customerId,
            Guid agentSessionGuid,
            long chatSessionSkey)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(
                                new
                                    {
                                        customerId,
                                        agentSessionGuid,
                                        chatSessionSkey,
                                    });

                        var agentSession = m_agentManager.GetAgentSession(agentSessionGuid);
                        if (agentSession == null)
                            throw new InvalidOperationException("Agent session not found asid=" + agentSessionGuid);

                        var chatEvent = new AgentRejectsAgentSessionChatEvent(
                            m_nowProvider.UtcNow,
                            agentSession.AgentId);
                        m_chatSessionManager.AddEvent(customerId, chatSessionSkey, chatEvent);
                    });
        }


        public void InviteDepartmentToChatSession(
            uint customerId,
            Guid agentSessionGuid,
            long chatSessionSkey,
            uint invitedDepartmentId,
            bool actOnBehalfOfInvitor,
            string messageText)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(
                                new
                                    {
                                        customerId,
                                        agentSessionGuid,
                                        chatSessionSkey,
                                        invitedDepartmentId,
                                        actOnBehalfOfInvitor,
                                        messageText,
                                    });

                        var agentSession = m_agentManager.GetAgentSession(agentSessionGuid);
                        if (agentSession == null)
                            throw new InvalidOperationException("Agent session not found asid=" + agentSessionGuid);

                        var chatEvent = new AgentInvitesDepartmentChatEvent(
                            m_nowProvider.UtcNow,
                            messageText,
                            agentSession.AgentId,
                            invitedDepartmentId,
                            actOnBehalfOfInvitor);
                        m_chatSessionManager.AddEvent(customerId, chatSessionSkey, chatEvent);
                    });
        }

        public void CancelDepartmentInvitationToChatSession(
            uint customerId,
            Guid agentSessionGuid,
            long chatSessionSkey,
            uint invitedDepartmentId,
            string messageText)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(
                                new
                                    {
                                        customerId,
                                        agentSessionGuid,
                                        chatSessionSkey,
                                        invitedDepartmentId,
                                        messageText,
                                    });

                        var agentSession = m_agentManager.GetAgentSession(agentSessionGuid);
                        if (agentSession == null)
                            throw new InvalidOperationException("Agent session not found asid=" + agentSessionGuid);

                        var chatEvent = new AgentCancelsInviteDeptChatEvent(
                            m_nowProvider.UtcNow,
                            messageText,
                            agentSession.AgentId,
                            invitedDepartmentId);
                        m_chatSessionManager.AddEvent(customerId, chatSessionSkey, chatEvent);
                    });
        }

        public void SendMessageToChatSession(
            uint customerId,
            Guid agentSessionGuid,
            long chatSessionSkey,
            bool isToAgentsOnly,
            string messageText)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(
                                new
                                    {
                                        customerId,
                                        agentSessionGuid,
                                        chatSessionSkey,
                                        isToAgentsOnly,
                                        messageText,
                                    });

                        var agentSession = m_agentManager.GetAgentSession(agentSessionGuid);
                        if (agentSession == null)
                            throw new InvalidOperationException("Agent session not found asid=" + agentSessionGuid);

                        var chatEvent = new AgentSendsMessageChatEvent(
                            m_nowProvider.UtcNow,
                            messageText,
                            agentSession.AgentId,
                            isToAgentsOnly);
                        m_chatSessionManager.AddEvent(customerId, chatSessionSkey, chatEvent);
                    });
        }

        public void LeaveChatSession(
            uint customerId,
            Guid agentSessionGuid,
            long chatSessionSkey,
            string messageText,
            bool isDisconnected,
            bool isBecameOffline)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(
                                new
                                    {
                                        customerId,
                                        agentSessionGuid,
                                        chatSessionSkey,
                                        messageText,
                                        isDisconnected,
                                        isBecameOffline,
                                    });

                        var agentSession = m_agentManager.GetAgentSession(agentSessionGuid);
                        if (agentSession == null)
                            throw new InvalidOperationException("Agent session not found asid=" + agentSessionGuid);

                        var chatEvent = new AgentLeavesSessionChatEvent(
                            m_nowProvider.UtcNow,
                            messageText,
                            agentSession.AgentId,
                            isDisconnected,
                            isBecameOffline);
                        m_chatSessionManager.AddEvent(customerId, chatSessionSkey, chatEvent);
                    });
        }

        public void CloseChatSession(
            uint customerId,
            Guid agentSessionGuid,
            long chatSessionSkey,
            string text)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(
                                new
                                    {
                                        customerId,
                                        agentSessionGuid,
                                        chatSessionSkey,
                                        text,
                                    });

                        var agentSession = m_agentManager.GetAgentSession(agentSessionGuid);
                        if (agentSession == null)
                            throw new InvalidOperationException("Agent session not found asid=" + agentSessionGuid);

                        var chatEvent = new AgentClosesSessionChatEvent(
                            m_nowProvider.UtcNow,
                            text,
                            agentSession.AgentId);
                        m_chatSessionManager.AddEvent(customerId, chatSessionSkey, chatEvent);
                    });
        }

        public void SendTranscriptToVisitor(
            uint customerId,
            Guid agentSessionGuid,
            long chatSessionSkey,
            int visitorTimezoneOffsetMinutes)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(
                                new
                                    {
                                        customerId,
                                        agentSessionGuid,
                                        chatSessionSkey,
                                    });

                        var agentSession = m_agentManager.GetAgentSession(agentSessionGuid);
                        if (agentSession == null)
                            throw new InvalidOperationException("Agent session not found asid=" + agentSessionGuid);

                        var chatSession = m_chatSessionManager.GetSession(customerId, chatSessionSkey);
                        if (chatSession == null)
                            throw new InvalidOperationException("Chat session not found csid=" + chatSessionSkey);

                        if (chatSession.VisitorId == null)
                            throw new InvalidOperationException("Chat session has no Visitor, can't send transcript. csid=" + chatSessionSkey);

                        if (!chatSession.Events.Any())
                            throw new InvalidOperationException("Session is empty, can't send transcript. csid=" + chatSessionSkey);

                        using (var c = m_databaseFactory.CreateContext())
                        {
                            var visitor = m_visitorStorage.Get(chatSession.VisitorId.Value);
                            if (string.IsNullOrWhiteSpace(visitor.Email))
                                throw new InvalidOperationException(
                                    "Visitor has no emails defined, can't send transcript. csid=" + chatSessionSkey + ", vid=" + visitor.Id);

                            var agentList = m_userManager.GetUsers(
                                c.Db,
                                agentSession.CustomerId,
                                chatSession.Messages
                                    .Where(x => x.SenderAgentId.HasValue)
                                    .Select(x => x.SenderAgentId.Value)
                                    .ToHashSet());

                            // TODO: load templates by visitor's (or specified) culture using reflection
                            var mailRequest = MailHelper.BuildChatSessionTranscriptMailRequest(visitor, chatSession, agentList, visitorTimezoneOffsetMinutes);
                            var mailError = m_emailSender.Send(mailRequest).WaitAndUnwrapException();
                            if (!string.IsNullOrEmpty(mailError))
                            {
                                var error = MailHelper.BuildErrorMessage(chatSessionSkey, visitor.Id, mailError);
                                throw new InvalidOperationException(error);
                            }

                            var chatEvent = new SessionTranscriptSentToVisitorChatEvent(
                                m_nowProvider.UtcNow,
                                agentSession.AgentId,
                                chatSession.Events.Last().Id);
                            m_chatSessionManager.AddEvent(customerId, chatSessionSkey, chatEvent);
                        }
                    });
        }


        public void MediaCallProposal(
            uint customerId,
            Guid agentSessionGuid,
            string agentConnectionId,
            long chatSessionSkey,
            bool hasVideo)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(
                                new
                                    {
                                        customerId,
                                        agentSessionGuid,
                                        chatSessionSkey,
                                        hasVideo,
                                    });

                        var agentSession = m_agentManager.GetAgentSession(agentSessionGuid);
                        if (agentSession == null)
                            throw new InvalidOperationException("Agent session not found sid=" + agentSessionGuid);

                        var chatEvent = new AgentMediaCallProposalChatEvent(
                            m_nowProvider.UtcNow,
                            agentSession.AgentId,
                            hasVideo,
                            agentConnectionId);
                        m_chatSessionManager.AddEvent(customerId, chatSessionSkey, chatEvent);
                    });
        }

        public void MediaCallStop(
            uint customerId,
            Guid agentSessionGuid,
            long chatSessionSkey,
            string reason)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(
                                new
                                    {
                                        customerId,
                                        agentSessionGuid,
                                        chatSessionSkey,
                                        reason,
                                    });

                        var agentSession = m_agentManager.GetAgentSession(agentSessionGuid);
                        if (agentSession == null)
                            throw new InvalidOperationException("Agent session not found asid=" + agentSessionGuid);

                        var chatEvent = new AgentStoppedMediaCallChatEvent(
                            m_nowProvider.UtcNow,
                            reason,
                            agentSession.AgentId);
                        m_chatSessionManager.AddEvent(customerId, chatSessionSkey, chatEvent);
                    });
        }

        public void RtcSendIceCandidate(
            uint customerId,
            Guid agentSessionGuid,
            string visitorConnectionId,
            string candidate)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(
                                new
                                    {
                                        customerId,
                                        agentSessionGuid,
                                        visitorConnectionId,
                                        candidate,
                                    });

                        m_subscriptionManager.VisitorEventSubscribers.Publish(
                            x => x.RtcSendIceCandidate(visitorConnectionId, candidate));
                    });
        }

        public void RtcSendCallOffer(
            uint customerId,
            Guid agentSessionGuid,
            string visitorConnectionId,
            string sdp)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(
                                new
                                    {
                                        customerId,
                                        agentSessionGuid,
                                        visitorConnectionId,
                                        sdp,
                                    });

                        m_subscriptionManager.VisitorEventSubscribers.Publish(
                            x => x.RtcSendCallOffer(visitorConnectionId, sdp));
                    });
        }

        ////            +StartChatSessionToAgent
        ////                notify TargetAgent
        ////            +StartChatSessionToDepartment
        ////                notify all TargetDepartment agents
        ////            +AgentAcceptsAgentSessionChatEvent
        ////                notify visitor in the chat session  
        ////                notify all agents in the chat session
        ////            +AgentAcceptsDepartmentSessionChatEvent
        ////                notify visitor in the chat session  
        ////                notify all agents in the chat session
        ////                notify all agents in the department
        ////            +AgentRejectsAgentSessionChatEvent
        ////                AgentRejectsDepartmentSessionChatEvent
        ////            +AgentSendsMessageChatEvent
        ////            +AgentInvitesAgentChatEvent
        ////            +AgentInvitesDepartmentChatEvent
        ////            +AgentCancelsInviteAgentChatEvent
        ////            +AgentCancelsInviteDeptChatEvent
        ////            AgentLeavesSessionChatEvent
    }
}