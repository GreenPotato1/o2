using System;
using System.Linq;
using Com.O2Bionics.PageTracker.DataModel;
using Com.O2Bionics.Utils.JsonSettings;
using log4net.Config;

namespace Com.O2Bionics.PageTracker.DatabaseTool
{
    public static class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            var quiet = args.Contains("--quiet");

            var jsonSettingsReader = new JsonSettingsReader();
            var settings = jsonSettingsReader.ReadFromFile<PageTrackerSettings>();
            if (args.Contains("--recreate-schema"))
            {
                var cs = settings.Database;
                new DatabaseManager(cs, !quiet).RecreateSchema();
            }
            else if (args.Contains("--delete-data"))
            {
                var cs = settings.Database;
                new DatabaseManager(cs, !quiet).DeleteData();
            }
            else if (args.Contains("--reload-data"))
            {
                var cs = settings.Database;
                new DatabaseManager(cs, !quiet).ReloadData();
            }
            else
            {
                Console.WriteLine("Unknown command.");
            }
        }
    }
}