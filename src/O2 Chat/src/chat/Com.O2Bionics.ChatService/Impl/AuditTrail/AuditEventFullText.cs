using System;
using System.Collections.Generic;
using System.Text;
using Com.O2Bionics.AuditTrail.Client.Utilities;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.Widget;
using Com.O2Bionics.ChatService.Contract.WidgetAppearance;
using Com.O2Bionics.ChatService.Impl.AuditTrail.Names;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService.Impl.AuditTrail
{
    public static class AuditEventFullText
    {
        //If you change this class, make changes to the "SpecificClassDiff.cs".

        public static void SetAnalyzedFields<T>(
            [NotNull] this AuditEvent<T> auditEvent,
            //Whether to sort the departments by name - only for user.
            bool sortNames = false)
            where T : class
        {
            auditEvent.Changed = auditEvent.FieldChanges.ChangedToString();

            bool hasOld = null != auditEvent.OldValue, hasNew = null != auditEvent.NewValue;

            var type = typeof(T);
            var builder = new StringBuilder();
            if (hasOld)
                OneInstance(type, auditEvent.OldValue, builder);
            if (hasNew)
                OneInstance(type, auditEvent.NewValue, builder);

            if (type == typeof(UserInfo))
            {
                var userHistory = auditEvent as AuditEvent<UserInfo>;
#if DEBUG
                if (null == userHistory)
                    throw new Exception("null == userHistory");
#endif
                AppendUserExtraInfo(userHistory, sortNames, builder);
            }

            auditEvent.All = 0 == builder.Length ? null : builder.ToString();
        }

        private static void OneInstance<T>(Type type, [NotNull] T value, StringBuilder builder)
            where T : class
        {
            // ReSharper disable AssignNullToNotNullAttribute
            if (type == typeof(UserInfo))
                OneInstance(value as UserInfo, builder);
            else if (type == typeof(DepartmentInfo))
                OneInstance(value as DepartmentInfo, builder);
            else if (type == typeof(ChatWidgetAppearance))
                OneInstance(value as ChatWidgetAppearance, builder);
            else if (type == typeof(CustomerInfo))
                OneInstance(value as CustomerInfo, builder);
            else if (type == typeof(WidgetDailyViewCountExceededEvent))
                OneInstance(value as WidgetDailyViewCountExceededEvent, builder);
            else if (type == typeof(WidgetUnknownDomain))
                OneInstance(value as WidgetUnknownDomain, builder);
            else if (type == typeof(WidgetUnknownDomainTooManyEvent))
                OneInstance(value as WidgetUnknownDomainTooManyEvent, builder);
            // ReSharper restore AssignNullToNotNullAttribute
#if DEBUG
            else
                throw new Exception($"Unknown type of AuditEvent<{type.FullName}>.");
#endif
        }

        private static void OneInstance([NotNull] UserInfo value, StringBuilder builder)
        {
            builder.AppendIfNotEmpty(value.Status.ToString());
            builder.AppendIfNotEmpty(value.FirstName);
            builder.AppendIfNotEmpty(value.LastName);
            builder.AppendIfNotEmpty(value.Email);
            builder.AppendIfNotEmpty(value.Avatar);
        }

        private static void AppendUserExtraInfo([NotNull] AuditEvent<UserInfo> auditEvent, bool sortNames, [NotNull] StringBuilder builder)
        {
            if (null == auditEvent.ObjectNames || !auditEvent.ObjectNames.TryGetValue(EntityNames.Department, out var departments))
                return;

            HashSet<uint> ids = null;

            AppendDepartmentIds(auditEvent.OldValue, ref ids);
            AppendDepartmentIds(auditEvent.NewValue, ref ids);
            if (null == ids || 0 == ids.Count)
                return;

            var names = new List<string>(ids.Count);
            foreach (var id in ids)
            {
                if (departments.TryGetValue(id.ToString(), out var name))
                    names.Add(name);
            }

            if (sortNames)
                names.Sort();

            builder.AppendIfNotEmpty(names);
        }

        private static void AppendDepartmentIds([CanBeNull] UserInfo info, ref HashSet<uint> ids)
        {
            if (null == info)
                return;

            AddIds(info.AgentDepartments, ref ids);
            AddIds(info.SupervisorDepartments, ref ids);
        }

        private static void AddIds([CanBeNull] HashSet<uint> departments, [CanBeNull] ref HashSet<uint> ids)
        {
            if (null == departments || 0 == departments.Count)
                return;

            if (null == ids)
                ids = new HashSet<uint>();

            foreach (var rawId in departments)
                ids.Add(rawId);
        }

        private static void OneInstance([NotNull] DepartmentInfo value, StringBuilder builder)
        {
            builder.AppendIfNotEmpty(value.Status.ToString());
            builder.AppendIfNotEmpty(value.Name);
            builder.AppendIfNotEmpty(value.Description);
        }

        private static void OneInstance([NotNull] ChatWidgetAppearance value, StringBuilder builder)
        {
            builder.AppendIfNotEmpty(value.ThemeId);
            builder.AppendIfNotEmpty(value.ThemeMinId);
            builder.AppendIfNotEmpty(value.Location.ToString());

            builder.AppendIfNotEmpty(value.OffsetX.ToString());
            if (value.OffsetX != value.OffsetY)
                builder.AppendIfNotEmpty(value.OffsetY.ToString());

            builder.AppendIfNotEmpty(value.MinimizedStateTitle);
            builder.AppendIfNotEmpty(value.CustomCssUrl);
        }

        private static void OneInstance([NotNull] CustomerInfo value, StringBuilder builder)
        {
            builder.AppendIfNotEmpty(value.Status.ToString());
            builder.AppendIfNotEmpty(value.Name);
            builder.AppendIfNotEmpty(value.Domains);
            builder.AppendIfNotEmpty(value.CreateIp);
        }

        private static void OneInstance([NotNull] WidgetDailyViewCountExceededEvent value, StringBuilder builder)
        {
            builder.AppendIfNotEmpty(value.Limit.ToString());
            builder.AppendIfNotEmpty(value.Date.DateToString());
        }

        private static void OneInstance([NotNull] WidgetUnknownDomain value, StringBuilder builder)
        {
            builder.AppendIfNotEmpty(value.Domains);
            builder.AppendIfNotEmpty(value.Name);
        }

        private static void OneInstance([NotNull] WidgetUnknownDomainTooManyEvent value, StringBuilder builder)
        {
            builder.AppendIfNotEmpty(value.Domains);
            builder.AppendIfNotEmpty(value.Limit.ToString());
            builder.AppendIfNotEmpty(value.Date.DateToString());
        }
    }
}