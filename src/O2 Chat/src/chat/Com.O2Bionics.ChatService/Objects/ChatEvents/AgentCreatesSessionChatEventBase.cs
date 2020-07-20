using System;
using Com.O2Bionics.ChatService.DataModel;

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    public abstract class AgentCreatesSessionChatEventBase : ChatEventBase
    {
        protected AgentCreatesSessionChatEventBase(DateTime timestampUtc, string text) : base(timestampUtc, text)
        {
        }

        protected AgentCreatesSessionChatEventBase(CHAT_EVENT dbo) : base(dbo)
        {
        }
    }
}