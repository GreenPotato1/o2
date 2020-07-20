using System;
using System.Collections.Generic;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;
using LinqToDB;
using LinqToDB.Data;

namespace Com.O2Bionics.ChatService.Objects
{
    public class Visitor
    {
        public Visitor(uint customerId, ulong id, DateTime timestampUtc)
        {
            CustomerId = customerId;
            Id = id;
            AddTimestampUtc = timestampUtc;
            UpdateTimestampUtc = timestampUtc;
        }

        public Visitor(Visitor x)
        {
            CustomerId = x.CustomerId;
            Id = x.Id;
            AddTimestampUtc = x.AddTimestampUtc;

            UpdateTimestampUtc = x.UpdateTimestampUtc;
            Name = x.Name;
            Email = x.Email;
            Phone = x.Phone;
            MediaSupport = x.MediaSupport;
            TranscriptMode = x.TranscriptMode;
        }

        public Visitor(VISITOR dbo)
        {
            CustomerId = dbo.CUSTOMER_ID.Value;
            Id = (ulong)dbo.VISITOR_ID;
            AddTimestampUtc = dbo.ADD_TIMESTAMP;

            UpdateTimestampUtc = dbo.UPDATE_TIMESTAMP;
            Name = dbo.NAME;
            Email = dbo.EMAIL;
            Phone = dbo.PHONE;
            MediaSupport = dbo.MEDIA_SUPPORT.HasValue ? (MediaSupport)dbo.MEDIA_SUPPORT.Value : (MediaSupport?)null;
            TranscriptMode = dbo.TRANSCRIPT_MODE.HasValue ? (VisitorSendTranscriptMode)dbo.TRANSCRIPT_MODE.Value : (VisitorSendTranscriptMode?)null;
        }

        public ulong Id { get; private set; }
        public uint CustomerId { get; private set; }
        public DateTime AddTimestampUtc { get; private set; }

        public DateTime UpdateTimestampUtc { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public MediaSupport? MediaSupport { get; set; }
        public VisitorSendTranscriptMode? TranscriptMode { get; set; }

        public VisitorInfo AsInfo()
        {
            return new VisitorInfo
                {
                    UniqueId = Id,
                    AddTimestampUtc = AddTimestampUtc,
                    Name = Name,
                    Email = Email,
                    Phone = Phone,
                    MediaSupport = MediaSupport ?? Contract.MediaSupport.NotSupported,
                    TranscriptMode = TranscriptMode,
                };
        }

        public static void Insert(ChatDatabase db, Visitor obj)
        {
            var dbo = new VISITOR
                {
                    CUSTOMER_ID = obj.CustomerId,
                    VISITOR_ID = obj.Id,
                    ADD_TIMESTAMP = obj.AddTimestampUtc,
                    UPDATE_TIMESTAMP = obj.UpdateTimestampUtc,
                    NAME = obj.Name,
                    EMAIL = obj.Email,
                    PHONE = obj.Phone,
                    MEDIA_SUPPORT =
                        obj.MediaSupport.HasValue ? (sbyte)obj.MediaSupport.Value : (sbyte?)null,
                    TRANSCRIPT_MODE =
                        obj.TranscriptMode.HasValue ? (sbyte)obj.TranscriptMode.Value : (sbyte?)null,
                };

            // manual query generation is used here because linq2db built queries use varchar parameter type
            // for nvarchar fields. this leads to loose of non-ascii characters.
            var pp = new List<DataParameter>
                {
                    new DataParameter("CUSTOMER_ID", dbo.CUSTOMER_ID),
                    new DataParameter("VISITOR_ID", dbo.VISITOR_ID),
                    new DataParameter("ADD_TIMESTAMP", dbo.ADD_TIMESTAMP),
                    new DataParameter("UPDATE_TIMESTAMP", dbo.UPDATE_TIMESTAMP),
                    new DataParameter("NAME", dbo.NAME, DataType.NText),
                    new DataParameter("EMAIL", dbo.EMAIL, DataType.NText),
                    new DataParameter("PHONE", dbo.PHONE),
                    new DataParameter("MEDIA_SUPPORT", dbo.MEDIA_SUPPORT, DataType.SByte),
                    new DataParameter("TRANSCRIPT_MODE", dbo.TRANSCRIPT_MODE),
                };

            var fields = string.Join(",", pp.Select(x => x.Name));
            var parameters = string.Join(",", pp.Select(x => ":" + x.Name));
            var sql = $"insert into VISITOR ({fields}) values ({parameters})";
            db.Execute(sql, pp.ToArray());
        }

        public static void Update(ChatDatabase db, Visitor original, Visitor changed)
        {
            var idParam = new DataParameter("ID", original.Id);

            var pp = new List<DataParameter> { new DataParameter("UPDATE_TIMESTAMP", changed.UpdateTimestampUtc) };

            if (original.Name != changed.Name)
                pp.Add(new DataParameter("NAME", changed.Name, DataType.NText));
            if (original.Email != changed.Email)
                pp.Add(new DataParameter("EMAIL", changed.Email, DataType.NText));
            if (original.Phone != changed.Phone)
                pp.Add(new DataParameter("PHONE", changed.Phone));
            if (original.MediaSupport != changed.MediaSupport)
                pp.Add(new DataParameter("MEDIA_SUPPORT", changed.MediaSupport.HasValue ? (sbyte?)changed.MediaSupport.Value : null));
            if (original.TranscriptMode != changed.TranscriptMode)
                pp.Add(new DataParameter("TRANSCRIPT_MODE", changed.TranscriptMode.HasValue ? (sbyte?)changed.TranscriptMode.Value : null));
            var sql = $"update VISITOR SET {string.Join(",", pp.Select(x => x.Name + "=:" + x.Name))} WHERE VISITOR_ID=:ID";
            db.Execute(sql, pp.Concat(new[] { idParam }).ToArray());
        }
    }
}