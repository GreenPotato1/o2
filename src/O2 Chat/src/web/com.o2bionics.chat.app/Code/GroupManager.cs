using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Com.O2Bionics.Utils;
using log4net;

namespace Com.O2Bionics.ChatService.Web.Console
{
    public static class GroupManager
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(GroupManager));

        private static readonly ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
        private static readonly Dictionary<string, HashSet<string>> m_groups = new Dictionary<string, HashSet<string>>();
        private static readonly Dictionary<string, Guid> m_agentSessions = new Dictionary<string, Guid>();

        public static void Add(Guid agentSessionGuid, string connectionId, IEnumerable<string> groupNames1, params string[] groupNames2)
        {
            var groupNames = new HashSet<string>();
            if (groupNames1 != null) foreach (var x in groupNames1) groupNames.Add(x);
            if (groupNames2 != null) foreach (var x in groupNames2) groupNames.Add(x);

            if (m_log.IsDebugEnabled)
                m_log.DebugFormat("add: {0} to {1}", connectionId, string.Join(", ", groupNames));

            m_lock.Write(
                () =>
                    {
                        foreach (var groupName in groupNames)
                        {
                            HashSet<string> connections;
                            if (m_groups.TryGetValue(groupName, out connections))
                                connections.Add(connectionId);
                            else
                                m_groups.Add(groupName, new HashSet<string> { connectionId });
                        }
                        m_agentSessions[connectionId] = agentSessionGuid;
                    });
        }

        public static void Remove(string connectionId)
        {
            if (m_log.IsDebugEnabled)
                m_log.DebugFormat("remove: {0}", connectionId);

            m_lock.Write(
                () =>
                    {
                        foreach (var x in m_groups.Values)
                            x.Remove(connectionId);
                        m_agentSessions.Remove(connectionId);
                    });
        }

        public static IList<string> GetConnections(IEnumerable<string> groupNames1, params string[] groupNames2)
        {
            var groupNames = new HashSet<string>();
            if (groupNames1 != null) foreach (var x in groupNames1) groupNames.Add(x);
            if (groupNames2 != null) foreach (var x in groupNames2) groupNames.Add(x);


            var list = m_lock.Read(
                () => m_groups
                    .Where(x => groupNames.Contains(x.Key))
                    .SelectMany(x => x.Value)
                    .Distinct()
                    .ToList());

            if (m_log.IsDebugEnabled)
                m_log.DebugFormat("get: ({0}) - {1}", string.Join(", ", groupNames), string.Join(", ", list));

            return list;
        }

        public static IList<string> GetConnections(params string[] groupNames)
        {
            return GetConnections(null, groupNames);
        }

        public static Dictionary<string, Guid> GetAgentSessionConnections()
        {
            return m_lock.Read(() => new Dictionary<string, Guid>(m_agentSessions));
        }
    }
}