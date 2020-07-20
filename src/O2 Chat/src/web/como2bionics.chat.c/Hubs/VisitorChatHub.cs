using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.Utils;
using Microsoft.AspNet.SignalR;

namespace Com.O2Bionics.ChatService.Web.Chat.Hubs
{
    public class VisitorChatHub : Hub
    {
        private static ITcpServiceClient<IVisitorChatService> ChatService =>
            GlobalContainer.Resolve<ITcpServiceClient<IVisitorChatService>>();

        public ChatWindowOpenResult ChatWindowOpen(MediaSupport mediaSupport, string mediaCallMode)
        {
            LogEvent(new { mediaSupport, mediaCallMode, });

            return ChatService.Call(
                x => x.ChatWindowOpen(
                    CustomerId,
                    VisitorId,
                    PageHistoryId,
                    mediaSupport));
        }

        public void UpdateVisitorInfo(VisitorInfo visitorInfo)
        {
            LogEvent(new { visitorInfo, });

            ChatService.Call(x => x.UpdateVisitorInfo(CustomerId, VisitorId, visitorInfo));
        }

        public void ClearVisitorInfo()
        {
            LogEvent(new { });

            ChatService.Call(x => x.ClearVisitorInfo(CustomerId, VisitorId));
        }

        public void StartChat(uint departmentId, string text)
        {
            LogEvent(new { departmentId, text, });

            if (string.IsNullOrWhiteSpace(text))
                throw new HubException("Message Text can't be null or whitespace");
            ChatService.Call(x => x.StartChatSession(CustomerId, VisitorId, departmentId, false, text));
        }

        public void SendOfflineMessage(uint departmentId, string text)
        {
            LogEvent(new { departmentId, text, });

            if (string.IsNullOrWhiteSpace(text))
                throw new HubException("Message Text can't be null or whitespace");
            ChatService.Call(x => x.StartChatSession(CustomerId, VisitorId, departmentId, true, text));
        }

        public void SendMessage(string text)
        {
            LogEvent(new { text, });

            if (string.IsNullOrWhiteSpace(text))
                throw new HubException("Message Text can't be null or whitespace");
            ChatService.Call(x => x.SendMessage(VisitorId, text));
        }

        public void EndChat()
        {
            LogEvent(new { });

            Clients.Group(GroupNames.VisitorGroupName(VisitorId))
                .VisitorClosedSession();
        }


        public void MediaCallProposalRejected()
        {
            LogEvent(new { });

            ChatService.Call(x => x.MediaCallProposalRejected(VisitorId));
        }

        public void MediaCallProposalAccepted(bool withVideo)
        {
            LogEvent(new { withVideo, });

            ChatService.Call(x => x.MediaCallProposalAccepted(VisitorId, withVideo));
        }

        public void MediaCallSetConnectionId()
        {
            LogEvent(new { });

            ChatService.Call(x => x.MediaCallSetConnectionId(VisitorId, CustomerId, Context.ConnectionId));
        }

        public void MediaCallStop(string reason)
        {
            LogEvent(new { reason, });

            ChatService.Call(x => x.MediaCallStop(VisitorId, reason));
        }

        public void RtcSendIceCandidate(string agentConnectionId, string candidate)
        {
            ChatService.Call(s => s.RtcSendIceCandidate(agentConnectionId, candidate));
        }

        public void RtcSendCallAnswer(string agentConnectionId, string sdp)
        {
            ChatService.Call(s => s.RtcSendCallAnswer(agentConnectionId, sdp));
        }

        public CallResultStatus SendTranscript(long sessionId, int visitorTimezoneOffsetMinutes)
        {
            LogEvent(new { sessionId, visitorTimezoneOffsetMinutes });

            return ChatService.Call(x => x.SendTranscript(VisitorId, CustomerId, sessionId, visitorTimezoneOffsetMinutes));
        }

        public override async Task OnConnected()
        {
            LogEvent(new { });

            await Groups.Add(Context.ConnectionId, GroupNames.CustomerGroupName(CustomerId));
            await Groups.Add(Context.ConnectionId, GroupNames.VisitorGroupName(VisitorId));

//            VisitorHubConnections.OnConnected(VisitorId, Context.ConnectionId, PageHistoryId);

            // TODO: service.OnReconnect()

            await base.OnConnected();
        }

        public override async Task OnReconnected()
        {
            LogEvent(new { });

            await Groups.Add(Context.ConnectionId, GroupNames.CustomerGroupName(CustomerId));
            await Groups.Add(Context.ConnectionId, GroupNames.VisitorGroupName(VisitorId));
//            VisitorHubConnections.OnConnected(VisitorId, Context.ConnectionId, PageHistoryId);

            // TODO: service.OnReconnect()

            await base.OnReconnected();
        }

        public override async Task OnDisconnected(bool stopCalled)
        {
            LogEvent(new { stopCalled, });

//            VisitorHubConnections.OnDisconnected(VisitorId, Context.ConnectionId);

            ChatService.Call(x => x.OnDisconnected(CustomerId, VisitorId, Context.ConnectionId, stopCalled));

            await base.OnDisconnected(stopCalled);
        }


        private void LogEvent(object args = null, [CallerMemberName] string methodName = "")
        {
            var argsString = args != null ? args.JsonStringify() : "";
            Trace.WriteLine(
                $"visitor hub({CustomerId}:v={VisitorId},h=:{PageHistoryId}/{Context.ConnectionId}): {methodName}({argsString})");
        }

        #region required query string parameters

        private uint? m_customerId;

        private uint CustomerId
        {
            get
            {
                if (!m_customerId.HasValue)
                {
                    uint v;
                    var s = Context.QueryString["c"];
                    if (!uint.TryParse(s, out v))
                        throw new HubException($"Can't parse required uint CustomerId(c) '{s}'");
                    m_customerId = v;
                }

                return m_customerId.Value;
            }
        }

        private ulong? m_visitorId;

        private ulong VisitorId
        {
            get
            {
                if (!m_visitorId.HasValue)
                {
                    var s = Context.QueryString["v"];
                    if (!ulong.TryParse(s, out var v))
                        throw new HubException($"Can't parse required VisitorId(v) '{s}'");
                    m_visitorId = v;
                }

                return m_visitorId.Value;
            }
        }

        private bool m_pageHistoryIdSet = false;
        private string m_pageHistoryId;

        private string PageHistoryId
        {
            get
            {
                if (!m_pageHistoryIdSet)
                {
                    m_pageHistoryId = Context.QueryString["h"];
                    m_pageHistoryIdSet = true;
                }

                return m_pageHistoryId;
            }
        }

        #endregion
    }
}