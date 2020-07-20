using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Com.O2Bionics.AuditTrail.Contract;
using JetBrains.Annotations;
using pair = Com.O2Bionics.AuditTrail.Contract.IdName<uint>;

namespace Com.O2Bionics.AuditTrail.Client.Utilities
{
    public static class AuditEventFullTextChanges
    {
        [CanBeNull]
        public static string ChangedToString([CanBeNull] this FieldChanges fieldChanges)
        {
            if (null == fieldChanges)
                return null;

            //Omit BoolChanges.
            //Also, there no DateTimeChanges (Create and update timestamps are ignored).

            var builder = new StringBuilder();

            Decimals(fieldChanges.DecimalChanges, builder);
            Strings(fieldChanges.StringChanges, builder);
            IdLists(fieldChanges.IdListChanges, builder);
            StringLists(fieldChanges.StringListChanges, builder);

            var result = 0 == builder.Length ? null : builder.ToString();
            return result;
        }

        public static void AppendIfNotEmpty(this StringBuilder builder, [CanBeNull] string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            if (0 < builder.Length)
            {
                const char separator = ' ';
                builder.Append(separator);
            }

            builder.Append(value);
        }

        public static void AppendIfNotEmpty(this StringBuilder builder, [CanBeNull] IList<string> list)
        {
            if (null == list || 0 == list.Count)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < list.Count; i++)
                builder.AppendIfNotEmpty(list[i]);
        }

        private static void Decimals([CanBeNull] List<PlainFieldChange<decimal>> list, StringBuilder builder)
        {
            if (null == list)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < list.Count; i++)
            {
                builder.AppendIfNotEmpty(list[i].OldValue.ToString(CultureInfo.InvariantCulture));
                builder.AppendIfNotEmpty(list[i].NewValue.ToString(CultureInfo.InvariantCulture));
            }
        }

        private static void Strings([CanBeNull] List<PlainFieldChange<string>> list, StringBuilder builder)
        {
            if (null == list)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < list.Count; i++)
            {
                builder.AppendIfNotEmpty(list[i].OldValue);
                builder.AppendIfNotEmpty(list[i].NewValue);
            }
        }

        private static void IdLists(List<IdListChange> list, StringBuilder builder)
        {
            if (null == list)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < list.Count; i++)
            {
                IdList(list[i].Deleted, builder);
                IdList(list[i].Inserted, builder);
            }
        }

        private static void IdList([CanBeNull] List<pair> list, StringBuilder builder)
        {
            if (null == list)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < list.Count; i++)
                builder.AppendIfNotEmpty(list[i].Name);
        }

        private static void StringLists([CanBeNull] List<ListChange<string>> list, StringBuilder builder)
        {
            if (null == list)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < list.Count; i++)
            {
                StringList(list[i].Deleted, builder);
                StringList(list[i].Inserted, builder);
            }
        }

        private static void StringList([CanBeNull] List<string> list, StringBuilder builder)
        {
            if (null == list)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < list.Count; i++)
                builder.AppendIfNotEmpty(list[i]);
        }
    }
}