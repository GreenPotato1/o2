using Com.O2Bionics.AuditTrail.Client.Utilities;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.WidgetAppearance;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService.Impl.AuditTrail
{
    /// <summary>
    ///     Reports the difference, if any, between two instances of the
    ///     contract classes.
    /// </summary>
    public static class SpecificClassDiff
    {
        //If you change this class, make changes to the "AuditEventFullText.cs" and "AuditTrailFormat.ts".

        [CanBeNull]
        public static FieldChanges Diff([NotNull] this ChatWidgetAppearance oldValue, [NotNull] ChatWidgetAppearance newValue)
        {
            var changeList = new FieldChanges();

            DifferenceUtils.AddIfDifferent(
                oldValue.ThemeId,
                newValue.ThemeId,
                nameof(ChatWidgetAppearance.ThemeId),
                ref changeList.StringChanges);

            DifferenceUtils.AddIfDifferent(
                oldValue.ThemeMinId,
                newValue.ThemeMinId,
                nameof(ChatWidgetAppearance.ThemeId),
                ref changeList.StringChanges);

            DifferenceUtils.AddIfDifferent(
                oldValue.Location,
                newValue.Location,
                nameof(ChatWidgetAppearance.Location),
                ref changeList.StringChanges);

            DifferenceUtils.AddIfDifferent(
                oldValue.OffsetX,
                newValue.OffsetX,
                nameof(ChatWidgetAppearance.OffsetX),
                ref changeList.DecimalChanges);

            DifferenceUtils.AddIfDifferent(
                oldValue.OffsetY,
                newValue.OffsetY,
                nameof(ChatWidgetAppearance.OffsetY),
                ref changeList.DecimalChanges);

            DifferenceUtils.AddIfDifferent(
                oldValue.MinimizedStateTitle,
                newValue.MinimizedStateTitle,
                nameof(ChatWidgetAppearance.MinimizedStateTitle),
                ref changeList.StringChanges);

            DifferenceUtils.AddIfDifferent(
                oldValue.CustomCssUrl,
                newValue.CustomCssUrl,
                nameof(ChatWidgetAppearance.CustomCssUrl),
                ref changeList.StringChanges);

            DifferenceUtils.AddIfDifferent(
                oldValue.PoweredByVisible,
                newValue.PoweredByVisible,
                nameof(ChatWidgetAppearance.PoweredByVisible),
                ref changeList.BoolChanges);

            return NullIfEmpty(changeList);
        }

        [CanBeNull]
        public static FieldChanges Diff([NotNull] this CustomerInfo oldValue, [NotNull] CustomerInfo newValue)
        {
            var changeList = new FieldChanges();

            DifferenceUtils.AddIfDifferent(
                oldValue.Status,
                newValue.Status,
                nameof(CustomerInfo.Status),
                ref changeList.StringChanges);

            DifferenceUtils.AddIfDifferent(
                oldValue.UpdateTimestampUtc,
                newValue.UpdateTimestampUtc,
                nameof(CustomerInfo.UpdateTimestampUtc),
                ref changeList.DateTimeChanges);

            DifferenceUtils.AddIfDifferent(
                oldValue.Name,
                newValue.Name,
                nameof(CustomerInfo.Name),
                ref changeList.StringChanges);

            DifferenceUtils.AddIfDifferent(
                oldValue.CreateIp,
                newValue.CreateIp,
                nameof(CustomerInfo.CreateIp),
                ref changeList.StringChanges);

            DifferenceUtils.AddIfDifferent(
                oldValue.Domains,
                newValue.Domains,
                nameof(CustomerInfo.Domains),
                ref changeList.StringListChanges);

            return NullIfEmpty(changeList);
        }

        [CanBeNull]
        public static FieldChanges Diff([NotNull] this DepartmentInfo oldValue, [NotNull] DepartmentInfo newValue)
        {
            var changeList = new FieldChanges();

            DifferenceUtils.AddIfDifferent(
                oldValue.Status,
                newValue.Status,
                nameof(DepartmentInfo.Status),
                ref changeList.StringChanges);

            DifferenceUtils.AddIfDifferent(
                oldValue.IsPublic,
                newValue.IsPublic,
                nameof(DepartmentInfo.IsPublic),
                ref changeList.BoolChanges);

            DifferenceUtils.AddIfDifferent(
                oldValue.Name,
                newValue.Name,
                nameof(DepartmentInfo.Name),
                ref changeList.StringChanges);

            DifferenceUtils.AddIfDifferent(
                oldValue.Description,
                newValue.Description,
                nameof(DepartmentInfo.Description),
                ref changeList.StringChanges);

            return NullIfEmpty(changeList);
        }

        [CanBeNull]
        public static FieldChanges Diff(
            [NotNull] this UserInfo oldValue,
            [NotNull] UserInfo newValue,
            [NotNull] INameResolver nameResolver)
        {
            var changeList = new FieldChanges();

            DifferenceUtils.AddIfDifferent(
                oldValue.UpdateTimestampUtc,
                newValue.UpdateTimestampUtc,
                nameof(UserInfo.UpdateTimestampUtc),
                ref changeList.DateTimeChanges);

            DifferenceUtils.AddIfDifferent(
                oldValue.Status,
                newValue.Status,
                nameof(UserInfo.Status),
                ref changeList.StringChanges);

            DifferenceUtils.AddIfDifferent(
                oldValue.FirstName,
                newValue.FirstName,
                nameof(UserInfo.FirstName),
                ref changeList.StringChanges);

            DifferenceUtils.AddIfDifferent(
                oldValue.LastName,
                newValue.LastName,
                nameof(UserInfo.LastName),
                ref changeList.StringChanges);

            DifferenceUtils.AddIfDifferent(
                oldValue.Email,
                newValue.Email,
                nameof(UserInfo.Email),
                ref changeList.StringChanges);

            DifferenceUtils.AddIfDifferent(
                oldValue.IsOwner,
                newValue.IsOwner,
                nameof(UserInfo.IsOwner),
                ref changeList.BoolChanges);

            DifferenceUtils.AddIfDifferent(
                oldValue.IsAdmin,
                newValue.IsAdmin,
                nameof(UserInfo.IsAdmin),
                ref changeList.BoolChanges);

            DifferenceUtils.AddIfDifferent(
                oldValue.Avatar,
                newValue.Avatar,
                nameof(UserInfo.Avatar),
                ref changeList.StringChanges);

            var customerId = oldValue.CustomerId;
            DifferenceUtils.AddIfDifferent(
                nameResolver,
                customerId,
                oldValue.AgentDepartments,
                newValue.AgentDepartments,
                nameof(UserInfo.AgentDepartments),
                ref changeList.IdListChanges);

            DifferenceUtils.AddIfDifferent(
                nameResolver,
                customerId,
                oldValue.SupervisorDepartments,
                newValue.SupervisorDepartments,
                nameof(UserInfo.SupervisorDepartments),
                ref changeList.IdListChanges);

            return NullIfEmpty(changeList);
        }

        private static FieldChanges NullIfEmpty(FieldChanges fieldChanges)
        {
            var result = null != fieldChanges && 0 < fieldChanges.Count() ? fieldChanges : null;
            return result;
        }
    }
}