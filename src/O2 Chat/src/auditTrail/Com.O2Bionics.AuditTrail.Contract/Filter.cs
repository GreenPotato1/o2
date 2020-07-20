using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Com.O2Bionics.AuditTrail.Contract.Properties;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;

namespace Com.O2Bionics.AuditTrail.Contract
{
    [DataContract]
    public sealed class Filter
    {
        private const int MaxPageSize = 1 << 15;

        public Filter()
        {
        }

        public Filter(
            [NotNull] string productCode,
            //If zero - do not fetch the documents.
            int pageSize = 0,
            int fromRow = 0,
            string substring = null)
        {
            ProductCode = productCode;
            PageSize = pageSize;
            FromRow = fromRow;
            Substring = substring;
        }

        /// <summary>
        ///     Copy constructor.
        /// </summary>
        public Filter([NotNull] Filter value)
        {
            if (null == value)
                throw new ArgumentNullException(nameof(value));

            ProductCode = value.ProductCode;
            PageSize = value.PageSize;
            FromRow = value.FromRow;
            CustomerId = value.CustomerId;
            Operations = value.Operations;
            Statuses = value.Statuses;
            AuthorIds = value.AuthorIds;
            FromTime = value.FromTime;
            ToTime = value.ToTime;
            ChangedOnly = value.ChangedOnly;
            Substring = value.Substring;
            SearchPosition = value.SearchPosition;
        }

        [DataMember(IsRequired = true)]
        public string ProductCode { get; set; }

        /// <summary>
        ///     How many rows to select.
        /// </summary>
        [DataMember(IsRequired = true)]
        public int PageSize { get; set; }

        /// <summary>
        ///     Start from zero.
        /// </summary>
        [DataMember]
        public int FromRow { get; set; }

        /// <summary>
        ///     Whether to include only the changed fields to filter by <see cref="Substring" />.
        ///     When false, the full text search is performed against all fields.
        ///     Note. For deleted and inserted documents, "AuditEvent.Changed" == "AuditEvent.All".
        /// </summary>
        [DataMember]
        public bool ChangedOnly { get; set; }

        /// <summary>
        ///     Used for full-text search in all fields.
        /// </summary>
        [DataMember]
        public string Substring { get; set; }

        [DataMember]
        public string CustomerId { get; set; }

        [DataMember]
        public List<string> Operations { get; set; }

        [DataMember]
        public List<string> Statuses { get; set; }

        /// <summary>
        /// If not null, it must have the values of the sorted fields,
        /// which are <seealso cref="AuditEvent{T}.Timestamp"/> and <seealso cref="AuditEvent{T}.Id"/>. 
        /// </summary>
        [DataMember]
        [CanBeNull]
        public SearchPositionInfo SearchPosition { get; set; }

        /// <summary>
        ///     S keys of the users who made changes.
        /// </summary>
        [DataMember]
        public List<string> AuthorIds { get; set; }

        public string Validate(bool throwOnError = true)
        {
            if (string.IsNullOrEmpty(ProductCode))
            {
                var error = string.Format(Resources.EmptyStringError1, nameof(ProductCode));
                if (throwOnError)
                    throw new Exception(error);
                return error;
            }

            if (PageSize < 0)
            {
                var error = string.Format(Resources.MustBeNonNegative2, nameof(PageSize), PageSize);
                if (throwOnError)
                    throw new Exception(error);
                return error;
            }

            if (MaxPageSize < PageSize)
            {
                var error = string.Format(Resources.PageSizeCannotExceed2, PageSize, MaxPageSize);
                if (throwOnError)
                    throw new Exception(error);
                return error;
            }

            if (FromRow < 0)
            {
                var error = string.Format(Resources.MustBeNonNegative2, nameof(FromRow), FromRow);
                if (throwOnError)
                    throw new Exception(error);
                return error;
            }

            if (ToTime < FromTime)
            {
                var error = string.Format(Resources.FromGreaterThanToTimestampError2, FromTime, ToTime);
                if (throwOnError)
                    throw new Exception(error);
                return error;
            }

            if (null != SearchPosition)
            {
                if (0 != FromRow)
                {
                    var error = string.Format(Resources.SearchAfterFromRowError1, FromRow);
                    if (throwOnError)
                        throw new Exception(error);
                    return error;
                }
            }

            return null;
        }

        public void SetDates(bool clearAfterSet = false)
        {
            if (!string.IsNullOrEmpty(FromTimeStr))
                FromTime = DateUtilities.ParseDate(FromTimeStr);

            if (!string.IsNullOrEmpty(ToTimeStr))
                ToTime = DateUtilities.ParseDate(ToTimeStr).AddDays(1);

            if (clearAfterSet)
                FromTimeStr = ToTimeStr = null;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("Product=").Append(ProductCode);
            builder.Append(", PageSize=").Append(PageSize);
            builder.Append(", FromRow=").Append(FromRow);
            builder.Append(", Customer=").Append(CustomerId);
            builder.Append(", FromTime=").Append(FromTime);
            builder.Append(", ToTime=").Append(ToTime);
            AppendIfAny(builder, nameof(Operations), Operations);
            AppendIfAny(builder, nameof(Statuses), Statuses);
            AppendIfAny(builder, nameof(AuthorIds), AuthorIds);
            builder.Append(", ChangedOnly=").Append(ChangedOnly);
            builder.Append(", Substring='").Append(Substring).Append('\'');
            builder.Append(", SearchPosition=").Append(SearchPosition);

            var result = builder.ToString();
            return result;
        }

        private static void AppendIfAny<T>(
            [NotNull] StringBuilder builder,
            [NotNull] string name,
            [CanBeNull] IList<T> values,
            string nameSeparator = ", ",
            string valueSeparator = ", ")
        {
            if (null == values || 0 == values.Count)
                return;

            if (0 < builder.Length)
                builder.Append(nameSeparator);

            builder.Append(name).Append("='");
            for (var i = 0; i < values.Count; i++)
            {
                if (0 < i)
                    builder.Append(valueSeparator);
                builder.Append(values[i]);
            }

            builder.Append('\'');
        }

        #region Time range

        [DataMember]
        public DateTime FromTime { get; set; }

        [DataMember]
        public DateTime ToTime { get; set; }

        [DataMember]
        public string FromTimeStr { get; set; }

        [DataMember]
        public string ToTimeStr { get; set; }

        #endregion
    }
}