using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.ChatService.Contract.MailerService;
using Com.O2Bionics.ChatService.Objects;
using Com.O2Bionics.MailerService.Contract;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;
using Jil;

namespace Com.O2Bionics.ChatService.Impl
{
    public static class MailHelper
    {
        private sealed class Message
        {
            [UsedImplicitly] public DateTime TimestampUtc;
            [UsedImplicitly] public string Text;
            [UsedImplicitly] public string SenderClass;
            [UsedImplicitly] public string SenderName;
        }

        [NotNull]
        public static MailRequest BuildChatSessionTranscriptMailRequest(
            [NotNull] Visitor visitor,
            [NotNull] ChatSession chatSession,
            [NotNull] List<UserInfo> agentList,
            int visitorTimezoneOffsetMinutes)
        {
            var agentMap = agentList.ToDictionary(a => a.Id);

            var messages = chatSession.Messages
                .Where(x => !x.IsToAgentsOnly)
                .ToList()
                .Select(m => BuildMessage(visitor, agentMap, m))
                .ToList();

            var templateModel = new
                {
                    VisitorTimezoneOffsetMinutes = visitorTimezoneOffsetMinutes,
                    Messages = messages
                };
            var mailRequest = new MailRequest
                {
                    ProductCode = ProductCodes.Chat,
                    TemplateId = TemplateIds.ChatSessionTranscript,
                    To = visitor.Email,
                    TemplateModel = JSON.Serialize(templateModel, JsonSerializerBuilder.SkipNullJilOptions)
                };
            return mailRequest;
        }

        [NotNull]
        public static string BuildErrorMessage(long chatSessionSkey, ulong visitorId, string error)
        {
            Debug.Assert(!string.IsNullOrEmpty(error));

            var result = $"Error sending email: {error}, csid={chatSessionSkey}, vid={visitorId}.";
            return result;
        }

        private static Message BuildMessage(
            [NotNull] Visitor visitor,
            [NotNull] Dictionary<uint, UserInfo> agentMap,
            [NotNull] ChatSessionMessage old)
        {
            var result = new Message { TimestampUtc = old.TimestampUtc, Text = old.Text, SenderName = string.Empty, };
            switch (old.Sender)
            {
                case ChatMessageSender.Agent:
                    result.SenderClass = "agent";

                    Debug.Assert(old.SenderAgentId.HasValue && agentMap.ContainsKey(old.SenderAgentId.Value));
                    if (agentMap.TryGetValue(old.SenderAgentId.Value, out var a))
                        result.SenderName = a.FirstName + " " + a.LastName;
                    else
                    {
                        result.SenderName = "Unknown sender";
                        Debug.Fail(result.SenderName);
                    }

                    break;
                case ChatMessageSender.Visitor:
                    result.SenderClass = "visitor";
                    result.SenderName = "Visitor " + visitor.Name + " <" + visitor.Email + ">";
                    break;
                case ChatMessageSender.System:
                    result.SenderClass = "system";
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown {nameof(old.Sender)}={old.Sender}");
            }

            return result;
        }
    }
}