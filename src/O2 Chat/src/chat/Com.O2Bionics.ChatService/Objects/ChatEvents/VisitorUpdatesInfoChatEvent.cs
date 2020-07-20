using System;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.Utils;
using Jil;

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    [ChatEventType(ChatEventType.VisitorUpdatesInfo)]
    public class VisitorUpdatesInfoChatEvent : ChatEventBase
    {
        public class EventArgs
        {
            public bool WasRemoved { get; set; }
            public string NewName { get; set; }
            public string NewEmail { get; set; }
            public string NewPhone { get; set; }

            [JilDirective(TreatEnumerationAs = typeof(int))]
            public VisitorSendTranscriptMode? NewTranscriptMode { get; set; }

            public EventArgs(bool wasRemoved, string newName, string newEmail, string newPhone, VisitorSendTranscriptMode? newTranscriptMode)
            {
                WasRemoved = wasRemoved;
                NewName = newName;
                NewEmail = newEmail;
                NewPhone = newPhone;
                NewTranscriptMode = newTranscriptMode;
            }

            public EventArgs()
            {
                WasRemoved = false;
                NewName = null;
                NewEmail = null;
                NewPhone = null;
                NewTranscriptMode = null;
            }
        }

        public VisitorUpdatesInfoChatEvent(
            DateTime timestampUtc,
            bool wasRemoved,
            string newName = null,
            string newEmail = null,
            string newPhone = null,
            VisitorSendTranscriptMode? newTranscriptMode = null)
            : base(timestampUtc, new EventArgs(wasRemoved, newName, newEmail, newPhone, newTranscriptMode).JsonStringify2())
        {
            Args = new EventArgs(wasRemoved, newName, newEmail, newPhone, newTranscriptMode);
        }

        public VisitorUpdatesInfoChatEvent(CHAT_EVENT dbo)
            : base(dbo)
        {
            Args = string.IsNullOrWhiteSpace(Text) ? new EventArgs() : Text.JsonUnstringify2<EventArgs>();
        }

        public EventArgs Args { get; }

        public override void Apply(ChatSession session, IObjectResolver resolver)
        {
            if (Args.WasRemoved)
            {
                session.AddSystemMessage(this, true, "Visitor has removed visitor information");
            }
            else
            {
                if (Args.NewName != null || Args.NewEmail != null || Args.NewPhone != null)
                {
                    var message = string.Join(
                        ", ",
                        new[]
                                {
                                    Args.NewName != null ? "name to be " + Args.NewName : null,
                                    Args.NewEmail != null ? "email to be " + Args.NewEmail : null,
                                    Args.NewPhone != null ? "phone to be " + Args.NewPhone : null,
                                    Args.NewTranscriptMode != null ? "transcriptMode to be " + Args.NewTranscriptMode.Value.ToString("G") : null,
                                }
                            .Where(x => x != null));

                    session.AddSystemMessage(this, true, "Visitor has changed " + message);
                }
            }
        }

        public override void Notify(ChatSession chatSession, IObjectResolver resolver, ISubscriptionManager subscriptionManager)
        {
            var visitorId = chatSession.VisitorId;
            if (visitorId.HasValue)
            {
                var messages = chatSession.EventMessagesAsInfo(Id);

                subscriptionManager.VisitorEventSubscribers
                    .Publish(
                        x => x.VisitorInfoChanged(
                            visitorId.Value,
                            Args.WasRemoved,
                            Args.NewName,
                            Args.NewEmail,
                            Args.NewPhone,
                            Args.NewTranscriptMode));
                subscriptionManager.AgentEventSubscribers
                    .Publish(
                        x =>
                            x.VisitorInfoChanged(
                                chatSession.CustomerId,
                                visitorId.Value,
                                Args.WasRemoved,
                                Args.NewName,
                                Args.NewEmail,
                                Args.NewPhone,
                                Args.NewTranscriptMode,
                                chatSession.Skey,
                                messages));
            }
        }
    }
}