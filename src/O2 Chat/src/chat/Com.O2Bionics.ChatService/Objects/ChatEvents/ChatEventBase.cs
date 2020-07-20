using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.Utils;
using LinqToDB;
using LinqToDB.Data;

// ReSharper disable PossibleInvalidOperationException

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    [Serializable]
    public class ChatEventTypeAttribute : Attribute
    {
        public ChatEventTypeAttribute(ChatEventType eventType)
        {
            EventType = eventType;
        }

        public ChatEventType EventType { get; set; }
    }

    public abstract class ChatEventBase
    {
        protected ChatEventBase(DateTime timestampUtc, string text)
        {
            TimestampUtc = timestampUtc;
            Text = text;
        }

        protected ChatEventBase(CHAT_EVENT dbo)
        {
            Id = (long)dbo.CHAT_EVENT_ID;
            TimestampUtc = dbo.TIMESTAMP;
            Text = dbo.TEXT;
        }

        public long Id { get; private set; }
        public DateTime TimestampUtc { get; private set; }
        public string Text { get; private set; }

        public void AssignId(long id)
        {
            Id = id;
        }

        public virtual bool IsEndSessionEvent => false;

        public abstract void Apply(ChatSession session, IObjectResolver resolver);

        protected virtual void Save(CHAT_EVENT dbo)
        {
            dbo.CHAT_EVENT_ID = Id;
            dbo.CHAT_EVENT_TYPE_ID = (sbyte)EventType;
            dbo.TIMESTAMP = TimestampUtc;
            dbo.TEXT = Text;
        }

        public ChatEventType EventType => GetType().GetCustomAttribute<ChatEventTypeAttribute>().EventType;

        public void Save(ChatDatabase db, long sessionSkey)
        {
            var dbo = new CHAT_EVENT { CHAT_SESSION_ID = sessionSkey };
            Save(dbo);

            // manual query generation is used here because linq2db built queries use varchar parameter type
            // for nvarchar fields. this leads to loose of non-ascii characters.
            var pp = new List<DataParameter>();
            pp.Add(new DataParameter("CHAT_EVENT_ID", dbo.CHAT_EVENT_ID));
            pp.Add(new DataParameter("CHAT_SESSION_ID", dbo.CHAT_SESSION_ID));
            pp.Add(new DataParameter("CHAT_EVENT_TYPE_ID", dbo.CHAT_EVENT_TYPE_ID));
            pp.Add(new DataParameter("TIMESTAMP", dbo.TIMESTAMP));

            if (dbo.AGENT_ID.HasValue)
                pp.Add(new DataParameter("AGENT_ID", dbo.AGENT_ID.Value));
            if (dbo.TARGET_AGENT_ID.HasValue)
                pp.Add(new DataParameter("TARGET_AGENT_ID", dbo.TARGET_AGENT_ID.Value));
            if (dbo.TARGET_DEPARTMENT_ID.HasValue)
                pp.Add(new DataParameter("TARGET_DEPARTMENT_ID", dbo.TARGET_DEPARTMENT_ID.Value));
            if (dbo.IS_TO_AGENTS_ONLY.HasValue)
                pp.Add(new DataParameter("IS_TO_AGENTS_ONLY", dbo.IS_TO_AGENTS_ONLY.Value));
            if (dbo.IS_OFFLINE_SESSION.HasValue)
                pp.Add(new DataParameter("IS_OFFLINE_SESSION", dbo.IS_OFFLINE_SESSION.Value));
            if (dbo.ACT_ON_BEHALF_OF_INVITOR.HasValue)
                pp.Add(new DataParameter("ACT_ON_BEHALF_OF_INVITOR", dbo.ACT_ON_BEHALF_OF_INVITOR.Value));
            if (dbo.IS_DISCONNECTED.HasValue)
                pp.Add(new DataParameter("IS_DISCONNECTED", dbo.IS_DISCONNECTED.Value));
            if (dbo.IS_BECAME_OFFLINE.HasValue)
                pp.Add(new DataParameter("IS_BECAME_OFFLINE", dbo.IS_BECAME_OFFLINE.Value));
            if (!string.IsNullOrEmpty(dbo.TEXT))
                pp.Add(new DataParameter("TEXT", dbo.TEXT, DataType.NText)); // DataType.NVarChar is changed by linq2db to varchar

            var sql = string.Format(
                "insert into CHAT_EVENT ({0}) values ({1})",
                string.Join(",", pp.Select(x => x.Name)),
                string.Join(",", pp.Select(x => ":" + x.Name)));
            db.Execute(sql, pp.ToArray());
        }

        public static ChatEventBase Load(CHAT_EVENT dbo)
        {
            try
            {
                var type = (ChatEventType)dbo.CHAT_EVENT_TYPE_ID;
                return CreateInstance(type, dbo);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Failed ChatEvent initialization. session skey={0} id={1}: {2}",
                        dbo.CHAT_SESSION_ID,
                        dbo.CHAT_EVENT_ID,
                        e));
            }
        }

        private static ChatEventBase CreateInstance(ChatEventType eventType, CHAT_EVENT dbo)
        {
            Type type;
            if (!m_eventTypeToClassMap.TryGetValue(eventType, out type))
                throw new InvalidOperationException("Class not found for ChatEventType=" + eventType);
            return (ChatEventBase)Activator.CreateInstance(type, dbo);
        }

        #region m_eventTypeToClassMap

        private static readonly Dictionary<ChatEventType, Type> m_eventTypeToClassMap;

        static ChatEventBase()
        {
            var types = typeof(ChatEventBase).FindDerivedTypes()
                .Where(x => !x.IsAbstract)
                .ToList();
            var notMarkedTypes = types
                .Where(x => x.GetCustomAttribute<ChatEventTypeAttribute>(true) == null)
                .Select(x => x.Name)
                .ToList();
            if (notMarkedTypes.Any())
            {
                throw new InvalidOperationException(
                    "Some classes inherited from ChatEventBase has no ChatEventType Attributes specified: ["
                    + string.Join(", ", notMarkedTypes)
                    + "]");
            }

            var typesByEventType = types
                .ToLookup(x => x.GetCustomAttribute<ChatEventTypeAttribute>(true).EventType);
            var notDistinctTypes = typesByEventType.Where(x => x.Count() > 1).ToList();
            if (notDistinctTypes.Any())
                throw new InvalidOperationException(
                    "Same ChatEventTypeId for multiple types: ["
                    + string.Join(", ", notDistinctTypes.Select(x => "[" + x.Key + ": " + string.Join(", ", x.Select(y => y.Name)) + "]"))
                    + "]");
            m_eventTypeToClassMap = typesByEventType.ToDictionary(x => x.Key, x => x.First());
        }

        #endregion

        protected static bool AsBool(sbyte? value)
        {
            return value == 1;
        }

        protected static sbyte? AsSbyte(bool value)
        {
            return value ? (sbyte?)1 : 0;
        }

        public abstract void Notify(ChatSession chatSession, IObjectResolver resolver, ISubscriptionManager subscriptionManager);
    }
}