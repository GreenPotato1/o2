using System;
using System.Collections.Generic;
using Com.O2Bionics.FeatureService.Constants;
using Com.O2Bionics.FeatureService.Impl.Properties;
using Com.O2Bionics.Utils;
using log4net;

namespace Com.O2Bionics.FeatureService.Impl.DataModel
{
    public class DatabaseManager : DatabaseManagerBase<Database>
    {
        public DatabaseManager(string connectionString, bool logQueries = true)
            : base(connectionString, logQueries, LogManager.GetLogger(typeof(DatabaseManager)))
        {
        }

        protected override string SchemaScript { get; } = Resources.database;

        protected override string[] Tables { get; } =
            {
                "CUSTOMER",
                "FEATURES",
                "SERVICES",
                "SERVICE_SUBSCRIPTION",
                "SERVICE_FEATURES",
                "CUSTOMER_FEATURES",
            };

        protected override void ExecuteInContext(Action<Database> action)
        {
            new DatabaseFactory("", ConnectionString, LogQueries).Query("", action);
        }

        protected override void InsertInitialData(Database db, DateTime now)
        {
            var h = new DatabaseObjectHelper(db);

            var defaultPlanId = h.AddService("Default Plan", 0);

            // miscellaneous
            var maxUsersFeatureId = h.AddFeature(FeatureCodes.MaxUsers, FeatureValueAggregationMethod.Sum, true);
            var maxDepartmentsFeatureId = h.AddFeature(FeatureCodes.MaxDepartments, FeatureValueAggregationMethod.Sum, true);
            var geoLocationFeatureId = h.AddFeature(FeatureCodes.IsGeoLocationEnabled, FeatureValueAggregationMethod.CAS, false);
            var avatarsAllowedId = h.AddFeature(FeatureCodes.Avatars, FeatureValueAggregationMethod.CAS, false);

            h.AddServiceFeatureValue(defaultPlanId, maxUsersFeatureId, "20");
            h.AddServiceFeatureValue(defaultPlanId, maxDepartmentsFeatureId, "20");
            h.AddServiceFeatureValue(defaultPlanId, geoLocationFeatureId, FeatureValues.True);
            h.AddServiceFeatureValue(defaultPlanId, avatarsAllowedId, FeatureValues.True);

            // visitor widget customization
            var widgetSelectThemeFeatureId = h.AddCasFeature(FeatureCodes.IsWidgetSelectThemeAllowed);
            var widgetMinSelectThemeFeatureId = h.AddCasFeature(FeatureCodes.IsWidgetMinSelectThemeAllowed);
            var widgetPositioningFeatureId = h.AddFeature(FeatureCodes.IsWidgetPositioningAllowed);
            var widgetFullCssCustomizationFeatureId = h.AddFeature(FeatureCodes.IsWidgetFullCssCustomizationAllowed);
            var widgetPoweredByHiddenFeatureId = h.AddFeature(FeatureCodes.IsWidgetPoweredByHidden);
            var widgetMinimizedStateCustomizationFeatureId = h.AddFeature(FeatureCodes.IsWidgetMinimizedStateCustomizationAllowed);

            h.AddServiceFeatureValue(defaultPlanId, widgetSelectThemeFeatureId, FeatureValues.True);
            h.AddServiceFeatureValue(defaultPlanId, widgetMinSelectThemeFeatureId, FeatureValues.True);
            h.AddServiceFeatureValue(defaultPlanId, widgetPositioningFeatureId, FeatureValues.True);
            h.AddServiceFeatureValue(defaultPlanId, widgetFullCssCustomizationFeatureId, FeatureValues.True);
            h.AddServiceFeatureValue(defaultPlanId, widgetPoweredByHiddenFeatureId, FeatureValues.True);
            h.AddServiceFeatureValue(defaultPlanId, widgetMinimizedStateCustomizationFeatureId, FeatureValues.True);

            var widgetDailyViewLimit = h.AddFeature(FeatureCodes.WidgetDailyViewLimit);
            h.AddServiceFeatureValue(defaultPlanId, widgetDailyViewLimit, "500");

            var loginVisibleDays = h.AddFeature(FeatureCodes.LoginVisibleDays);
            h.AddServiceFeatureValue(defaultPlanId, loginVisibleDays, "100");

            var auditVisibleDays = h.AddFeature(FeatureCodes.AuditVisibleDays);
            h.AddServiceFeatureValue(defaultPlanId, auditVisibleDays, "100");

            var idNames = new[]
                {
                    new KeyValuePair<int, string>(1, "Chat Test Customer"),
                    new KeyValuePair<int, string>(2, "Second Customer")
                };
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < idNames.Length; i++)
            {
                var customerId = idNames[i].Key;
                h.AddCustomer(idNames[i].Value, customerId);
                h.AddServiceSubscription(defaultPlanId, customerId);
            }
        }
    }
}