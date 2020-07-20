using System.Collections.Generic;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Objects;

namespace Com.O2Bionics.ChatService.Impl
{
    public class ObjectResolver : IObjectResolver
    {
        private readonly IChatDatabaseFactory m_databaseFactory;
        private readonly IUserStorage m_userStorage;
        private readonly IUserManager m_userManager;
        private readonly IDepartmentStorage m_departmentStorage;
        private readonly IVisitorStorage m_visitorStorage;

        public ObjectResolver(
            IChatDatabaseFactory databaseFactory,
            IUserStorage userStorage,
            IUserManager userManager,
            IVisitorStorage visitorStorage,
            IDepartmentStorage departmentStorage)
        {
            m_databaseFactory = databaseFactory;
            m_userStorage = userStorage;
            m_userManager = userManager;
            m_visitorStorage = visitorStorage;
            m_departmentStorage = departmentStorage;
        }

        private Visitor GetVisitor(ulong id)
        {
            return m_visitorStorage.Get(id);
        }

        public string GetDepartmentName(uint customerId, uint id)
        {
            return m_databaseFactory.Query(
                db => m_departmentStorage.Get(db, customerId, id).Name);
        }

        public string GetAgentName(uint customerId, uint id)
        {
            return m_databaseFactory.Query(
                db => m_userStorage.Get(db, customerId, id).FullName());
        }

        public VisitorInfo GetVisitorInfo(ulong? id)
        {
            if (!id.HasValue) return null;
            var visitor = GetVisitor(id.Value);
            return visitor?.AsInfo();
        }

        public AgentInfo GetAgentInfo(uint customerId, uint? id)
        {
            return id.HasValue
                ? m_databaseFactory.Query(db => m_userManager.GetAgent(db, customerId, id.Value))
                : null;
        }

        public HashSet<uint> GetAgentDepartments(uint customerId, uint? id)
        {
            return id.HasValue
                ? m_databaseFactory.Query(db => m_userStorage.Get(db, customerId, id.Value)).AgentDepartmentIds
                : null;
        }

        public DepartmentInfo GetDepartmentInfo(uint customerId, uint? id)
        {
            return id.HasValue
                ? m_databaseFactory.Query(db => m_departmentStorage.Get(db, customerId, id.Value)).AsInfo()
                : null;
        }
    }
}