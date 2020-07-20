using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Com.O2Bionics.AuditTrail.Contract.Names;
using Com.O2Bionics.AuditTrail.Contract.Settings;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.Console.KibanaModels;
using Com.O2Bionics.Console.Properties;
using Com.O2Bionics.Console.Settings;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.JsonSettings;
using Com.O2Bionics.Utils.Network;
using JetBrains.Annotations;
using pair = System.Collections.Generic.KeyValuePair<string, string>;

namespace Com.O2Bionics.Console
{
    // Some details are at https://rocket.o2bionics.com/projects/chat/work_packages/333/
    public sealed class KibanaCommand : BaseCommand, ICommand
    {
        //Elastic Kibana can:
        //1. Get list of dashboards: http://10.0.0.136:5601/api/saved_objects/dashboard
        // 1.1. Get objects: http://10.0.0.136:5601/api/saved_objects?per_page=100&search=yourIndexName
        //2. Export a given audit dashboard: http://10.0.0.136:5601/api/kibana/dashboards/export?dashboard=3f39f2d0-f166-11e7-b4dc-f73eabee847d
        //2. Export a given error dashboard: http://10.0.0.136:5601/api/kibana/dashboards/export?dashboard=4b3904a0-f138-11e7-b4dc-f73eabee847d
        //3. Import a dashboard:
        // curl -XPOST 10.0.0.136:5601/api/kibana/dashboards/import -H 'kbn-xsrf:true' -H 'Content-type:application/json' -d @dashboard.json

        private const string KibanaHeaderName = "kbn-xsrf";
        private const string KibanaHeaderValue = "true";

        private const string TitleSuffix = " Dashboard";

        private const string AuditCommandName = "--import-kibana-audit", ErrorTrackerCommandName = "--import-kibana-error-tracking";

        private const char Separator = '%';

        public string[] Names => new[] { AuditCommandName, ErrorTrackerCommandName };

        public string GetUsage(JsonSettingsReader reader)
        {
            var kibanaUrl = ReadKibanaUrl(reader);

            var auditSettings = reader.ReadFromFile<AuditTrailServiceSettings>();
            var auditIndex = IndexNameFormatter.FormatWithValidation(auditSettings.Index.Name, ProductCodes.Chat);
            var auditDashboard = auditIndex + TitleSuffix;

            var errorSettings = reader.ReadFromFile<ErrorTrackerSettings>();
            var errorIndex = errorSettings.Index.Name;
            var errorDashboard = errorIndex + TitleSuffix;

            var result = string.Format(
                Resources.KibanaCommandUsage10,
                //Audit.
                auditDashboard,
                kibanaUrl,
                auditIndex,
                auditSettings.ElasticConnection,
                Utilities.ExeName,
                AuditCommandName,
                //ErrorTracker.
                errorDashboard,
                errorIndex,
                errorSettings.ElasticConnection,
                ErrorTrackerCommandName);
            return result;
        }

        public void Run(string commandName, JsonSettingsReader reader)
        {
            RunAsync(commandName, reader).WaitAndUnwrapException();
            reader.CheckIndexesUniquenessSafe();
        }

        private async Task RunAsync(string commandName, JsonSettingsReader reader)
        {
            if (string.IsNullOrEmpty(commandName))
                throw new ArgumentNullException(nameof(commandName));

            var now = DateTime.UtcNow;
            var nowAsString = now.ToUtcString();

            var kibanaUrl = ReadKibanaUrl(reader);
            var readableCommand = AuditCommandName == commandName ? "Audit" : "Error tracker";
            WriteLine(Resources.StartKibanaImport2, readableCommand, kibanaUrl);

            var dashboard = PrepareDashboard(commandName, reader, nowAsString, out var index, out var patternId);
            try
            {
                await DeleteExistingObjects(kibanaUrl, index);
            }
            catch (Exception e)
            {
                WriteLine(Resources.ErrorDeletingExistingObject1, e);
#if DEBUG
                const string err = "\nDEBUG. Exit the application";
                WriteLine(err);
                Environment.Exit(1);
#endif
            }

            await ImportDashboard(kibanaUrl, dashboard, readableCommand);
            await SetDefaultIndexPattern(kibanaUrl, readableCommand, patternId);
        }

        private static string ReadKibanaUrl(JsonSettingsReader reader)
        {
            var kibanaUrl = reader.ReadFromFile<KibanaSettings>().KibanaUrl;
            return kibanaUrl;
        }

        private static void GetIndexAndDashboard(
            [NotNull] string commandName,
            JsonSettingsReader reader,
            Assembly assembly,
            out string indexName,
            out string dashboardTemplate)
        {
            if (null == assembly)
                throw new ArgumentNullException(nameof(assembly));

            switch (commandName)
            {
                case AuditCommandName:
                    var auditSettings = reader.ReadFromFile<AuditTrailServiceSettings>();
                    indexName = IndexNameFormatter.FormatWithValidation(auditSettings.Index.Name, ProductCodes.Chat);
                    dashboardTemplate = assembly.ReadEmbeddedResource("Com.O2Bionics.Console.kibana-dashboard-audit.json");
                    break;
                case ErrorTrackerCommandName:
                    var errorSettings = reader.ReadFromFile<ErrorTrackerSettings>();
                    indexName = errorSettings.Index.Name;
                    dashboardTemplate = assembly.ReadEmbeddedResource("Com.O2Bionics.Console.kibana-dashboard-error.json");
                    break;
                default:
                    throw new ArgumentException($"Unknown command '{commandName}'.");
            }
        }

        [NotNull]
        private static string UpdateTemplate(string template, string nowAsString, string index)
        {
            if (string.IsNullOrEmpty(template))
                throw new ArgumentNullException(nameof(template));
            if (string.IsNullOrEmpty(nowAsString))
                throw new ArgumentNullException(nameof(nowAsString));
            if (string.IsNullOrEmpty(index))
                throw new ArgumentNullException(nameof(index));

            var result = template;
            var parameters = new List<KeyValuePair<string, string>>
                {
                    new pair("now", nowAsString),
                    new pair("index", index)
                };

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < parameters.Count; i++)
            {
                var p = parameters[i];
                var key = Separator + p.Key + Separator;
                result = result.Replace(key, p.Value);
            }

            if (string.IsNullOrEmpty(result))
                throw new Exception("The template is empty.");
            return result;
        }

        private static string SetTemplateIds([NotNull] string template, [NotNull] out string patternId)
        {
            var result = template;
            patternId = string.Empty;

            const string idTemplate = "%id{0}%";
            const int maxId = 1000, patternNumber = 1;
            var id = 0;
            for (; id < maxId; ++id)
            {
                var name = string.Format(idTemplate, id);
                var pos = result.IndexOf(name, StringComparison.Ordinal);
                if (pos < 0)
                    break;

                var value = Guid.NewGuid().ToString();
                result = result.Replace(name, value);
                if (patternNumber == id)
                    patternId = value;
            }

            if (0 == id)
                throw new Exception($"There must be '{string.Format(idTemplate, 0)}' in a template '{template}'.");
            if (maxId <= id)
                throw new Exception($"There are too many ({id}) ids in a template '{result}'.");
            if (string.IsNullOrEmpty(patternId))
                throw new Exception($"The 'index-pattern', marked with '%id{patternNumber}%', must have been set in a template '{template}'.");

            return result;
        }

        private static void CheckDashboard([NotNull] string dashboard)
        {
            var pos = dashboard.IndexOf(Separator);
            if (0 <= pos)
                throw new Exception($"The template with '{Separator}' is incomplete: '{dashboard}'.");
        }

        [NotNull]
        private static string PrepareDashboard(
            string commandName,
            JsonSettingsReader reader,
            string nowAsString,
            [NotNull] out string index,
            [NotNull] out string patternId)
        {
            var assembly = Assembly.GetExecutingAssembly();
            GetIndexAndDashboard(commandName, reader, assembly, out index, out var template);

            var updatedTemplate = UpdateTemplate(template, nowAsString, index);
            var dashboard = SetTemplateIds(updatedTemplate, out patternId);
            CheckDashboard(dashboard);
            return dashboard;
        }

        [ItemCanBeNull]
        private static async Task<string> FetchExistingObjects([NotNull] HttpClient client, [NotNull] KibanaDeleteContext context)
        {
            var response = await client.GetAsync(context.GetPath);
            var result = await response.Content.ReadAsStringAsync();
            CheckStatus(nameof(FetchExistingObjects), context.GetPath, response, result);

            return string.IsNullOrEmpty(result) ? null : result;
        }

        private static void CheckStatus(
            [NotNull] string name,
            [NotNull] string url,
            [NotNull] HttpResponseMessage response,
            [CanBeNull] string result,
            HttpStatusCode extraErrorCode = 0)
        {
            if (HttpStatusCode.OK == response.StatusCode || 0 < extraErrorCode && extraErrorCode == response.StatusCode)
                return;

            throw new Exception($"Status={response.StatusCode} in {name}, url='{url}', response='{result}'.");
        }

        private static async Task<string> DeleteObjectImpl(HttpClient client, SavedObject savedObject)
        {
            savedObject.Validate();

// DELETE http://10.0.0.136:5601/api/saved_objects/visualization/90942110-f228-11e7-b4dc-f73eabee847d
// accept application/json, text/plain, */*
// kbn-version 6.1.1
// Referer http://10.0.0.136:5601/app/kibana

            var url = $"/api/saved_objects/{savedObject.type}/{savedObject.id}";
            var response = await client.DeleteAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            const HttpStatusCode notFound = HttpStatusCode.NotFound;
            CheckStatus(nameof(DeleteObjectImpl), url, response, result, notFound);

            return string.IsNullOrEmpty(result) ? null : result;
        }

        private async Task DeleteExistingObjects(string kibanaUrl, string index)
        {
            const int maxAttempts = 1000;

            var context = new KibanaDeleteContext(index);
            HttpClient client = null;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                client = new HttpClient();
                Parameterize(client, kibanaUrl);

                for (var i = 0; i < maxAttempts && context.CanRun; ++i)
                    if (!await DeleteAttempt(client, context))
                        break;
            }
            finally
            {
                client?.Dispose();
                WriteLine(Resources.DeleteExistingKibanaObjectsReport4, context.DeletedCount, kibanaUrl, index, stopwatch.ElapsedMilliseconds);
            }
        }

        private static void Parameterize(HttpClient client, string kibanaUrl)
        {
            client.BaseAddress = new Uri(kibanaUrl);

            var requestHeaders = client.DefaultRequestHeaders;
            requestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            requestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            requestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            requestHeaders.Add(KibanaHeaderName, KibanaHeaderValue);
            requestHeaders.Add("Origin", kibanaUrl);
            requestHeaders.Add("Referer", Path.Combine(kibanaUrl, "/app/kibana"));
            requestHeaders.Add(
                "User-Agent",
                @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36");
        }

        private async Task<bool> DeleteAttempt([NotNull] HttpClient client, [NotNull] KibanaDeleteContext context)
        {
            var raw = await FetchExistingObjects(client, context);
            if (string.IsNullOrEmpty(raw))
                return false;

            List<SavedObject> objects;
            try
            {
                var savedObjectReport = raw.JsonUnstringify2<SavedObjectReport>();
                objects = savedObjectReport?.saved_objects;
                if (null == objects || 0 == objects.Count)
                    return false;
            }
            catch (Exception e)
            {
                throw new Exception($"Error parsing the existing objects '{raw}'.", e);
            }

            DeleteObjectChunk(objects, client, context);
            return true;
        }

        private void DeleteObjectChunk(List<SavedObject> objects, HttpClient client, KibanaDeleteContext context)
        {
            var hasError = false;

            void DeleteObject(SavedObject o)
            {
                var stopwatch = Stopwatch.StartNew();
                var deleteReport = DeleteObjectImpl(client, o).WaitAndUnwrapException();

                const string emptyJson = "{}";
                string report;
                var isOk = string.IsNullOrEmpty(deleteReport) || emptyJson == deleteReport;
                if (isOk)
                {
                    report = string.Format(Resources.DeletedExistingKibanaObject3, o.type, o.id, stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    hasError = true;
                    report = string.Format(Resources.ErrorDeletingExistingKibanaObject1, deleteReport);
                }

                lock (context.Lock)
                {
                    if (isOk)
                        ++context.DeletedCount;
                    WriteLine(report);
                }
            }

            var degree = Math.Min(objects.Count, context.MaxDegree);
            objects.AsParallel().WithDegreeOfParallelism(degree).ForAll(DeleteObject);

            if (hasError)
                context.EncounterError();
        }

        private async Task ImportDashboard(string kibanaUrl, string dashboard, string readableCommand)
        {
            var stopwatch = Stopwatch.StartNew();
            const string path = "/api/kibana/dashboards/import?force=true";
            var url = Path.Combine(kibanaUrl, path);

            using (var httpClient = new HttpClient())
            {
                Parameterize(httpClient, kibanaUrl);

                var response = await HttpHelper.PostString(httpClient, url, dashboard);
                WriteLine(Resources.ImportToKibanaReport4, readableCommand, url, stopwatch.ElapsedMilliseconds, response);
            }
        }

        private async Task SetDefaultIndexPattern([NotNull] string kibanaUrl, [NotNull] string readableCommand, [NotNull] string patternId)
        {
            var stopwatch = Stopwatch.StartNew();
            const string path = "/api/kibana/settings/defaultIndex";
            var url = Path.Combine(kibanaUrl, path);

            using (var httpClient = new HttpClient())
            {
                Parameterize(httpClient, kibanaUrl);

                var data = "{\"value\": \"" + patternId + "\"}";
                var response = await HttpHelper.PostString(httpClient, url, data);
                WriteLine(Resources.SetDefaultIndexPatternReport4, readableCommand, url, stopwatch.ElapsedMilliseconds, response);
            }
        }
    }
}