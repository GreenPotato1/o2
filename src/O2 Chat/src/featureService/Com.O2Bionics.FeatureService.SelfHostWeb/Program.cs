using System;
using System.Linq;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.FeatureService.Impl;
using Com.O2Bionics.FeatureService.Impl.DataModel;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.JsonSettings;
using log4net;
using log4net.Config;
using Microsoft.Owin.Hosting;

namespace Com.O2Bionics.FeatureService.SelfHostWeb
{
    public static class Program
    {
        private const string ApplicationName = "FeatureServiceHost";

        private const string ProductCodeSwitch = "--product-code=";
        private const string ProductCodeDefault = "chat";

        private static void Main(string[] args)
        {
            var quiet = args.Contains("--quiet");

            var productCode = ProductCodeDefault;
            var productCodeParam = args.FirstOrDefault(x => x.StartsWith(ProductCodeSwitch));
            if (productCodeParam != null)
            {
                productCode = productCodeParam.Substring(ProductCodeSwitch.Length);
            }

            var jsonSettingsReader = new JsonSettingsReader();
            var settings = jsonSettingsReader.ReadFromFile<FeatureServiceSettings>();
            if (args.Contains("--recreate-schema"))
            {
                Configure();
                var cs = settings.Databases[productCode];
                new DatabaseManager(cs, !quiet).RecreateSchema();
            }
            else if (args.Contains("--delete-data"))
            {
                Configure();
                var cs = settings.Databases[productCode];
                new DatabaseManager(cs, !quiet).DeleteData();
            }
            else if (args.Contains("--reload-data"))
            {
                Configure();
                var cs = settings.Databases[productCode];
                new DatabaseManager(cs, !quiet).ReloadData();
            }
            else
            {
                StartService(settings);
            }
        }

        private static void Configure()
        {
            XmlConfigurator.Configure();
        }

        private static void StartService(FeatureServiceSettings settings)
        {
            ServiceHelper.Run(
                "O2BionicsFeatureService",
                "Provides http access to O2Bionics feature registry",
                "O2Bionics Feature Service Host",
                CreateService,
                () => LogConfigurator.Configure(settings.ErrorTracker, ApplicationName));
        }

        private static IDisposable CreateService(ILog log)
        {
            var settings = new JsonSettingsReader().ReadFromFile<FeatureServiceSettings>();
            var uri = settings.SelfHostWebBindUri;
            if (string.IsNullOrWhiteSpace(uri))
                throw new Exception("SelfHostWebBindUri setting in the JSON file is empty or not defined");

            log.InfoFormat("Starting service at {0}", uri);

            return WebApp.Start<Startup>(uri);
        }
    }
}