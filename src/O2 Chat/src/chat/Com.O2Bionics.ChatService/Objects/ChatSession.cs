using System;
using System.Collections.Generic;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Objects.ChatEvents;
using LinqToDB;

namespace Com.O2Bionics.ChatService.Objects
{
    public class ChatSession
    {
        public ChatSession(long skey, uint customerId, ulong? visitorId, bool isOffline, DateTime nowUtc)
        {
            Skey = skey;
            CustomerId = customerId;
            AddTimestampUtc = nowUtc;
            VisitorId = visitorId;
            IsOffline = isOffline;

            Status = ChatSessionStatus.Queued;

            Messages = new List<ChatSessionMessage>();
            Invites = new List<ChatSessionInvite>();
            Agents = new List<ChatSessionAgent>();
            Events = new List<ChatEventBase>();

            AgentsInvolved = new HashSet<decimal>();
            DepartmentsInvolved = new HashSet<uint>();

            IsVisitorConnected = false;
            VisitorTranscriptLastEvent = null;
            VisitorTranscriptTimestampUtc = null;
        }

        public ChatSession(CHAT_SESSION session, IEnumerable<CHAT_EVENT> events, IObjectResolver resolver)
        {
            Skey = (long)session.CHAT_SESSION_ID;
            CustomerId = session.CUSTOMER_ID.Value;
            AddTimestampUtc = session.ADD_TIMESTAMP;
            VisitorId = (ulong?)session.VISITOR_ID;
            IsOffline = session.IS_OFFLINE != 0;

            Status = ChatSessionStatus.Queued;

            Messages = new List<ChatSessionMessage>();
            Invites = new List<ChatSessionInvite>();
            Agents = new List<ChatSessionAgent>();

            AgentsInvolved = new HashSet<decimal>();
            DepartmentsInvolved = new HashSet<uint>();

            VisitorTranscriptLastEvent = null;
            VisitorTranscriptTimestampUtc = null;

            Events = events
                .Select(ChatEventBase.Load)
                .OrderBy(y => y.Id)
                .ToList();
            foreach (var evt in Events)
                evt.Apply(this, resolver);

            IsVisitorConnected = false;
        }

        public ChatSession(ChatSession x)
        {
            Skey = x.Skey;
            CustomerId = x.CustomerId;
            AddTimestampUtc = x.AddTimestampUtc;
            VisitorId = x.VisitorId;
            IsOffline = x.IsOffline;

            Status = x.Status;

            Messages = new List<ChatSessionMessage>(x.Messages);
            Invites = new List<ChatSessionInvite>(x.Invites);
            Agents = new List<ChatSessionAgent>(x.Agents);
            Events = new List<ChatEventBase>(x.Events);

            AgentsInvolved = new HashSet<decimal>(x.AgentsInvolved);
            DepartmentsInvolved = new HashSet<uint>(x.DepartmentsInvolved);

            IsVisitorConnected = x.IsVisitorConnected;
            VisitorTranscriptLastEvent = x.VisitorTranscriptLastEvent;
            VisitorTranscriptTimestampUtc = x.VisitorTranscriptTimestampUtc;

            MediaCallStatus = x.MediaCallStatus;
            MediaCallAgentId = x.MediaCallAgentId;
            MediaCallAgentHasVideo = x.MediaCallAgentHasVideo;
            MediaCallVisitorHasVideo = x.MediaCallVisitorHasVideo;
            MediaCallAgentConnectionId = x.MediaCallAgentConnectionId;
            MediaCallVisitorConnectionId = x.MediaCallVisitorConnectionId;
        }

        // immutable fields
        // TODO: rename to id
        // TODO: change type to ulong
        public long Skey { get; private set; }

        public uint CustomerId { get; private set; }
        public DateTime AddTimestampUtc { get; private set; }
        public ulong? VisitorId { get; private set; }
        public bool IsOffline { get; private set; }

        // mutable fields
        public ChatSessionStatus Status { get; set; }

        public List<ChatEventBase> Events { get; private set; }

        // mutable fields not published in the database
        public List<ChatSessionMessage> Messages { get; private set; }

        public List<ChatSessionInvite> Invites { get; private set; }
        public List<ChatSessionAgent> Agents { get; private set; }

        public HashSet<decimal> AgentsInvolved { get; }
        public HashSet<uint> DepartmentsInvolved { get; }

        public bool IsVisitorConnected { get; set; }
        public long? VisitorTranscriptLastEvent { get; set; }
        public DateTime? VisitorTranscriptTimestampUtc { get; set; }

        public MediaCallStatus MediaCallStatus { get; set; }
        public uint MediaCallAgentId { get; set; }
        public bool? MediaCallAgentHasVideo { get; set; }
        public bool? MediaCallVisitorHasVideo { get; set; }
        public string MediaCallAgentConnectionId { get; set; }
        public string MediaCallVisitorConnectionId { get; set; }


        private DateTime LastEventTimestampUtc => Events.Last().TimestampUtc;

        private static readonly HashSet<ChatEventType> m_answerEventTypes = new HashSet<ChatEventType>(
            new[] { ChatEventType.AgentSendsMessage });

        private static readonly HashSet<ChatEventType> m_endEventTypes = new HashSet<ChatEventType>(
            new[]
                {
                    ChatEventType.AgentLeavesSession,
                    ChatEventType.VisitorLeavesSession,
                    ChatEventType.AgentClosesSession,
                });

        private DateTime? AnswerTimestampUtc =>
            Events.FirstOrDefault(x => m_answerEventTypes.Contains(x.EventType))?.TimestampUtc;

        private DateTime? EndTimestampUtc =>
            Events.LastOrDefault(x => m_endEventTypes.Contains(x.EventType))?.TimestampUtc;

        public void AddSystemMessage(ChatEventBase evt, bool isToAgentsOnly, string format, params object[] args)
        {
            Messages.Add(
                new ChatSessionMessage(
                    id: Messages.Count,
                    eventId: evt.Id,
                    timestampUtc: evt.TimestampUtc,
                    sender: ChatMessageSender.System,
                    senderAgentId: null,
                    senderAgentName: null,
                    onBehalfOfId: null,
                    onBehalfOfName: null,
                    isToAgentsOnly: isToAgentsOnly,
                    text: string.Format(format, args)
                ));
        }

        public void AddVisitorMessage(ChatEventBase evt)
        {
            Messages.Add(
                new ChatSessionMessage(
                    id: Messages.Count,
                    eventId: evt.Id,
                    timestampUtc: evt.TimestampUtc,
                    sender: ChatMessageSender.Visitor,
                    senderAgentId: null,
                    senderAgentName: null,
                    onBehalfOfId: null,
                    onBehalfOfName: null,
                    isToAgentsOnly: false,
                    text: evt.Text
                ));
        }

        public void AddAgentMessage(
            ChatEventBase evt,
            uint agentId,
            string agentName,
            bool isToAgentsOnly = false,
            uint? onBehalfOfId = null,
            string onBehalfOfName = null)
        {
            Messages.Add(
                new ChatSessionMessage(
                    id: Messages.Count,
                    eventId: evt.Id,
                    timestampUtc: evt.TimestampUtc,
                    sender: ChatMessageSender.Agent,
                    senderAgentId: agentId,
                    senderAgentName: agentName,
                    onBehalfOfId: onBehalfOfId,
                    onBehalfOfName: onBehalfOfName,
                    isToAgentsOnly: isToAgentsOnly,
                    text: evt.Text
                ));
        }

        public ChatSessionInfo AsInfo()
        {
            var info = new ChatSessionInfo();
            FillSessionInfo(info);
            return info;
        }

        public FullChatSessionInfo AsFullInfo()
        {
            var info = new FullChatSessionInfo();
            FillSessionInfo(info);
            info.Messages = Messages.Select(x => x.AsInfo()).ToList();
            return info;
        }

        private void FillSessionInfo(ChatSessionInfo info)
        {
            info.Skey = Skey;
            info.Status = Status;
            info.IsOffline = IsOffline;
            info.IsVisitorConnected = IsVisitorConnected;
            info.VisitorTranscriptLastEvent = VisitorTranscriptLastEvent;
            info.VisitorTranscriptTimestampUtc = VisitorTranscriptTimestampUtc;
            info.AddTimestampUtc = AddTimestampUtc;
            info.LastEventTimestampUtc = LastEventTimestampUtc;
            info.AnswerTimestampUtc = AnswerTimestampUtc;
            info.EndTimestampUtc = EndTimestampUtc;
            info.MediaCallStatus = MediaCallStatus;
            info.MediaCallAgentId = MediaCallAgentId;
            info.MediaCallAgentHasVideo = MediaCallAgentHasVideo;
            info.MediaCallVisitorHasVideo = MediaCallVisitorHasVideo;
            info.MediaCallAgentConnectionId = MediaCallAgentConnectionId;
            info.MediaCallVisitorConnectionId = MediaCallVisitorConnectionId;

            info.VisitorId = VisitorId;
            info.Invites = Invites.Select(x => x.AsInfo()).ToList();
            info.Agents = Agents.Select(x => x.AsInfo()).ToList();
            info.AgentsInvolved = new HashSet<decimal>(AgentsInvolved);
            info.DepartmentsInvolved = new HashSet<uint>(DepartmentsInvolved);

            info.VisitorMessageCount = Messages.Count(x => x.Sender == ChatMessageSender.Visitor);
            info.AgentMessageCount = Messages.Count(x => x.Sender == ChatMessageSender.Agent);
        }

        public List<ChatSessionMessageInfo> EventMessagesAsInfo(long eventId)
        {
            return Messages
                .Where(x => x.EventId == eventId)
                .OrderBy(x => x.Id)
                .Select(x => x.AsInfo())
                .ToList();
        }

        public void Add(ChatEventBase chatEvent)
        {
            Events.Add(chatEvent);
        }

        public static void Insert(ChatDatabase db, ChatSession obj)
        {
            var dbo = new CHAT_SESSION
                {
                    CHAT_SESSION_ID = obj.Skey,
                    CUSTOMER_ID = obj.CustomerId,
                    ADD_TIMESTAMP = obj.AddTimestampUtc,
                    VISITOR_ID = obj.VisitorId,
                    IS_OFFLINE = (sbyte)(obj.IsOffline ? 1 : 0),
                    CHAT_SESSION_STATUS_ID = (sbyte)obj.Status,
                };
            db.Insert(dbo);

            foreach (var evt in obj.Events)
                evt.Save(db, obj.Skey);
        }

        public static void Update(ChatDatabase db, ChatSession original, ChatSession changed)
        {
            if (original.Status != changed.Status)
            {
                var update = db.CHAT_SESSION
                    .Where(x => x.CHAT_SESSION_ID == original.Skey)
                    .Set(x => x.CHAT_SESSION_STATUS_ID, (sbyte)changed.Status);
                update.Update();
            }

            var lastStoredEventId = original.Events.Last().Id;
            foreach (var evt in changed.Events.Where(x => x.Id > lastStoredEventId))
                evt.Save(db, changed.Skey);
        }
    }
}