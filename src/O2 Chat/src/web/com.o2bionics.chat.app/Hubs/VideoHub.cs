using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Web;
using Microsoft.AspNet.SignalR;

// ReSharper disable SpecifyACultureInStringConversionExplicitly

namespace Com.O2Bionics.ChatService.Web.Console.Hubs
{
    [Authorize]
    public class VideoHub : Hub
    {
        public void Start()
        {
        }

        public void SendCallRequest(string peerCid)
        {
            Trace.WriteLine(string.Format("send call request {0}", peerCid));
            Clients.Client(peerCid).CallRequest(Context.ConnectionId);
        }

        public void AcceptCall(string peerCid)
        {
            Trace.WriteLine(string.Format("accept call {0}", peerCid));
            Clients.Client(peerCid).CallAccepted(Context.ConnectionId);
        }

        public void RejectCall(string peerCid, string message)
        {
            Trace.WriteLine(string.Format("reject call {0} {1}", peerCid, message));
            Clients.Client(peerCid).CallRejected(Context.ConnectionId, message);
        }

        public void ExitCall(string peerCid)
        {
            Trace.WriteLine(string.Format("exit call {0}", peerCid));
            Clients.Client(peerCid).ExitCall(Context.ConnectionId);
        }

        public void SendCallOffer(string peerCid, string sdp)
        {
            Trace.WriteLine(string.Format("send call offer {0} {1}", peerCid, sdp));
            Clients.Client(peerCid).CallOffer(Context.ConnectionId, sdp);
        }

        public void SendCallAnswer(string peerCid, string sdp)
        {
            Trace.WriteLine(string.Format("send call answer {0} {1}", peerCid, sdp));
            Clients.Client(peerCid).CallAnswer(Context.ConnectionId, sdp);
        }

        public void SendIceCandidate(string peerCid, string candidate)
        {
            Trace.WriteLine(string.Format("send ice candidate {0} {1}", peerCid, candidate));
            Clients.Client(peerCid).IceCandidate(Context.ConnectionId, candidate);
        }

        public override async Task OnConnected()
        {
            LogConnectionEvent("connect");

            await base.OnConnected();
        }

        public override Task OnReconnected()
        {
            LogConnectionEvent("reconnect");

            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            LogConnectionEvent("disconnect", stopCalled.ToString());

            return base.OnDisconnected(stopCalled);
        }

        private void LogConnectionEvent(string name, params string[] args)
        {
            var ids = new[]
                    {
                        Context.ConnectionId,
                        ConnectionId.AsSignalRConnectionId(),
                        CustomerId.ToString(),
                        AgentId.ToString(),
                    }
                .Concat(args);
            Trace.WriteLine("video " + name + " " + string.Join(" ", ids));
        }


        private static TcpServiceClient<IAgentConsoleService> AgentService
        {
            get { return GlobalContainer.Resolve<TcpServiceClient<IAgentConsoleService>>(); }
        }

        private Guid ConnectionId
        {
            get { return Guid.Parse(Context.QueryString["coid"] ?? Context.ConnectionId); }
        }

        private bool IsReconnectedConnection
        {
            get { return Context.QueryString["coid"] != null; }
        }

        private uint AgentId
        {
            get { return GetUintIdentifier(ClaimTypes.Sid); }
        }

        private uint CustomerId
        {
            get { return GetUintIdentifier(ClaimTypes.GroupSid); }
        }

        private uint GetUintIdentifier(string claimType)
        {
            var user = Context.User as ClaimsPrincipal;
            if (user == null)
                throw new HubException("Current user is not a ClaimsPrincipal");
            var claim = user.FindFirst(claimType);
            if (claim == null)
                throw new HubException("Current user has no claims of required type " + claimType);
            uint value;
            if (!uint.TryParse(claim.Value, out value))
                throw new HubException(string.Format("Value '{0}' is not a valid Int32 value", claim.Value));
            return value;
        }
    }
}