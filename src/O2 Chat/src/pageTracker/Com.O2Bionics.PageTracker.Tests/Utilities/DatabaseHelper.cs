using System;
using Com.O2Bionics.PageTracker.DataModel;
using Com.O2Bionics.PageTracker.Tests.Settings;

namespace Com.O2Bionics.PageTracker.Tests.Utilities
{
    public sealed class DatabaseHelper
    {
        public DatabaseHelper()
            : this(new TestPageTrackerSettings())
        {
        }

        public DatabaseHelper(PageTrackerSettings settings)
        {
            Settings = settings;
            DbFactory = new DatabaseFactory(Settings);
        }

        public PageTrackerSettings Settings { get; }

        public IDatabaseFactory DbFactory { get; }

        public T Query<T>(Func<Database, T> func)
        {
            return DbFactory.Query(func);
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
    }
}