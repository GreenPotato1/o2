using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Objects;
using Com.O2Bionics.Utils;
using log4net;

namespace Com.O2Bionics.ChatService.Impl.Storage
{
    public class DepartmentStorage : IDepartmentStorage
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(DepartmentStorage));

        private readonly ConcurrentDictionary<decimal, Dictionary<uint, Department>> m_customerDepartments
            = new ConcurrentDictionary<decimal, Dictionary<uint, Department>>();

        private readonly INowProvider m_nowProvider;

        public DepartmentStorage(
            INowProvider nowProvider)
        {
            m_nowProvider = nowProvider;
        }

        public Department Get(ChatDatabase db, uint customerId, uint departmentId)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));

            Dictionary<uint, Department> departments;
            if (!m_customerDepartments.TryGetValue(customerId, out departments))
            {
                departments = LoadAndCache(db, customerId);
            }

            Department department;
            return departments.TryGetValue(departmentId, out department) ? department : null;
        }

        public List<Department> GetAll(ChatDatabase db, uint customerId, bool skipPrivate)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));

            Dictionary<uint, Department> departments;
            if (!m_customerDepartments.TryGetValue(customerId, out departments))
            {
                departments = LoadAndCache(db, customerId);
            }

            var result = departments.Values.AsEnumerable();
            if (skipPrivate)
                result = result.Where(x => x.IsPublic);
            return result.ToList();
        }

        public HashSet<uint> GetPublicIds(ChatDatabase db, uint customerId, HashSet<uint> onlineDepartmentIds)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (onlineDepartmentIds == null) throw new ArgumentNullException(nameof(onlineDepartmentIds));

            Dictionary<uint, Department> departments;
            if (!m_customerDepartments.TryGetValue(customerId, out departments))
            {
                departments = LoadAndCache(db, customerId);
            }

            var result = departments.Values
                .Where(x => onlineDepartmentIds.Contains(x.Id) && x.IsPublic)
                .Select(x => x.Id);
            return new HashSet<uint>(result);
        }

        private Dictionary<uint, Department> LoadAndCache(ChatDatabase db, uint customerId)
        {
            var departments = Department.GetAll(db, customerId);
            if (departments.Count == 0)
            {
                Dictionary<uint, Department> t;
                m_customerDepartments.TryRemove(customerId, out t);
            }
            else
            {
                m_customerDepartments[customerId] = departments;
            }

            return departments;
        }


        public Department CreateNew(ChatDatabase db, Department department)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));
            if (department == null)
                throw new ArgumentNullException(nameof(department));

            var messages = new Department.Validator().ValidateNew(department);
            if (messages.Any()) throw new ValidationException(messages);

            if (GetAll(db, department.CustomerId, false).Any(x => x.Name == department.Name))
            {
                messages.Add(new ValidationMessage("name", "Other department already exists with provided name"));
                throw new ValidationException(messages);
            }

            m_log.DebugFormat("creating new department with customer={0}, name={1}", department.CustomerId, department.Name);

            var created = Department.Insert(db, m_nowProvider.UtcNow, department);

            {
                var customerId = department.CustomerId;
                db.OnCommitActions.Add(
                    () =>
                        {
                            Dictionary<uint, Department> t;
                            m_customerDepartments.TryRemove(customerId, out t);
                        });
            }

            return created;
        }

        public Department Update(ChatDatabase db, uint customerId, uint id, Department.UpdateInfo update)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));
            if (update == null)
                throw new ArgumentNullException(nameof(update));

            var messages = new Department.Validator().ValidateUpdate(update);
            if (messages.Count > 0)
                throw new ValidationException(messages);

            if (GetAll(db, customerId, false).Any(x => x.Name == update.Name && x.Id != id))
            {
                messages.Add(new ValidationMessage("name", "Other department already exists with provided name"));
                throw new ValidationException(messages);
            }

            var department = Get(db, customerId, id);
            if (department == null)
                throw new InvalidOperationException("Department not found by id=" + id);

            m_log.DebugFormat("updating department with id={0}, name={1}", id, department.Name);

            var updated = Department.Update(db, m_nowProvider.UtcNow, id, update);

            db.OnCommitActions.Add(
                () =>
                    {
                        Dictionary<uint, Department> t;
                        m_customerDepartments.TryRemove(customerId, out t);
                    });

            return updated;
        }
    }
}