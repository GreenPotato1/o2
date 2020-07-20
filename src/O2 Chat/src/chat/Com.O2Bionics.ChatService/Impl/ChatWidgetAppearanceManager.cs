using System.Collections.Generic;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.WidgetAppearance;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.FeatureService.Constants;
using Com.O2Bionics.Utils;

namespace Com.O2Bionics.ChatService.Impl
{
    public class ChatWidgetAppearanceManager : IChatWidgetAppearanceManager
    {
        private readonly ISettingsStorage m_settingsStorage;
        private readonly IChatDatabaseFactory m_databaseFactory;
        private readonly IFeatureServiceClient m_featureService;

        public ChatWidgetAppearanceManager(
            ISettingsStorage settingsStorage,
            IChatDatabaseFactory databaseFactory,
            IFeatureServiceClient featureService)
        {
            m_settingsStorage = settingsStorage;
            m_databaseFactory = databaseFactory;
            m_featureService = featureService;
        }

        public void Save(uint customerId, ChatWidgetAppearance widgetAppearance)
        {
            var writableSettings = m_settingsStorage.GetWritableCustomerSettings(customerId);
            writableSettings.Customization = SerializeWidgetAppearance(widgetAppearance);
            using (var dc = m_databaseFactory.CreateContext())
            {
                m_settingsStorage.SaveCustomerSettings(dc, customerId, writableSettings);
                dc.Commit();
            }
        }

        public ChatWidgetAppearanceInfo Get(uint customerId)
        {
            var appearanceSettingsJson = m_settingsStorage.GetCustomerSettings(customerId).Customization;
            var appearanceData = DeserializeWidgetAppearance(appearanceSettingsJson);
            var enabledFeatures = GetEnabledFeatures(customerId);
            ApplyFeatureRestrictions(appearanceData, enabledFeatures);

            return new ChatWidgetAppearanceInfo
                {
                    EnabledFeatures = enabledFeatures,
                    AppearanceData = appearanceData,
                };
        }

        private void ApplyFeatureRestrictions(ChatWidgetAppearance widgetAppearanceObj, HashSet<string> enabledFeatures)
        {
            if (!enabledFeatures.Contains(FeatureCodes.IsWidgetSelectThemeAllowed))
                widgetAppearanceObj.ThemeId = ChatWidgetThemes.Default;

            if (!enabledFeatures.Contains(FeatureCodes.IsWidgetMinSelectThemeAllowed))
                widgetAppearanceObj.ThemeMinId = ChatWidgetThemes.DefaultMin;

            if (!enabledFeatures.Contains(FeatureCodes.IsWidgetFullCssCustomizationAllowed))
                widgetAppearanceObj.CustomCssUrl = "";

            if (!enabledFeatures.Contains(FeatureCodes.IsWidgetPoweredByHidden))
                widgetAppearanceObj.PoweredByVisible = true;

            if (!enabledFeatures.Contains(FeatureCodes.IsWidgetPositioningAllowed))
            {
                widgetAppearanceObj.Location = ChatWidgetLocation.BottomRight;
                widgetAppearanceObj.OffsetX = 0;
                widgetAppearanceObj.OffsetY = 0;
            }

            if (!enabledFeatures.Contains(FeatureCodes.IsWidgetMinimizedStateCustomizationAllowed))
                widgetAppearanceObj.MinimizedStateTitle = "";
        }

        private static ChatWidgetAppearance DeserializeWidgetAppearance(string customizationJson)
        {
            if (string.IsNullOrWhiteSpace(customizationJson))
                return new ChatWidgetAppearance();

            return customizationJson.JsonUnstringify2<ChatWidgetAppearance>();
        }

        private static string SerializeWidgetAppearance(ChatWidgetAppearance appearance)
        {
            return appearance?.JsonStringify2();
        }

        public HashSet<string> GetEnabledFeatures(uint customerId)
        {
            return m_featureService.GetBoolSet(customerId, FeatureCodes.WidgetAppearanceFeatureCodes).WaitAndUnwrapException();
        }
    }
}