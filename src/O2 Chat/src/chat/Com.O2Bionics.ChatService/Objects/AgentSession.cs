using System;
using System.Linq;
using Com.O2Bionics.ChatService.DataModel;
using LinqToDB;

namespace Com.O2Bionics.ChatService.Objects
{
    public class AgentSession
    {
        public AgentSession(Guid guid, uint customerId, uint agentId, DateTime utcNow)
        {
            Guid = guid;
            CustomerId = customerId;
            AgentId = agentId;
            AddTimestampUtc = utcNow;

            LastAccessTimestampUtc = utcNow;
        }

        public AgentSession(AgentSession obj)
        {
            Guid = obj.Guid;
            CustomerId = obj.CustomerId;
            AgentId = obj.AgentId;
            AddTimestampUtc = obj.AddTimestampUtc;

            LastAccessTimestampUtc = obj.LastAccessTimestampUtc;
        }

        public AgentSession(AGENT_SESSION x)
        {
            Guid = new Guid(x.GUID);
            CustomerId = x.CUSTOMER_ID.Value;
            AgentId = x.AGENT_ID.Value;
            AddTimestampUtc = x.ADD_TIMESTAMP;

            LastAccessTimestampUtc = x.LAST_ACCESS_TIMESTAMP;
        }

        public Guid Guid { get; }
        public uint CustomerId { get; }
        public uint AgentId { get; }
        public DateTime AddTimestampUtc { get; }

        public DateTime LastAccessTimestampUtc { get; set; }

        public static void Insert(ChatDatabase db, AgentSession obj)
        {
            var dbo = new AGENT_SESSION
                {
                    GUID = obj.Guid.ToByteArray(),
                    CUSTOMER_ID = obj.CustomerId,
                    AGENT_ID = obj.AgentId,
                    ADD_TIMESTAMP = obj.AddTimestampUtc,
                    LAST_ACCESS_TIMESTAMP = obj.LastAccessTimestampUtc,
                };
            db.Insert(dbo);
        }

        public static void Update(ChatDatabase db, AgentSession original, AgentSession changed)
        {
            var update = db.AGENT_SESSION
                .Where(x => x.GUID == original.Guid.ToByteArray())
                .Set(x => x.LAST_ACCESS_TIMESTAMP, changed.LastAccessTimestampUtc);
            update.Update();
        }
    }
}