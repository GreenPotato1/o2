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
    public class CannedMessage
    {
        private static readonly string m_tableName = typeof(CANNED_MESSAGE).Name;

        public CannedMessage(
            uint? userId,
            uint? departmentId,
            string messageKey,
            string messageValue)
        {
            AddTimestampUtc = DateTime.MinValue;
            UpdateTimestampUtc = DateTime.MinValue;
            UserId = userId;
            DepartmentId = departmentId;
            MessageKey = messageKey;
            MessageValue = messageValue;
        }

        public CannedMessage(CANNED_MESSAGE dbo)
        {
            Id = dbo.ID;
            AddTimestampUtc = dbo.CREATE_TIMESTAMP;
            UpdateTimestampUtc = dbo.UPDATE_TIMESTAMP;
            MessageKey = dbo.KEY;
            MessageValue = dbo.VALUE;
            UserId = dbo.USER_ID;
            DepartmentId = dbo.DEPARTMENT_ID;
        }

        public uint Id { get; }
        public uint? UserId { get; }
        public uint? DepartmentId { get; }
        public DateTime AddTimestampUtc { get; }
        public DateTime UpdateTimestampUtc { get; }
        public string MessageKey { get; }
        public string MessageValue { get; }


        public CannedMessageInfo AsInfo()
        {
            return new CannedMessageInfo
                {
                    Id = Id,
                    AddTimestampUtc = AddTimestampUtc,
                    UpdateTimestampUtc = UpdateTimestampUtc,
                    Key = MessageKey,
                    Value = MessageValue,
                    UserId = UserId,
                    DepartmentId = DepartmentId
                };
        }

        public static CannedMessage Get(ChatDatabase db, uint id)
        {
            var dbo = db.CANNED_MESSAGE.FirstOrDefault(x => x.ID == id);
            return dbo == null ? null : new CannedMessage(dbo);
        }

        public static List<CannedMessage> GetCustomerData(ChatDatabase db, uint customerId)
        {
            return db.CANNED_MESSAGE
                .Where(x => x.CUSTOMER_ID == customerId)
                .Select(x => new CannedMessage(x))
                .ToList();
        }

        public static CannedMessage Insert(ChatDatabase db, DateTime utcNow, uint customerId, CannedMessage obj)
        {
            // manual query generation is used here because linq2db built queries use varchar parameter type
            // for nvarchar fields. this leads to loose of non-ascii characters.

            var newId = EntityCreateUtility.GenerateId();
            var pp = new List<DataParameter>
                {
                    new DataParameter("ID", newId),
                    new DataParameter("KEY", obj.MessageKey, DataType.NText),
                    new DataParameter("VALUE", obj.MessageValue, DataType.NText),
                    new DataParameter("CREATE_TIMESTAMP", utcNow),
                    new DataParameter("UPDATE_TIMESTAMP", utcNow),
                    new DataParameter("CUSTOMER_ID", customerId),
                    new DataParameter("DEPARTMENT_ID", obj.DepartmentId),
                    new DataParameter("USER_ID", obj.UserId),
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

        public static CannedMessage Update(ChatDatabase db, DateTime utcNow, uint id, UpdateInfo update)
        {
            var idParam = new DataParameter("ID", id);
            var pp = new List<DataParameter> { new DataParameter("UPDATE_TIMESTAMP", utcNow) };
            if (update.MessageKey != null)
                pp.Add(new DataParameter("KEY", update.MessageKey, DataType.NText));
            if (update.MessageValue != null)
                pp.Add(new DataParameter("VALUE", update.MessageValue, DataType.NText));

            var fieldUpdate = string.Join(",", pp.Select(x => x.Name + "=:" + x.Name));
            var sql = $"update {m_tableName} SET {fieldUpdate} WHERE ID=:ID";
            db.Execute(sql, pp.Concat(new[] { idParam }).ToArray());
            return Get(db, id);
        }

        public static void Delete(ChatDatabase db, uint id)
        {
            db.CANNED_MESSAGE.Delete(x => x.ID == id);
        }

        public class UpdateInfo
        {
            public string MessageKey { get; set; }
            public string MessageValue { get; set; }
        }

        public class Validator : BusinessObjectValidatorBase
        {
            private const int MessageKeyLength = 50;
            private const int MessageValueLength = 1999;

            public List<ValidationMessage> ValidateNew(CannedMessage cannedMessage)
            {
                if (cannedMessage == null) throw new ArgumentNullException(nameof(cannedMessage));

                if (!(cannedMessage.DepartmentId == null ^ cannedMessage.UserId == null))
                {
                    throw new ArgumentException("DepartmentId, UserId both fields can not be empty or filled");
                }

                var messages = new List<ValidationMessage>();
                ValidateStringField(messages, "MessageKey", cannedMessage.MessageKey, true, MessageKeyLength, false);
                ValidateStringField(messages, "MessageValue", cannedMessage.MessageValue, true, MessageValueLength, true);

                return messages;
            }

            public List<ValidationMessage> ValidateUpdate(UpdateInfo update)
            {
                if (update == null) throw new ArgumentNullException(nameof(update));

                var messages = new List<ValidationMessage>();
                ValidateStringField(messages, "MessageKey", update.MessageKey, true, MessageKeyLength, false);
                ValidateStringField(messages, "MessageValue", update.MessageValue, true, MessageValueLength, true);

                return messages;
            }
        }
    }
}