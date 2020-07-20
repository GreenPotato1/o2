using System.Collections.Generic;

namespace Com.O2Bionics.FeatureService.Constants
{
    public static class FeatureCodes
    {
        // miscellaneous
        public const string MaxUsers = "chat.MaxUsers";
        public const string MaxDepartments = "chat.MaxDepartments";
        public const string IsGeoLocationEnabled = "chat.Geolocation";
        public const string Avatars = "chat.Avatars";

        public const string WidgetDailyViewLimit = "chat.WidgetDailyViewLimit";

        // visitor widget customization
        public const string IsWidgetSelectThemeAllowed = "chat.WidgetSelectTheme";
        public const string IsWidgetMinSelectThemeAllowed = "chat.WidgetMinSelectTheme";
        public const string IsWidgetPositioningAllowed = "chat.WidgetPositioning";
        public const string IsWidgetFullCssCustomizationAllowed = "chat.WidgetFullCssCustomization";
        public const string IsWidgetPoweredByHidden = "chat.WidgetPoweredByHidden";
        public const string IsWidgetMinimizedStateCustomizationAllowed = "chat.IsWidgetMinimizedStateCustomizationAllowed";

        public static readonly List<string> WidgetAppearanceFeatureCodes =
            new List<string>
                {
                    IsWidgetSelectThemeAllowed,
                    IsWidgetMinSelectThemeAllowed,
                    IsWidgetFullCssCustomizationAllowed,
                    IsWidgetPositioningAllowed,
                    IsWidgetPoweredByHidden,
                    IsWidgetMinimizedStateCustomizationAllowed
                };

        public const string LoginVisibleDays = "chat.loginEvent.visibleDays";
        public const string AuditVisibleDays = "chat.auditEvent.visibleDays";
    }
}
