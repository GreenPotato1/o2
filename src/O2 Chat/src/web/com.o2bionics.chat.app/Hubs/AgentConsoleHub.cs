using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.Utils;
using log4net;
using Microsoft.AspNet.SignalR;

// ReSharper disable SpecifyACultureInStringConversionExplicitly1

namespace Com.O2Bionics.ChatService.Web.Console.Hubs
{
    [Authorize(Roles = RoleNames.Agent)]
    public class AgentConsoleHub : Hub
    {                
        public override async Task OnConnected()
        {
            LogEvent(new { });

            var result = AgentService.Call(s => s.Connected(CustomerId, AgentSessionGuid, AgentId, Context.ConnectionId));

            if (result != null)
            {
                var groupNames =
                    new[]
                            {
                                GroupNames.CustomerGroupName(result.CustomerId),
                                GroupNames.AgentGroupName(result.AgentId),
                            }
                        .Concat(result.Departments.Select(GroupNames.DepartmentGroupName));
                GroupManager.Add(AgentSessionGuid, Context.ConnectionId, groupNames);
            }

            await base.OnConnected();
        }

        public override Task OnReconnected()
        {
            LogEvent(new { });

//            var result = AgentService.Call(s => s.Reconnected(ConnectionId));
//            if (result != null)
//            {
//                var groupNames =
//                    new[]
//                        {
//                            GroupNames.CustomerGroupName(CustomerId),
//                            GroupNames.AgentGroupName(AgentId),
//                        }
//                        .Concat(result.Departments.Select(GroupNames.DepartmentGroupName));
//                GroupManager.Add(Context.ConnectionId, groupNames);
//            }
//
            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            LogEvent(new { stopCalled });

            GroupManager.Remove(Context.ConnectionId);
            AgentService.Call(s => s.Disconnected(CustomerId, AgentSessionGuid, AgentId, Context.ConnectionId));

            return base.OnDisconnected(stopCalled);
        }

        private static readonly ILog m_log = LogManager.GetLogger(typeof(AgentConsoleHub));

        private void LogEvent(object args = null, [CallerMemberName] string methodName = "")
        {
            if (!m_log.IsDebugEnabled) return;

            var argsString = args != null ? args.JsonStringify() : "";
            m_log.DebugFormat(
                "agent hub({0}:a={1},s={2}/{3}): {4}({5})",
                CustomerId,
                AgentId,
                AgentSessionGuid,
                Context.ConnectionId,
                methodName,
                argsString);
        }

        #region parameters

        private static TcpServiceClient<IAgentConsoleService> AgentService =>
            GlobalContainer.Resolve<TcpServiceClient<IAgentConsoleService>>();

        private Guid? m_agentSessionGuid;

        private Guid AgentSessionGuid
        {
            get
            {
                if (!m_agentSessionGuid.HasValue)
                {
                    m_agentSessionGuid = Context.QueryString["asgi"].FromWebGuid();
                }
                return m_agentSessionGuid.Value;
            }
        }

        private uint? m_customerId;
        private uint? m_agentId;
        private uint AgentId
        {
            get
            {
                if (!m_agentId.HasValue)
                {
                    GetIdentifiers();
                }
// ReSharper disable once PossibleInvalidOperationException
                return m_agentId.Value;
            }
        }
        private uint CustomerId
        {
            get
            {
                if (!m_customerId.HasValue)
                {
                    GetIdentifiers();
                }
// ReSharper disable once PossibleInvalidOperationException
                return m_customerId.Value;
            }
        }

        private void GetIdentifiers()
        {
            var user = Context.User as ClaimsPrincipal;
            if (user == null)
                throw new HubException("Current user is not a ClaimsPrincipal");
            m_customerId = GetUintIdentifier(user, ClaimTypes.GroupSid);
            m_agentId = GetUintIdentifier(user, ClaimTypes.Sid);
        }

        private static uint GetUintIdentifier(ClaimsPrincipal user, string claimType)
        {
            var claim = user.FindFirst(claimType);
            if (claim == null)
                throw new HubException("Current user has no claims of required type " + claimType);
            uint value;
            if (!uint.TryParse(claim.Value, out value))
                throw new HubException($"Value '{claim.Value}' is not a valid decimal value");
            return value;
        }

        #endregion
    }
}