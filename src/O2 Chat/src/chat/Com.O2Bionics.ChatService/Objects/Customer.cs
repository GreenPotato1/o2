using System;
using System.Collections.Generic;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;
using LinqToDB;
using LinqToDB.Data;
using Oracle.ManagedDataAccess.Client;

namespace Com.O2Bionics.ChatService.Objects
{
    public class Customer
    {
        private static readonly string m_tableName = typeof(CUSTOMER).Name;

        public uint Id { get; }

        public DateTime AddTimestampUtc { get; }
        public DateTime UpdateTimestampUtc { get; }

        public ObjectStatus Status { get; }
        public string Name { get; }

        /// <summary>
        /// Separated with <seealso cref="DomainUtilities.DomainSeparator"/>.
        /// </summary>
        public string Domains { get; }

        public string CreateIp { get; }

        public Customer(
            ObjectStatus status,
            string name,
            string domains,
            string createIp)
        {
            AddTimestampUtc = DateTime.MinValue;
            UpdateTimestampUtc = DateTime.MinValue;

            Status = status;
            Name = name;
            Domains = domains;
            CreateIp = createIp;
        }

        private Customer(CUSTOMER c)
        {
            Id = c.ID;
            AddTimestampUtc = c.CREATE_TIMESTAMP;
            UpdateTimestampUtc = c.UPDATE_TIMESTAMP;
            Status = (ObjectStatus)c.STATUS_ID;
            Name = c.NAME;
            Domains = c.DOMAINS;
            CreateIp = c.CREATE_IP;
        }

        public CustomerInfo AsInfo()
        {
            var domains = DomainUtilities.GetDomains(Domains);

            return new CustomerInfo
                {
                    Id = Id,
                    AddTimestampUtc = AddTimestampUtc,
                    UpdateTimestampUtc = UpdateTimestampUtc,
                    Status = Status,
                    Name = Name,
                    Domains = domains,
                    CreateIp = CreateIp
                };
        }

        [CanBeNull]
        public static Customer Get(ChatDatabase db, uint id)
        {
            return db.CUSTOMERs
                .Where(c => c.ID == id && c.STATUS_ID != (int)ObjectStatus.Deleted)
                .Select(c => new Customer(c))
                .FirstOrDefault();
        }

        public static Customer Insert(ChatDatabase db, DateTime utcNow, Customer obj)
        {
            var newId = EntityCreateUtility.GenerateId();
            var pp = new List<DataParameter>
                {
                    new DataParameter("ID", newId),
                    new DataParameter("PARENT_USERID", DBNull.Value),
                    new DataParameter("CREATE_TIMESTAMP", utcNow),
                    new DataParameter("UPDATE_TIMESTAMP", utcNow),
                    new DataParameter("STATUS_ID", (int)obj.Status),
                    new DataParameter("NAME", obj.Name, DataType.NText),
                    new DataParameter("DOMAINS", obj.Domains, DataType.NText),
                    new DataParameter("CREATE_IP", obj.CreateIp),
                };

            var fieldNames = string.Join(",", pp.Select(x => x.Name));
            var fieldValues = string.Join(",", pp.Select(x => ":" + x.Name));
            var sql = $"insert into {m_tableName} ({fieldNames}) values ({fieldValues})";

            for (var i = 0; i < EntityCreateUtility.InsertAttemptsLimit; i++)
            {
                try
                {
                    db.Execute(sql, pp.ToArray());
                    break;
                }
                catch (OracleException ex)
                {
                    //PK error. try one more time
                    if (ex.Number == 1 && ex.Message.Contains($"PK_{m_tableName}"))
                    {
                        if (i == EntityCreateUtility.InsertAttemptsLimit - 1)
                            throw;

                        continue;
                    }
                }
            }

            return Get(db, newId);
        }

        public static Customer Update(ChatDatabase db, DateTime utcNow, uint id, UpdateInfo update)
        {
            var idParam = new DataParameter("ID", id);

            var pp = new List<DataParameter> { new DataParameter("UPDATE_TIMESTAMP", utcNow) };

            if (update.Status.HasValue)
                pp.Add(new DataParameter("STATUS_ID", (int)update.Status.Value));
            if (update.Name != null)
                pp.Add(new DataParameter("NAME", update.Name, DataType.NText));
            if (update.Domains != null)
                pp.Add(new DataParameter("DOMAINS", update.Domains, DataType.NText));

            var setFields = string.Join(",", pp.Select(x => x.Name + "=:" + x.Name));
            var sql = $"update {m_tableName} SET {setFields} WHERE ID=:ID";
            db.Execute(sql, pp.Concat(new[] { idParam }).ToArray());

            return Get(db, id);
        }

        public class Validator : BusinessObjectValidatorBase
        {
            private const int NameMaxLength = 200;
            private const int CreateIpMaxLength = 46;

            public List<ValidationMessage> ValidateNew(Customer x)
            {
                if (x == null) throw new ArgumentNullException(nameof(x));

                var messages = new List<ValidationMessage>();

                ValidateStringField(messages, "name", x.Name, false, NameMaxLength, false);
                ValidateStringField(messages, "domains", x.Domains, false, int.MaxValue, false);
                ValidateStringField(messages, "createIp", x.CreateIp, true, CreateIpMaxLength, true);

                return messages;
            }

            public List<ValidationMessage> ValidateUpdate(UpdateInfo update)
            {
                if (update == null) throw new ArgumentNullException(nameof(update));

                var messages = new List<ValidationMessage>();

                ValidateStringField(messages, "name", update.Name, true, NameMaxLength, false);
                ValidateStringField(messages, "domains", update.Domains, true, int.MaxValue, false);

                return messages;
            }
        }

        public class UpdateInfo
        {
            public ObjectStatus? Status { get; set; }
            public string Name { get; set; }
            public string Domains { get; set; }
        }
    }
}