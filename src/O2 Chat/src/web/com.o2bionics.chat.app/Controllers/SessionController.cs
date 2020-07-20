using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Web.Mvc;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.Utils;
using log4net;

namespace Com.O2Bionics.ChatService.Web.Console.Controllers
{
    [Authorize(Roles = RoleNames.Supervisor + "," + RoleNames.Agent)]
    public class SessionController : ManagementControllerBase
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(SessionController));

        [HttpGet]
        public ActionResult Search(
            string startDate,
            string endDate,
            string searchWord,
            HashSet<decimal> agents,
            int pageSize,
            int? pageNumber)
        {
            var startDateValue = string.IsNullOrWhiteSpace(startDate)
                ? DateTime.UtcNow.Date
                : DateUtilities.ParseDate(startDate);
            var endDateValue = string.IsNullOrWhiteSpace(endDate)
                ? DateTime.UtcNow.Date
                : DateUtilities.ParseDate(endDate);

            var result = ManagementService.Call(
                s => s.GetSessions(
                    CustomerId,
                    CurrentUserId,
                    new SessionSearchFilter
                        {
                            SearchString = searchWord,
                            Agents = agents,
                            StartDate = startDateValue,
                            EndDate = endDateValue,
                        },
                    pageSize,
                    pageNumber ?? 1));
            return JilJson(result);
        }

        [HttpGet]
        public ActionResult Get(long sid, int messagesPageSize)
        {
            var result = ManagementService.Call(
                s => s.GetSession(
                    CustomerId,
                    CurrentUserId,
                    sid,
                    messagesPageSize));
            return JilJson(result);
        }

        [HttpGet]
        public ActionResult Messages(long sessionSkey, int messagesPageSize, int? pageNumber)
        {
            var result = ManagementService.Call(
                s => s.GetSessionMessages(
                    CustomerId,
                    CurrentUserId,
                    sessionSkey,
                    messagesPageSize,
                    pageNumber ?? 1));
            return JilJson(result);
        }


        [HttpGet]
        public ActionResult GetConsoleInfo(Guid agentSessionId, string connectionId)
        {
            LogEvent(agentSessionId, connectionId);
            return JilJson(AgentService.Call(s => s.GetConsoleInfo(CustomerId, agentSessionId)));
        }

        [HttpGet]
        public ActionResult GetFullChatSessionInfo(Guid agentSessionId, string connectionId, long chatSessionSkey)
        {
            LogEvent(agentSessionId, connectionId, new { chatSessionSkey, });
            return JilJson(AgentService.Call(s => s.GetFullChatSessionInfo(CustomerId, agentSessionId, chatSessionSkey)));
        }

        [HttpGet]
        public ActionResult GetVisitorInfo(Guid agentSessionId, string connectionId, uint visitorId)
        {
            LogEvent(agentSessionId, connectionId, new { visitorId, });
            return JilJson(AgentService.Call(s => s.GetVisitorInfo(CustomerId, agentSessionId, visitorId)));
        }
        
        [HttpPost]
        public ActionResult AcceptSessionAsAgent(Guid agentSessionId, string connectionId, long chatSessionSkey)
        {
            LogEvent(agentSessionId, connectionId, new { chatSessionSkey, });
            AgentService.Call(s => s.AcceptAgentChatSession(CustomerId, agentSessionId, chatSessionSkey));
            return VoidJson;
        }

        [HttpPost]
        public ActionResult AcceptSessionAsDepartment(Guid agentSessionId, string connectionId, long chatSessionSkey, uint departmentId)
        {
            LogEvent(agentSessionId, connectionId, new { chatSessionSkey, departmentId });
            AgentService.Call(s => s.AcceptDepartmentChatSession(CustomerId, agentSessionId, chatSessionSkey, departmentId));
            return VoidJson;
        }

        [HttpPost]
        public ActionResult ExitSession(Guid agentSessionId, string connectionId, long chatSessionSkey, string text)
        {
            LogEvent(agentSessionId, connectionId, new { chatSessionSkey, text, });
            AgentService.Call(s => s.LeaveChatSession(CustomerId, agentSessionId, chatSessionSkey, text, false, false));
            return VoidJson;
        }

        [HttpPost]
        public ActionResult CloseSession(Guid agentSessionId, string connectionId, long chatSessionSkey, string text)
        {
            LogEvent(agentSessionId, connectionId, new { chatSessionSkey, text, });
            AgentService.Call(s => s.CloseChatSession(CustomerId, agentSessionId, chatSessionSkey, text));
            return VoidJson;
        }

        [HttpPost]
        public ActionResult SendMessage(Guid agentSessionId, string connectionId, long chatSessionSkey, bool isToAgentsOnly, string text)
        {
            LogEvent(agentSessionId, connectionId, new { chatSessionSkey, isToAgentsOnly, text, });
            AgentService.Call(s => s.SendMessageToChatSession(CustomerId, agentSessionId, chatSessionSkey, isToAgentsOnly, text));
            return VoidJson;
        }

        [HttpPost]
        public ActionResult StartChatSessionToAgent(Guid agentSessionId, string connectionId, uint targetAgentId, string message)
        {
            LogEvent(agentSessionId, connectionId, new { targetAgentId, message });
            AgentService.Call(s => s.StartChatSessionToAgent(CustomerId, agentSessionId, targetAgentId, message));
            return VoidJson;
        }

        [HttpPost]
        public ActionResult StartChatSessionToDepartment(Guid agentSessionId, string connectionId, uint targetDepartmentId, string message)
        {
            LogEvent(agentSessionId, connectionId, new { targetDepartmentId, message });
            AgentService.Call(s => s.StartChatSessionToDepartment(CustomerId, agentSessionId, targetDepartmentId, message));
            return VoidJson;
        }

        [HttpPost]
        public ActionResult InviteAgentToChatSession(
            Guid agentSessionId,
            string connectionId,
            long chatSessionSkey,
            uint invitedAgentId,
            bool actOnBehalfOfInvitor,
            string text)
        {
            LogEvent(agentSessionId, connectionId, new { chatSessionSkey, invitedAgentId, actOnBehalfOfInvitor, text, });
            AgentService.Call(
                s => s.InviteAgentToChatSession(CustomerId, agentSessionId, chatSessionSkey, invitedAgentId, actOnBehalfOfInvitor, text));
            return VoidJson;
        }

        [HttpPost]
        public ActionResult CancelAgentInvitationToChatSession(
            Guid agentSessionId,
            string connectionId,
            long chatSessionSkey,
            uint invitedAgentId,
            string text)
        {
            LogEvent(agentSessionId, connectionId, new { chatSessionSkey, invitedAgentId, text, });
            AgentService.Call(s => s.CancelAgentInvitationToChatSession(CustomerId, agentSessionId, chatSessionSkey, invitedAgentId, text));
            return VoidJson;
        }

        [HttpPost]
        public ActionResult InviteDepartmentToChatSession(
            Guid agentSessionId,
            string connectionId,
            long chatSessionSkey,
            uint invitedDepartmentId,
            bool actOnBehalfOfInvitor,
            string text)
        {
            LogEvent(agentSessionId, connectionId, new { chatSessionSkey, invitedDepartmentId, actOnBehalfOfInvitor, text, });
            AgentService.Call(
                s => s.InviteDepartmentToChatSession(CustomerId, agentSessionId, chatSessionSkey, invitedDepartmentId, actOnBehalfOfInvitor, text));
            return VoidJson;
        }

        [HttpPost]
        public ActionResult CancelDepartmentInvitationToChatSession(
            Guid agentSessionId,
            string connectionId,
            long chatSessionSkey,
            uint invitedDepartmentId,
            string text)
        {
            LogEvent(agentSessionId, connectionId, new { chatSessionSkey, invitedDepartmentId, text, });
            AgentService.Call(s => s.CancelDepartmentInvitationToChatSession(CustomerId, agentSessionId, chatSessionSkey, invitedDepartmentId, text));
            return VoidJson;
        }

        [HttpPost]
        public ActionResult SendTranscriptToVisitor(Guid agentSessionId, string connectionId, long chatSessionSkey, int visitorTimezoneOffsetMinutes)
        {
            LogEvent(agentSessionId, connectionId, new { chatSessionSkey });
            AgentService.Call(s => s.SendTranscriptToVisitor(CustomerId, agentSessionId, chatSessionSkey, visitorTimezoneOffsetMinutes));
            return VoidJson;
        }

        [HttpPost]
        public ActionResult MediaCallProposal(Guid agentSessionId, string connectionId, long chatSessionSkey, bool hasVideo)
        {
            LogEvent(agentSessionId, connectionId, new { chatSessionSkey, hasVideo, });
            AgentService.Call(s => s.MediaCallProposal(CustomerId, agentSessionId, connectionId, chatSessionSkey, hasVideo));
            return VoidJson;
        }

        [HttpPost]
        public ActionResult MediaCallStop(Guid agentSessionId, string connectionId, long chatSessionSkey, string reason)
        {
            LogEvent(agentSessionId, connectionId, new { chatSessionSkey, reason, });
            AgentService.Call(s => s.MediaCallStop(CustomerId, agentSessionId, chatSessionSkey, reason));
            return VoidJson;
        }

        [HttpPost]
        public ActionResult RtcSendIceCandidate(Guid agentSessionId, string connectionId, string visitorConnectionId, string candidate)
        {
            // LogEvent(sessionId, connectionId,new { visitorConnectionId, candidate });
            AgentService.Call(s => s.RtcSendIceCandidate(CustomerId, agentSessionId, visitorConnectionId, candidate));
            return VoidJson;
        }

        [HttpPost]
        public ActionResult RtcSendCallOffer(Guid agentSessionId, string connectionId, string visitorConnectionId, string sdp)
        {
            // LogEvent(sessionId, connectionId,new { visitorConnectionId, sdp, });
            AgentService.Call(s => s.RtcSendCallOffer(CustomerId, agentSessionId, visitorConnectionId, sdp));
            return VoidJson;
        }

        [HttpPost]
        public ActionResult SessionSetStatus(Guid agentSessionId, string connectionId, bool isOnline)
        {
            LogEvent(agentSessionId, connectionId, new { isOnline });
            AgentService.Call(s => s.SessionSetStatus(CustomerId, agentSessionId, isOnline));
            return VoidJson;
        }

        private void LogEvent(
            Guid agentSessionGuid,
            string connectionId = null,
            object args = null,
            [CallerMemberName] string caller = "")
        {
            var argsString = args != null ? args.JsonStringify() : "";
            if (m_log.IsDebugEnabled)
                m_log.Debug(
                    $"Call({CustomerId}:a={CurrentUserId},s={agentSessionGuid}/{connectionId}): {caller}({argsString})");
        }
    }
}