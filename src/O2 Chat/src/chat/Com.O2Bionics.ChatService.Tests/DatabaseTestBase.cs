using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils.JsonSettings;
using NUnit.Framework;

namespace Com.O2Bionics.ChatService.Tests
{
    /// <summary>
    /// Creates the test database <see cref="Com.O2Bionics.Tests.Common.TestSettings.ChatServiceDatabase"/>.
    /// </summary>
    public class DatabaseTestBase
    {
        protected static readonly TestSettings TestSettings = new JsonSettingsReader().ReadFromFile<TestSettings>();
        protected static string ConnectionString => TestSettings.ChatServiceDatabase;

        [SetUp]
        public void SetUp()
        {
            var databaseManager = new DatabaseManager(ConnectionString, false);
            databaseManager.RecreateSchema();

            var databaseManager2 = new DatabaseManager(ConnectionString, false);
            databaseManager2.ReloadData();
        }
    }
}