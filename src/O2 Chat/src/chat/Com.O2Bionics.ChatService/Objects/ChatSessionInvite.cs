using System;
using Com.O2Bionics.ChatService.Contract;

namespace Com.O2Bionics.ChatService.Objects
{
    public abstract class ChatSessionInvite
    {
        public DateTime CreatedTimestampUtc { get; private set; }

        // null if Creator is a Visitor
        public uint? CreatorAgentId { get; private set; }

        protected uint InviteId { get; private set; }
        public uint? ActOnBehalfOfAgentId { get; private set; }

        public DateTime? AcceptedTimestampUtc { get; private set; }
        public uint? AcceptedByAgentId { get; private set; }

        public DateTime? CanceledTimestampUtc { get; private set; }
        public uint? CanceledByAgentId { get; private set; }

        protected ChatSessionInvite(
            DateTime createdTimestampUtc,
            uint? creatorAgentId,
            uint inviteAgentId,
            uint? actOnBehalfOfAgentId = null)
        {
            CreatedTimestampUtc = createdTimestampUtc;
            CreatorAgentId = creatorAgentId;
            InviteId = inviteAgentId;
            ActOnBehalfOfAgentId = actOnBehalfOfAgentId;
        }

        public void Accept(DateTime timestampUtc, uint agentId)
        {
            AcceptedTimestampUtc = timestampUtc;
            AcceptedByAgentId = agentId;
        }

        public void Cancel(DateTime timestampUtc, uint agentId)
        {
            CanceledTimestampUtc = timestampUtc;
            CanceledByAgentId = agentId;
        }

        public bool IsAccepted => AcceptedByAgentId.HasValue;

        public bool IsCanceled => CanceledByAgentId.HasValue;

        public bool IsPending => !IsAccepted && !IsCanceled;

        public abstract ChatSessionInviteInfo AsInfo();

        public ChatSessionInvite CreatePendingClone(DateTime timestampUtc)
        {
            var t = (ChatSessionInvite)MemberwiseClone();

            t.CreatedTimestampUtc = timestampUtc;
            t.AcceptedByAgentId = null;
            t.AcceptedTimestampUtc = null;
            t.CanceledByAgentId = null;
            t.CanceledTimestampUtc = null;

            return t;
        }
    }

    public class ChatSessionAgentInvite : ChatSessionInvite
    {
        public uint AgentId
        {
            get { return InviteId; }
        }

        public ChatSessionAgentInvite(
            DateTime createdTimestampUtc,
            uint? creatorAgentId,
            uint agentId,
            uint? actOnBehalfOfAgentId = null)
            : base(createdTimestampUtc, creatorAgentId, agentId, actOnBehalfOfAgentId)
        {
        }

        public override ChatSessionInviteInfo AsInfo()
        {
            return new ChatSessionAgentInviteInfo
                {
                    InviteType = ChatSessionInviteType.Agent,
                    AgentId = InviteId,
                    ActOnBehalfOfAgentId = ActOnBehalfOfAgentId,
                    CreatedTimestampUtc = CreatedTimestampUtc,
                    CreatorAgentId = CreatorAgentId,
                    AcceptedTimestampUtc = AcceptedTimestampUtc,
                    AcceptedByAgentId = AcceptedByAgentId,
                    CanceledTimestampUtc = CanceledTimestampUtc,
                    CanceledByAgentId = CanceledByAgentId,
                };
        }
    }

    public class ChatSessionDepartmentInvite : ChatSessionInvite
    {
        public uint DepartmentId
        {
            get { return InviteId; }
        }

        public ChatSessionDepartmentInvite(
            DateTime createdTimestampUtc,
            uint? creatorAgentId,
            uint departmentId,
            uint? actOnBehalfOfAgentId = null)
            : base(createdTimestampUtc, creatorAgentId, departmentId, actOnBehalfOfAgentId)
        {
        }

        public override ChatSessionInviteInfo AsInfo()
        {
            return new ChatSessionDepartmentInviteInfo
                {
                    InviteType = ChatSessionInviteType.Department,
                    DepartmentId = (uint)InviteId,
                    ActOnBehalfOfAgentId = ActOnBehalfOfAgentId,
                    CreatedTimestampUtc = CreatedTimestampUtc,
                    CreatorAgentId = CreatorAgentId,
                    AcceptedTimestampUtc = AcceptedTimestampUtc,
                    AcceptedByAgentId = AcceptedByAgentId,
                    CanceledTimestampUtc = CanceledTimestampUtc,
                    CanceledByAgentId = CanceledByAgentId,
                };
        }
    }
}