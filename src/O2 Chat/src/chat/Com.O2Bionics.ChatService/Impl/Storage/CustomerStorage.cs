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
    public class CustomerStorage : ICustomerStorage
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(CustomerStorage));

        private readonly ConcurrentDictionary<decimal, Customer> m_customers
            = new ConcurrentDictionary<decimal, Customer>();

        private readonly INowProvider m_nowProvider;

        public CustomerStorage(
            INowProvider nowProvider)
        {
            m_nowProvider = nowProvider;
        }

        public Customer Get(ChatDatabase db, uint id)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));

            if (m_customers.TryGetValue(id, out var c)) return c;

            c = Customer.Get(db, id);
            if (null != c)
                m_customers[id] = c;
            return c;
        }

        public Customer CreateNew(ChatDatabase db, Customer obj)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var messages = new Customer.Validator().ValidateNew(obj);
            if (messages.Any()) throw new ValidationException(messages);

            m_log.InfoFormat("creating new customer with name={0}, webSite={1}", obj.Name, obj.Domains);

            return Customer.Insert(db, m_nowProvider.UtcNow, obj);
        }

        public Customer Update(ChatDatabase db, uint id, Customer.UpdateInfo update)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (update == null) throw new ArgumentNullException(nameof(update));

            var messages = new Customer.Validator().ValidateUpdate(update);
            if (messages.Count > 0)
                throw new ValidationException(messages);

            m_log.InfoFormat("updating customer with id={0}", id);

            var updated = Customer.Update(db, DateTime.UtcNow, id, update);

            db.OnCommitActions.Add(
                () =>
                    {
                        Customer t;
                        m_customers.TryRemove(id, out t);
                    });

            return updated;
        }

        public List<uint> GetIds(ChatDatabase db)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));

            return db.CUSTOMERs
                .Where(x => x.STATUS_ID != (int)ObjectStatus.Deleted)
                .OrderBy(x => x.ID)
                .Select(x => x.ID)
                .ToList();
        }

        public List<uint> GetActiveIds(ChatDatabase db)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));

            return db.CUSTOMERs
                .Where(x => x.STATUS_ID == (int)ObjectStatus.Active)
                .Select(x => x.ID)
                .ToList();
        }
    }
}