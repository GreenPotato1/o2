using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.FeatureService.Impl;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.JsonSettings;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Com.O2Bionics.FeatureService.Tests
{
    public class FeatureServiceTestServerHelper : IDisposable
    {
        private const int TestServerPort = 8081;

        private const string ServerSettingsFileName = "testServerSettings.json";
        private const string ServerExecutableFileName = "Com.O2Bionics.FeatureService.SelfHostWeb.exe";

        private readonly ILog m_log = LogManager.GetLogger(typeof(FeatureServiceTestServerHelper));
        private readonly Process m_process;

        public FeatureServiceTestServerHelper()
        {
            m_process = StartServer();
        }

        public void Dispose()
        {
            StopServer(m_process);
            m_process.Dispose();
        }

        public Uri Uri => new Uri("http://localhost:" + TestServerPort);

        public FeatureServiceClient CreateClient(
            string overrideProductCode = null,
            bool? ignoreCache = null,
            int? ttl = null,
            bool? logProcessing = null,
            bool? logSqlQuery = null,
            INowProvider nowProvider = null)
        {
            if (null == nowProvider)
                nowProvider = new DefaultNowProvider();

            var settings = new TestFeatureServiceClientSettings(
                DatabaseHelper.TestProductCode,
                new[] { Uri },
                0);
            return new FeatureServiceClient(
                settings,
                null,
                nowProvider,
                overrideProductCode,
                ignoreCache,
                ttl,
                logProcessing,
                logSqlQuery);
        }

        private Process StartServer()
        {
            var executingPath = AssemblyHelper.GetExecutingAssemblyPath();
            var serverSettingsFullPath = Path.Combine(executingPath, ServerSettingsFileName);
            CreateServerSettingsFile(serverSettingsFullPath);
            FixExeSettings(executingPath, serverSettingsFullPath);

            var name = "FeatureService_" + Guid.NewGuid();
            var exeFileName = Path.Combine(executingPath, ServerExecutableFileName);
            m_log.Debug($"Starting service name='{name}', path='{exeFileName}'.");

            var process = new Process
                {
                    StartInfo =
                        {
                            FileName = exeFileName,
                            Arguments = $"-displayname \"{name}\" -servicename \"{name}\"",
                            WindowStyle = ProcessWindowStyle.Minimized,
                        }
                };
            process.Start();
            Thread.Sleep(1000);
            return process;
        }

        private void CreateServerSettingsFile(string settingsFullPath)
        {
            var testSettings = new JsonSettingsReader().ReadFromFile<TestSettings>();

            var featureServiceSettings = new FeatureServiceSettings
                {
                    Databases = new Dictionary<string, string> { { DatabaseHelper.TestProductCode, testSettings.FeatureServiceDatabase } },
                    LogProcessing = false,
                    LogSqlQuery = false,
                    SelfHostWebBindUri = Uri.ToString(),
                    Cache = new FeatureServiceCacheSettings
                        {
                            MemoryLimitMegabytes = 0,
                            MemoryPollingInterval = TimeSpan.FromSeconds(5),
                            PhysicalMemoryLimitPercentage = 0,
                        },
                    TimeToLive = TimeSpan.FromMinutes(5),
                };
            var serverSettings = new
                {
                    featureService = featureServiceSettings,
                    errorTracker = testSettings.ErrorTracker,
                };

            var jsonSerializerSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                };
            var serverSettingsJson = JsonConvert.SerializeObject(serverSettings, jsonSerializerSettings);
            File.WriteAllText(settingsFullPath, serverSettingsJson);
        }

        private static void FixExeSettings(string executingPath, string serverSettingsFullPath)
        {
            var serverAppConfig = Path.Combine(executingPath, ServerExecutableFileName + ".config");
            var configFileMap = new ExeConfigurationFileMap { ExeConfigFilename = serverAppConfig };
            var config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

            config.AppSettings.Settings["ConfigFilePath"].Value = serverSettingsFullPath;
            //TODO: task-367. p3. LogDirectory "C:\O2Bionics\Logs" must be fixed too.
            config.Save();
        }

        private static void StopServer(Process p)
        {
            p.Kill();
            p.WaitForExit(); // Waits here for the process to exit.
        }
    }
}