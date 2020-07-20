using System;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.Utils;
using Jil;

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    [ChatEventType(ChatEventType.VisitorRequestedTranscriptSent)]
    public class VisitorRequestedTranscriptSentChatEvent : ChatEventBase
    {
        public class EventArgs
        {
            public string Email { get; set; }

            [JilDirective(TreatEnumerationAs = typeof(int))]
            public CallResultStatusCode Status { get; set; }

            public string ErrorMessage { get; set; }

            public EventArgs(string email, CallResultStatusCode status, string errorMessage)
            {
                Email = email;
                Status = status;
                ErrorMessage = errorMessage;
            }

            public EventArgs()
            {
                Email = "";
                Status = CallResultStatusCode.Failure;
                ErrorMessage = null;
            }
        }

        public VisitorRequestedTranscriptSentChatEvent(
            DateTime timestampUtc,
            string email,
            CallResultStatusCode status,
            string errorMessage)
            : base(timestampUtc, new EventArgs(email, status, errorMessage).JsonStringify2())
        {
            Args = new EventArgs(email, status, errorMessage);
        }

        public VisitorRequestedTranscriptSentChatEvent(CHAT_EVENT dbo)
            : base(dbo)
        {
            Args = string.IsNullOrWhiteSpace(Text) ? new EventArgs() : Text.JsonUnstringify2<EventArgs>();
        }

        public EventArgs Args { get; }

        public override void Apply(ChatSession session, IObjectResolver resolver)
        {
            var message = Args.Status == CallResultStatusCode.Success
                ? "Transcript was successfully sent to " + Args.Email
                : $"Failed sending email to {Args.Email} ({Args.ErrorMessage})";

            session.AddSystemMessage(this, false, message);
        }

        public override void Notify(ChatSession chatSession, IObjectResolver resolver, ISubscriptionManager subscriptionManager)
        {
            var chatSessionInfo = chatSession.AsInfo();
            var messages = chatSession.EventMessagesAsInfo(Id);

            subscriptionManager.VisitorEventSubscribers.Publish(
                x => x.VisitorRequestedTranscriptSent(chatSessionInfo, messages));
        }
    }
}