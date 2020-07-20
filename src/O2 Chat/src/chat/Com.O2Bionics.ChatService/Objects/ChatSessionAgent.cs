using Com.O2Bionics.ChatService.Contract;

namespace Com.O2Bionics.ChatService.Objects
{
    public class ChatSessionAgent
    {
        public ChatSessionAgent(uint agentId, uint? actsOnBehalfOfAgentId = null)
        {
            AgentId = agentId;
            ActsOnBehalfOfAgentId = actsOnBehalfOfAgentId;
        }

        public uint AgentId { get; private set; }
        public uint? ActsOnBehalfOfAgentId { get; private set; }

        public ChatSessionAgentInfo AsInfo()
        {
            return new ChatSessionAgentInfo
                {
                    AgentId = AgentId,
                    ActsOnBehalfOfAgentId = ActsOnBehalfOfAgentId,
                };
        }
    }
}