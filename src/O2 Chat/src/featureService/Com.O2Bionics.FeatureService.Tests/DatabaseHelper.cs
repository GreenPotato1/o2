using System;
using Com.O2Bionics.FeatureService.Impl;
using Com.O2Bionics.FeatureService.Impl.DataModel;

namespace Com.O2Bionics.FeatureService.Tests
{
    public class DatabaseHelper
    {
        public const string TestProductCode = "test";

        public DatabaseFactory DbFactory { get; }
        public FeatureServiceSettings Settings { get; }

        public DatabaseHelper(FeatureServiceSettings settings)
        {
            Settings = settings;
            DbFactory = new DatabaseFactory(settings);
        }
        
        public T Query<T>(Func<Database, T> func)
        {
            return DbFactory.Query(TestProductCode, func);
        }

        public void Query(Action<Database> action)
        {
            Query(
                db =>
                    {
                        action(db);
                        return 0;
                    });
        }

        public void CleanupDatabase()
        {
            new DatabaseManager(Settings.Databases[TestProductCode], false).DeleteData();
        }
    }
}