using System;
using System.Collections.Generic;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.Utils;
using LinqToDB;
using LinqToDB.Data;
using Oracle.ManagedDataAccess.Client;

namespace Com.O2Bionics.ChatService.Objects
{
    public class Department
    {
        private static readonly string m_tableName = typeof(DEPARTMENT).Name;

        public uint Id { get; set; }
        public uint CustomerId { get; }
        public DateTime AddTimestampUtc { get; }
        public DateTime UpdateTimestampUtc { get; }

        public ObjectStatus Status { get; }
        public string Name { get; }
        public string Description { get; }
        public bool IsPublic { get; }

        public Department(uint customerId, string name, string description, bool isPublic)
        {
            CustomerId = customerId;
            AddTimestampUtc = DateTime.MinValue;
            UpdateTimestampUtc = DateTime.MinValue;

            Status = ObjectStatus.Active;
            Name = name;
            Description = description;
            IsPublic = isPublic;
        }

        private Department(DEPARTMENT x)
        {
            Id = x.ID;
            CustomerId = x.CUSTOMER_ID.Value;
            AddTimestampUtc = x.ADD_TIMESTAMP;
            UpdateTimestampUtc = x.UPDATE_TIMESTAMP;

            Status = (ObjectStatus)x.STATUS_ID;
            Name = x.NAME;
            Description = x.DESCRIPTION;
            IsPublic = x.IS_PUBLIC != 0;
        }

        public DepartmentInfo AsInfo()
        {
            return new DepartmentInfo
                {
                    Id = Id,
                    CustomerId = CustomerId,
                    Status = Status,
                    IsPublic = IsPublic,
                    Name = Name,
                    Description = Description,
                };
        }

        public static Dictionary<uint, Department> GetAll(ChatDatabase db, uint customerId)
        {
            return db.DEPARTMENTs
                .Where(x => x.CUSTOMER_ID == customerId && x.STATUS_ID != (int)ObjectStatus.Deleted)
                .Select(x => new Department(x))
                .ToDictionary(x => x.Id);
        }

        private static Department Get(ChatDatabase db, uint id)
        {
            return db.DEPARTMENTs
                .Where(x => x.ID == id && x.STATUS_ID != (int)ObjectStatus.Deleted)
                .Select(x => new Department(x))
                .FirstOrDefault();
        }

        public static Department Insert(ChatDatabase db, DateTime utcNow, Department obj)
        {
            // manual query generation is used here because linq2db built queries use varchar parameter type
            // for nvarchar fields. this leads to loose of non-ascii characters.

            var newId = EntityCreateUtility.GenerateId();
            var pp = new List<DataParameter>
                {
                    new DataParameter("ID", newId),
                    new DataParameter("CUSTOMER_ID", obj.CustomerId),
                    new DataParameter("STATUS_ID", (int)obj.Status),
                    new DataParameter("IS_PUBLIC", obj.IsPublic ? 1 : 0),
                    new DataParameter("NAME", obj.Name, DataType.NText),
                    new DataParameter("DESCRIPTION", obj.Description, DataType.NText),
                    new DataParameter("ADD_TIMESTAMP", utcNow),
                    new DataParameter("UPDATE_TIMESTAMP", utcNow),
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
                    }
                }
            }

            return Get(db, newId);
        }

        public static Department Update(ChatDatabase db, DateTime utcNow, uint id, UpdateInfo update)
        {
            var idParam = new DataParameter("ID", id);

            var pp = new List<DataParameter> { new DataParameter("UPDATE_TIMESTAMP", utcNow) };

            if (update.Status.HasValue)
                pp.Add(new DataParameter("STATUS_ID", (int)update.Status.Value));
            if (update.IsPublic.HasValue)
                pp.Add(new DataParameter("IS_PUBLIC", update.IsPublic.Value ? 1 : 0));
            if (update.Name != null)
                pp.Add(new DataParameter("NAME", update.Name, DataType.NText));
            if (update.Description != null)
                pp.Add(new DataParameter("DESCRIPTION", update.Description, DataType.NText));

            var fieldUpdate = string.Join(",", pp.Select(x => x.Name + "=:" + x.Name));
            var sql = $"update {m_tableName} SET {fieldUpdate} WHERE ID=:ID";
            db.Execute(sql, pp.Concat(new[] { idParam }).ToArray());

            return Get(db, id);
        }

        public class Validator : BusinessObjectValidatorBase
        {
            private const int NameMaxLength = 32;
            private const int DescriptionMaxLength = 256;

            public List<ValidationMessage> ValidateNew(Department department)
            {
                if (department == null) throw new ArgumentNullException(nameof(department));

                var messages = new List<ValidationMessage>();

                ValidateStringField(messages, "name", department.Name, false, NameMaxLength, false);
                ValidateStringField(messages, "description", department.Description, true, DescriptionMaxLength, true);

                return messages;
            }

            public List<ValidationMessage> ValidateUpdate(UpdateInfo update)
            {
                if (update == null) throw new ArgumentNullException(nameof(update));

                var messages = new List<ValidationMessage>();

                ValidateStringField(messages, "name", update.Name, true, NameMaxLength, false);
                ValidateStringField(messages, "description", update.Description, true, DescriptionMaxLength, true);

                return messages;
            }
        }

        public class UpdateInfo
        {
            public ObjectStatus? Status { get; set; }
            public bool? IsPublic { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }
    }
}