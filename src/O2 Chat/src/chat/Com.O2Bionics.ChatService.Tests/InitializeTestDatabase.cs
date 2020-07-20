using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils.JsonSettings;
using NUnit.Framework;

namespace Com.O2Bionics.ChatService.Tests
{
    [TestFixture]
    [Explicit]
    public class InitializeTestDatabase
    {
        public static readonly string ConnectionString =
            new JsonSettingsReader().ReadFromFile<TestSettings>().ChatServiceDatabase;

        [Test, Explicit]
        public void RecreateSchema()
        {
            new DatabaseManager(ConnectionString).RecreateSchema();
        }

        [Test, Explicit]
        public void ReloadData()
        {
            new DatabaseManager(ConnectionString).ReloadData();
        }

        [Test, Explicit]
        public void DeleteData()
        {
            new DatabaseManager(ConnectionString).DeleteData();
        }
    }
}