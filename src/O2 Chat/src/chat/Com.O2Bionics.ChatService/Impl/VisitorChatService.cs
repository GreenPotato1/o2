using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Impl.Storage;
using Com.O2Bionics.ChatService.Objects.ChatEvents;
using Com.O2Bionics.MailerService.Client;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Network;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService.Impl
{
    [ServiceBehavior(
        ConcurrencyMode = ConcurrencyMode.Multiple,
        InstanceContextMode = InstanceContextMode.Single,
        AddressFilterMode = AddressFilterMode.Any)]
    public class VisitorChatService : ChatServiceBase, IVisitorChatService
    {
        private static readonly ChatFrameLoadResult m_customerIsDisabledResult =
            new ChatFrameLoadResult { Code = WidgetLoadSatusCode.DisabledCustomer };

        private readonly INowProvider m_nowProvider;
        private readonly IAgentManager m_agentManager;
        private readonly IChatSessionManager m_chatSessionManager;
        private readonly ISubscriptionManager m_subscriptionManager;
        private readonly ISettingsStorage m_settingsStorage;
        private readonly IChatWidgetAppearanceManager m_chatWidgetAppearanceManager;
        private readonly IVisitorStorage m_visitorStorage;
        private readonly IChatDatabaseFactory m_databaseFactory;
        private readonly IMailerServiceClient m_emailSender;
        private readonly IUserManager m_userManager;
        private readonly IWidgetLoadCounterStorage m_widgetLoadCounterStorage;
        private readonly IWidgetLoadUnknownDomainStorage m_widgetLoadUnknownDomainStorage;
        private readonly ICustomerCacheNotifier m_customerCacheNotifier;
        private readonly ICustomerStorage m_customerStorage;


        public VisitorChatService()
        {
        }

        public VisitorChatService(
            INowProvider nowProvider,
            IChatDatabaseFactory databaseFactory,
            IVisitorStorage visitorStorage,
            ISettingsStorage settingsStorage,
            IAgentManager agentManager,
            IChatSessionManager chatSessionManager,
            ISubscriptionManager subscriptionManager,
            IChatWidgetAppearanceManager chatWidgetAppearanceManager,
            ChatServiceSettings chatServiceSettings,
            IMailerServiceClient emailSender,
            IUserManager userManager,
            [NotNull] IWidgetLoadUnknownDomainStorage widgetLoadUnknownDomainStorage,
            [NotNull] IWidgetLoadCounterStorage widgetLoadCounterStorage,
            [NotNull] ICustomerCacheNotifier customerCacheNotifier,
            [NotNull] ICustomerStorage customerStorage)
        {
            m_nowProvider = nowProvider;
            m_databaseFactory = databaseFactory;
            m_visitorStorage = visitorStorage;
            m_settingsStorage = settingsStorage;
            m_agentManager = agentManager;
            m_chatSessionManager = chatSessionManager;
            m_subscriptionManager = subscriptionManager;
            m_chatWidgetAppearanceManager = chatWidgetAppearanceManager;
            m_emailSender = emailSender;
            m_userManager = userManager;
            m_widgetLoadUnknownDomainStorage =
                widgetLoadUnknownDomainStorage ?? throw new ArgumentNullException(nameof(widgetLoadUnknownDomainStorage));
            m_widgetLoadCounterStorage = widgetLoadCounterStorage ?? throw new ArgumentNullException(nameof(widgetLoadCounterStorage));
            m_customerCacheNotifier = customerCacheNotifier ?? throw new ArgumentNullException(nameof(customerCacheNotifier));
            m_customerStorage = customerStorage ?? throw new ArgumentNullException(nameof(customerStorage));
        }

        public void Subscribe(string endpoint)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { endpoint });

                        using (var dc = m_databaseFactory.CreateContext())
                        {
                            m_subscriptionManager.VisitorEventSubscribers.Add(dc, new Subscriber(endpoint));
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
                            LogMethodCall(new { endpoint });

                        using (var dc = m_databaseFactory.CreateContext())
                        {
                            m_subscriptionManager.VisitorEventSubscribers.Remove(dc, new Subscriber(endpoint));
                            dc.Commit();
                        }
                    });
        }

        public List<uint> GetActiveCustomerIds()
        {
            var list = m_databaseFactory.Query(db => m_customerStorage.GetActiveIds(db));
            return 0 < list.Count ? list : null;
        }

        public ChatFrameLoadResult ChatWindowLoad(uint customerId, ulong visitorId, string domain, bool isDemoMode)
        {
            return HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { customerId, visitorId, domain, isDemoMode, });

                        if (string.IsNullOrEmpty(domain))
                            throw new ArgumentNullException(nameof(domain));

                        var customer = m_databaseFactory.Query(db => m_customerStorage.Get(db, customerId));
                        if (null == customer || ObjectStatus.Active != customer.Status)
                            return m_customerIsDisabledResult;

                        var settings = m_settingsStorage.GetCustomerSettings(customerId);
                        Debug.Assert(null != settings);

                        var chatAppearanceInfo = m_chatWidgetAppearanceManager.Get(customerId);
                        chatAppearanceInfo.Domains = customer.Domains;

                        var result = new ChatFrameLoadResult
                            {
                                CustomerSettings = new CustomerSettingsInfo
                                    {
                                        IsEnabled = true,
                                        IsProactiveChatEnabled = settings.IsProactiveChatEnabled,
                                        IsVisitorCaptchaRequired = settings.IsVisitorCaptchaRequired,
                                        ChatWidgetAppearanceInfo = chatAppearanceInfo,
                                    },
                            };
                        if (!isDemoMode)
                        {
                            result.Code = CheckWidgetCanBeDisplayed(
                                    customerId,
                                    visitorId,
                                    customer.Domains,
                                    domain)
                                .WaitAndUnwrapException();
                        }

                        if (visitorId != 0u)
                        {
                            m_visitorStorage.GetOrCreate(customerId, visitorId);

                            //TODO: consider to improve. call here and in ChatWindowOpen method
                            var sessionMessages = m_chatSessionManager.GetVisitorOnlineSessionMessages(customerId, visitorId);
                            result.HasActiveSession = m_chatSessionManager.HasActiveOnlineSession(sessionMessages);
                        }

                        return result;
                    });
        }

        private async Task<WidgetLoadSatusCode> CheckWidgetCanBeDisplayed(
            uint customerId,
            ulong visitorId,
            [CanBeNull] string domains,
            [NotNull] string domain)
        {
            var now = m_nowProvider.UtcNow;

            if (!DomainUtilities.HasDomain(domains, domain))
            {
                domain = domain.LimitLength(ServiceConstants.MaximumDomainLength);
                Log.DebugFormat(
                    "Can't show the Chat Widget on a page loaded from the unregistered domain '{0}', customer={1}, vid={2}.",
                    domain,
                    customerId,
                    visitorId);
                var isTooMany = await m_widgetLoadUnknownDomainStorage.Add(now, customerId, domains, domain);
                return isTooMany ? WidgetLoadSatusCode.UnknownDomainNumberExceeded : WidgetLoadSatusCode.UnknownDomain;
            }

            var canLoad = await m_widgetLoadCounterStorage.Add(customerId, now, 1);
            var code = canLoad ? WidgetLoadSatusCode.Allowed : WidgetLoadSatusCode.ViewCounterExceeded;
            if (!canLoad && Log.IsDebugEnabled)
                Log.DebugFormat(
                    "Too many Chat Widget loads for customer={1}, domain='{0}', vid={2}.",
                    domain,
                    customerId,
                    visitorId);

            return code;
        }

        public ChatWindowOpenResult ChatWindowOpen(
            uint customerId,
            ulong visitorId,
            string historyId,
            MediaSupport mediaSupport)
        {
            return HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { customerId, visitorId, historyId, mediaSupport, });

                        var visitor = m_visitorStorage.GetOrCreate(customerId, visitorId);
                        m_visitorStorage.Update(
                            visitorId,
                            new VisitorUpdate { MediaSupport = mediaSupport });

                        var session = m_chatSessionManager.GetVisitorOnlineSession(customerId, visitorId);
                        var sessionMessages = m_chatSessionManager.GetVisitorOnlineSessionMessages(customerId, visitorId);
                        var sessionInfo = session?.AsInfo();

                        var customerObjects = m_databaseFactory.Query(
                            db => new
                                {
                                    agents = m_userManager.GetAgents(db, customerId),
                                    departments = m_agentManager.GetCustomerDepartmentsInfo(db, customerId, true),
                                });

                        var onlineDepartments = new HashSet<uint>(m_agentManager.GetCustomerOnlineDepartments(customerId));
                        onlineDepartments.IntersectWith(customerObjects.departments.Select(x => x.Id));

                        return new ChatWindowOpenResult
                            {
                                Visitor = visitor.AsInfo(),
                                Departments = customerObjects.departments,
                                OnlineDepartments = onlineDepartments.ToList(),
                                HasActiveSession = m_chatSessionManager.HasActiveOnlineSession(sessionMessages),
                                Session = sessionInfo,
                                SessionMessages = sessionMessages,
                                Agents = customerObjects.agents,
                            };
                    });
        }

        public long ResetWidgetLoads(uint[] customerIds)
        {
            return HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { customerIds });

                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        if (null == customerIds || 0 == customerIds.Length)
                            throw new ArgumentNullException(nameof(customerIds));

                        long result = 0;
                        for (var attempt = 0; attempt < TaskConstants.DateChangeAttempts; ++attempt)
                        {
                            var date1 = m_nowProvider.UtcNow.RemoveTime();
                            result = m_widgetLoadCounterStorage.Save(
#if DEBUG
                                true
#endif
                            );

                            Parallel.Invoke(
                                () =>
                                    {
                                        m_widgetLoadCounterStorage.LoadMany(
                                            customerIds
#if DEBUG
                                            ,
                                            true
#endif
                                        );
                                    },
                                () => { m_widgetLoadUnknownDomainStorage.LoadMany(customerIds).WaitAndUnwrapException(); });

                            var date2 = m_nowProvider.UtcNow.RemoveTime();
                            if (date1 == date2)
                                break;
                        }

                        m_customerCacheNotifier.NotifyMany(customerIds);
                        return result;
                    });
        }

        public void UpdateVisitorInfo(uint customerId, ulong visitorId, VisitorInfo newVisitorInfo)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { customerId, visitorId, newVisitorInfo });

                        var actualUpdate = m_visitorStorage.Update(
                            visitorId,
                            new VisitorUpdate
                                {
                                    Name = newVisitorInfo.Name,
                                    Email = newVisitorInfo.Email,
                                    Phone = newVisitorInfo.Phone,
                                    TranscriptMode = newVisitorInfo.TranscriptMode
                                });

                        if (m_chatSessionManager.GetVisitorOnlineSession(customerId, visitorId) != null)
                        {
                            var chatEvent = new VisitorUpdatesInfoChatEvent(
                                m_nowProvider.UtcNow,
                                false,
                                actualUpdate.Name,
                                actualUpdate.Email,
                                actualUpdate.Phone,
                                actualUpdate.TranscriptMode);
                            m_chatSessionManager.AddVisitorOnlineEvent(visitorId, chatEvent);
                        }
                    });
        }

        public void ClearVisitorInfo(uint customerId, ulong visitorId)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { customerId, visitorId, });

                        m_visitorStorage.Update(
                            visitorId,
                            new VisitorUpdate
                                {
                                    Name = "",
                                    Email = "",
                                    Phone = "",
                                });

                        var chatEvent = new VisitorUpdatesInfoChatEvent(
                            m_nowProvider.UtcNow,
                            true);
                        m_chatSessionManager.AddVisitorOnlineEvent(visitorId, chatEvent);
                    });
        }

        [Obsolete]
        public CustomerSettingsInfo GetCustomerSettings(uint customerId)
        {
            return HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { customerId });

                        var customer = m_databaseFactory.Query(db => m_customerStorage.Get(db, customerId));
                        if (null == customer || ObjectStatus.Active != customer.Status)
                            return new CustomerSettingsInfo { IsEnabled = false };

                        var settings = m_settingsStorage.GetCustomerSettings(customerId);
                        return new CustomerSettingsInfo
                            {
                                IsEnabled = true,
                                IsProactiveChatEnabled = settings.IsProactiveChatEnabled,
                                IsVisitorCaptchaRequired = settings.IsVisitorCaptchaRequired,
                            };
                    });
        }

        [Obsolete]
        public bool IsCaptchaRequired(uint customerId)
        {
            return HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { customerId });

                        return m_settingsStorage.GetCustomerSettings(customerId).IsVisitorCaptchaRequired;
                    });
        }

        public void StartChatSession(
            uint customerId,
            ulong visitorId,
            uint departmentId,
            bool isOfflineSession,
            string messageText)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { customerId, visitorId, departmentId, isOfflineSession, messageText });

                        var chatEvent = new VisitorCreatesSessionToDepartmentChatEvent(
                            m_nowProvider.UtcNow,
                            messageText,
                            departmentId,
                            isOfflineSession);
                        m_chatSessionManager.CreateNewSession(customerId, chatEvent, isOfflineSession, visitorId);
                    });
        }

        public void SendMessage(ulong visitorId, string messageText)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { visitorId, messageText });

                        var chatEvent = new VisitorSendsMessageChatEvent(
                            m_nowProvider.UtcNow,
                            messageText);
                        m_chatSessionManager.AddVisitorOnlineEvent(visitorId, chatEvent);
                    });
        }

        public CallResultStatus SendTranscript(ulong visitorId, uint customerId, long sessionId, int visitorTimezoneOffsetMinutes)
        {
            return HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { visitorId, customerId, visitorTimezoneOffsetMinutes });

                        var session = m_chatSessionManager.GetSession(customerId, sessionId);
                        if (session == null)
                            throw new InvalidOperationException("Invalid session id: " + sessionId);
                        if (session.VisitorId != visitorId)
                            throw new InvalidOperationException(
                                $"Session visitor id mismatch, actual: {visitorId}, session[{sessionId}]: {session.VisitorId}");

                        var visitor = m_visitorStorage.Get(visitorId);
                        if (visitor == null)
                            throw new InvalidOperationException("Invalid visitor id: " + visitorId);

                        CallResultStatus result;
                        if (string.IsNullOrWhiteSpace(visitor.Email))
                        {
                            result = new CallResultStatus(CallResultStatusCode.Failure, new ValidationMessage("Email", "Email is not defined"));
                        }
                        else
                        {
                            var agentList = m_databaseFactory.Query(
                                db =>
                                    m_userManager.GetUsers(
                                        db,
                                        session.CustomerId,
                                        session.Messages
                                            .Where(x => x.SenderAgentId.HasValue)
                                            .Select(x => x.SenderAgentId.Value)
                                            .ToHashSet()));

                            var mailRequest = MailHelper.BuildChatSessionTranscriptMailRequest(
                                visitor,
                                session,
                                agentList,
                                visitorTimezoneOffsetMinutes);
                            var mailError = m_emailSender.Send(mailRequest).WaitAndUnwrapException();
                            if (!string.IsNullOrEmpty(mailError))
                            {
                                var error = MailHelper.BuildErrorMessage(sessionId, visitor.Id, mailError);
                                result = new CallResultStatus(CallResultStatusCode.Failure, new ValidationMessage("email", error));
                                return result;
                            }

                            result = new CallResultStatus(CallResultStatusCode.Success);
                        }

                        var chatEvent = new VisitorRequestedTranscriptSentChatEvent(
                            m_nowProvider.UtcNow,
                            visitor.Email,
                            result.StatusCode,
                            result.Messages.FirstOrDefault()?.Message);
                        m_chatSessionManager.AddEvent(customerId, session.Skey, chatEvent);

                        return result;
                    });
        }

        public void MediaCallProposalRejected(ulong visitorId)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { visitorId });

                        var chatEvent = new VisitorRejectedMediaCallProposalChatEvent(
                            m_nowProvider.UtcNow);
                        m_chatSessionManager.AddVisitorOnlineEvent(visitorId, chatEvent);
                    });
        }

        public void MediaCallProposalAccepted(ulong visitorId, bool hasVideo)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { visitorId, hasVideo });

                        var chatEvent = new VisitorAcceptedMediaCallProposalChatEvent(
                            m_nowProvider.UtcNow,
                            hasVideo);
                        m_chatSessionManager.AddVisitorOnlineEvent(visitorId, chatEvent);
                    });
        }

        public void MediaCallSetConnectionId(ulong visitorId, uint customerId, string connectionId)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { visitorId, customerId, connectionId, });

                        var chatEvent = new VisitorSetsMediaCallConnectionIdChatEvent(
                            m_nowProvider.UtcNow,
                            connectionId);
                        m_chatSessionManager.AddVisitorOnlineEvent(visitorId, chatEvent);
                    });
        }

        public void MediaCallStop(ulong visitorId, string reason)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { visitorId, reason, });

                        var chatEvent = new VisitorStoppedMediaCallChatEvent(
                            m_nowProvider.UtcNow,
                            reason);
                        m_chatSessionManager.AddVisitorOnlineEvent(visitorId, chatEvent);
                    });
        }

        public void RtcSendIceCandidate(string agentConnectionId, string candidate)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { agentConnectionId, candidate, });

                        m_subscriptionManager.AgentEventSubscribers.Publish(
                            s => s.RtcSendIceCandidate(agentConnectionId, candidate));
                    });
        }

        public void RtcSendCallAnswer(string agentConnectionId, string sdp)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { agentConnectionId, sdp, });

                        m_subscriptionManager.AgentEventSubscribers.Publish(
                            s => s.RtcSendCallAnswer(agentConnectionId, sdp));
                    });
        }

        // TODO: deprecated?
        public void LeaveChatSession(ulong visitorId)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { visitorId });

                        var chatEvent = new VisitorLeavesSessionChatEvent(m_nowProvider.UtcNow);
                        m_chatSessionManager.AddVisitorOnlineEvent(visitorId, chatEvent);
                    });
        }

        public void OnDisconnected(uint customerId, ulong visitorId, string connectionId, bool stopCalled)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { customerId, visitorId, connectionId, stopCalled });

                        var session = m_chatSessionManager.GetVisitorOnlineSession(customerId, visitorId);
                        if (session != null && session.MediaCallVisitorConnectionId == connectionId)
                        {
                            var timeout = m_settingsStorage.GetServiceSettings().MediaCallDisconnectStopTimeout;

                            Log.DebugFormat(
                                "visitor {0} media call connection {1} is disconnected. scheduling media call stop in {2}",
                                visitorId,
                                connectionId,
                                timeout);

                            var sessionSkey = session.Skey;
                            Task.Delay(timeout)
                                .ContinueWith(
                                    _ =>
                                        {
                                            var session2 = m_chatSessionManager.GetSession(customerId, sessionSkey);
                                            if (session2 == null) return;
                                            if (session2.MediaCallVisitorConnectionId == connectionId)
                                            {
                                                var chatEvent = new VisitorStoppedMediaCallChatEvent(
                                                    m_nowProvider.UtcNow,
                                                    "Media call has been stopped because of the Visitor disconnect.");
                                                m_chatSessionManager.AddVisitorOnlineEvent(visitorId, chatEvent);
                                            }
                                        });
                        }
                    });
        }

        public void OnReconnected(ulong visitorId)
        {
            HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { visitorId });

                        var chatEvent = new VisitorReconnectChatEvent(m_nowProvider.UtcNow);
                        m_chatSessionManager.AddVisitorOnlineEvent(visitorId, chatEvent);
                    });
        }
    }
}