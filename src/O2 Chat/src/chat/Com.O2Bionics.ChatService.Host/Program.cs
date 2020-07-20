using System.Linq;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.JsonSettings;
using log4net.Config;

namespace Com.O2Bionics.ChatService.Host
{
    public static class Program
    {
        private const string ApplicationName = "ChatServiceHost";

        private static void Main(string[] args)
        {
            var quiet = args.Contains("--quiet");

            var jsonSettingsReader = new JsonSettingsReader();
            var settings = jsonSettingsReader.ReadFromFile<ChatServiceSettings>();
            if (args.Contains("--recreate-schema"))
            {
                Configure();
                new DatabaseManager(settings.Database, !quiet).RecreateSchema();
            }
            else if (args.Contains("--delete-data"))
            {
                Configure();
                new DatabaseManager(settings.Database, !quiet).DeleteData();
            }
            else if (args.Contains("--reload-data"))
            {
                Configure();
                new DatabaseManager(settings.Database, !quiet).ReloadData();
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

        private static void StartService(ChatServiceSettings settings)
        {
            ServiceHelper.Run(
                "O2BionicsChat",
                "Stores state of the O2Chat server",
                "O2Chat Service Host",
                log => new ChatServiceHost(),
                () => LogConfigurator.Configure(settings.ErrorTracker, ApplicationName),
                //()=>{},
                x => x.Start(),
                x => x.Stop()
            );
        }
    }
}