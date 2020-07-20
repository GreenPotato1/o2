using System;
using System.Collections.Generic;
using Com.O2Bionics.AuditTrail.Contract;
using JetBrains.Annotations;
using pair = Com.O2Bionics.AuditTrail.Contract.IdName<uint>;

namespace Com.O2Bionics.AuditTrail.Client.Utilities
{
    public static class DifferenceUtils
    {
        /// <summary>
        ///     Primitive types (decimal, int, time).
        ///     The string and <see cref="Enum" /> fields have an overload.
        /// </summary>
        public static void AddIfDifferent<T>(
            T oldValue,
            T newValue,
            [NotNull] string fieldName,
            [CanBeNull] ref List<PlainFieldChange<T>> list)
            where T : struct, IEquatable<T>
        {
#if DEBUG
            if (typeof(T).IsEnum)
                throw new Exception($"The type '{typeof(T).FullName}' cannot be an enumeration.");
#endif
            if (!oldValue.Equals(newValue))
                AddChange(oldValue, newValue, fieldName, ref list);
        }

        /// <summary>
        ///     Only for enumerations.
        /// </summary>
        public static void AddIfDifferent<T>(
            T oldValue,
            T newValue,
            [NotNull] string fieldName,
            [CanBeNull] ref List<PlainFieldChange<string>> list)
            where T : struct
        {
#if DEBUG
            if (!typeof(T).IsEnum)
                throw new Exception($"The type '{typeof(T).FullName}' must be an enumeration.");
#endif
            long Cast(T value)
            {
                return Convert.ToInt64(value);
            }

            long a = Cast(oldValue), b = Cast(newValue);
            if (a != b)
                AddChange(oldValue.ToString(), newValue.ToString(), fieldName, ref list);
        }

        public static void AddIfDifferent(
            [CanBeNull] string oldValue,
            [CanBeNull] string newValue,
            [NotNull] string fieldName,
            [CanBeNull] ref List<PlainFieldChange<string>> list)
        {
            if (ReferenceEquals(oldValue, newValue))
                return;

            var changed = null == oldValue || !oldValue.Equals(newValue);
            if (changed)
                AddChange(oldValue, newValue, fieldName, ref list);
        }

        /// <summary>
        ///     List of ids - changes are stored as a pair of (id, name).
        /// </summary>
        public static void AddIfDifferent(
            [NotNull] INameResolver nameResolver,
            uint customerId,
            [CanBeNull] HashSet<uint> oldValue,
            [CanBeNull] HashSet<uint> newValue,
            [NotNull] string fieldName,
            [CanBeNull] ref List<IdListChange> list)
        {
            if (ReferenceEquals(oldValue, newValue))
                return;

            var emptyOld = null == oldValue || 0 == oldValue.Count;
            var emptyNew = null == newValue || 0 == newValue.Count;
            if (emptyOld && emptyNew)
                return;

            var changes = new IdListChange();
            if (emptyOld)
            {
                //Only new.
                AddChanges(nameResolver, customerId, newValue, out var inserted);
                changes.Inserted = inserted;
            }
            else if (emptyNew)
            {
                //Only old.
                AddChanges(nameResolver, customerId, oldValue, out var deleted);
                changes.Deleted = deleted;
            }
            else
            {
                //Both exist.
                AddChangesIfMissing(nameResolver, customerId, oldValue, newValue, out var deleted);
                changes.Deleted = deleted;
                AddChangesIfMissing(nameResolver, customerId, newValue, oldValue, out var inserted);
                changes.Inserted = inserted;
            }

            if (null != changes.Deleted || null != changes.Inserted)
                AddChange(changes, fieldName, ref list);
        }

        public static void AddIfDifferent<T>(
            [CanBeNull] ICollection<T> oldValue,
            [CanBeNull] ICollection<T> newValue,
            [NotNull] string fieldName,
            [CanBeNull] ref List<ListChange<T>> list)
        {
            if (ReferenceEquals(oldValue, newValue))
                return;

            var emptyOld = null == oldValue || 0 == oldValue.Count;
            var emptyNew = null == newValue || 0 == newValue.Count;
            if (emptyOld && emptyNew)
                return;

            var changes = new ListChange<T>();
            if (emptyOld)
                //Only new.
            {
                changes.Inserted = new List<T>(newValue);
            }
            else if (emptyNew)
                //Only old.
            {
                changes.Deleted = new List<T>(oldValue);
            }
            else
            {
                //Both exist.
                AddChangesIfMissing(oldValue, newValue, out var deleted);
                changes.Deleted = deleted;

                AddChangesIfMissing(newValue, oldValue, out var inserted);
                changes.Inserted = inserted;
            }

            if (null != changes.Deleted || null != changes.Inserted)
                AddChange(changes, fieldName, ref list);
        }

        private static void AddChange<T>(
            T oldValue,
            T newValue,
            string fieldName,
            ref List<PlainFieldChange<T>> list)
        {
            if (null == list)
                list = new List<PlainFieldChange<T>>();
            list.Add(new PlainFieldChange<T>(fieldName, oldValue, newValue));
        }

        private static void AddChange<T>(
            [NotNull] T change,
            [NotNull] string fieldName,
            [CanBeNull] ref List<T> list)
            where T : INamed
        {
            if (null == list)
                list = new List<T>();

            change.Name = fieldName;
            list.Add(change);
        }

        private static void AddChanges(
            [NotNull] INameResolver nameResolver,
            uint customerId,
            [NotNull] HashSet<uint> values,
            [NotNull] out List<pair> changes)
        {
            changes = new List<pair>();
            foreach (var val in values)
            {
                var id = val;
                var name = nameResolver.GetDepartmentName(customerId, id);
                changes.Add(new pair(id, name));
            }
        }

        private static void AddChangesIfMissing(
            [NotNull] INameResolver nameResolver,
            uint customerId,
            [NotNull] ICollection<uint> source,
            [NotNull] ICollection<uint> set,
            [CanBeNull] out List<pair> changes)
        {
            changes = null;

            foreach (var val in source)
            {
                if (set.Contains(val))
                    continue;

                if (null == changes)
                    changes = new List<pair>();

                var id = val;
                var name = nameResolver.GetDepartmentName(customerId, id);
                changes.Add(new pair(id, name));
            }
        }

        private static void AddChangesIfMissing<T>(
            [NotNull] ICollection<T> source,
            [NotNull] ICollection<T> set,
            [CanBeNull] out List<T> changes)
        {
            changes = null;

            foreach (var val in source)
            {
                if (set.Contains(val))
                    continue;

                if (null == changes)
                    changes = new List<T>();
                changes.Add(val);
            }
        }
    }
}