using System.Collections.Generic;
using Com.O2Bionics.FeatureService.Constants;
using Com.O2Bionics.ChatService.Contract.WidgetAppearance;

namespace Com.O2Bionics.ChatService.Web.Console.Models.ChatCustomization
{
    public class ChatWidgetAppearanceViewModel
    {
        public string WidgetScript { get; set; }
        public HashSet<string> EnabledFeatures { get; set; }
        public ChatWidgetThemeSelector ThemeSelector { get; set; }
        public ChatWidgetMinThemeSelector ThemeMinSelector { get; set; }
        public ChatWidgetPositioning Positioning { get; set; }
        public ChatWidgetMinimizedState MinimizedState { get; set; }
        public ChatWidgetFullCSSCustomization FullCustomization { get; set; }
        public ChatWidgetPoweredByVisible PoweredBy { get; set; }
        public string BackgroundPageUrl { get; set; }
        public string ThemesUrl { get; set; }
    }

    public class ChatWidgetThemeSelector : FeatureControlledOptionBase
    {
        public ChatWidgetThemeSelector(HashSet<string> enabledFeatures, ChatWidgetAppearance widgetAppearance)
            : base(FeatureCodes.IsWidgetSelectThemeAllowed, enabledFeatures)
        {
            ThemeSelectedId = widgetAppearance.ThemeId;
            FullCssCustomizationAllowed = enabledFeatures.Contains(FeatureCodes.IsWidgetFullCssCustomizationAllowed);
        }

        public string ThemeSelectedId { get; set; }
        public bool FullCssCustomizationAllowed { get; }
    }

    public class ChatWidgetMinThemeSelector : FeatureControlledOptionBase
    {
        public ChatWidgetMinThemeSelector(HashSet<string> enabledFeatures, ChatWidgetAppearance widgetAppearance)
            : base(FeatureCodes.IsWidgetMinSelectThemeAllowed, enabledFeatures)
        {
            ThemeSelectedId = widgetAppearance.ThemeMinId;
        }

        public string ThemeSelectedId { get; set; }
    }

    public class ChatWidgetPositioning : FeatureControlledOptionBase
    {
        public ChatWidgetPositioning(HashSet<string> enabledFeatures, ChatWidgetAppearance widgetAppearance)
            : base(FeatureCodes.IsWidgetPositioningAllowed, enabledFeatures)
        {
            Location = widgetAppearance.Location;
            OffsetX = widgetAppearance.OffsetX;
            OffsetY = widgetAppearance.OffsetY;
        }

        public ChatWidgetLocation Location { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
    }

    public class ChatWidgetMinimizedState : FeatureControlledOptionBase
    {
        public ChatWidgetMinimizedState(HashSet<string> enabledFeatures, ChatWidgetAppearance widgetAppearance)
            : base(FeatureCodes.IsWidgetMinimizedStateCustomizationAllowed, enabledFeatures)
        {
            Title = widgetAppearance.MinimizedStateTitle;
        }

        public string Title { get; set; }
    }

    public class ChatWidgetFullCSSCustomization : FeatureControlledOptionBase
    {
        public ChatWidgetFullCSSCustomization(HashSet<string> enabledFeatures, ChatWidgetAppearance widgetAppearance)
            : base(FeatureCodes.IsWidgetFullCssCustomizationAllowed, enabledFeatures)
        {
            CssFileUrl = widgetAppearance.CustomCssUrl;
        }

        public string CssFileUrl { get; set; }
        public string DownloadCssUrl { get; set; }
    }

    public class ChatWidgetPoweredByVisible : FeatureControlledOptionBase
    {
        public ChatWidgetPoweredByVisible(HashSet<string> enabledFeatures, ChatWidgetAppearance widgetAppearance)
            : base(FeatureCodes.IsWidgetPoweredByHidden, enabledFeatures)
        {
            Hide = !widgetAppearance.PoweredByVisible;
        }

        public bool Hide { get; set; }
    }

    public abstract class FeatureControlledOptionBase
    {
        public string FeatureCode { get; }

        public bool IsEnabled { get; }

        protected FeatureControlledOptionBase(string featureCode, HashSet<string> enabledFeatures)
        {
            FeatureCode = featureCode;
            IsEnabled = enabledFeatures.Contains(featureCode);
        }
    }
}