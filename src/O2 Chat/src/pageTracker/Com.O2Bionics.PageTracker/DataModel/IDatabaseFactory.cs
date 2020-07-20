using System;

namespace Com.O2Bionics.PageTracker.DataModel
{
    public interface IDatabaseFactory
    {
        bool LogEnabled { get; set; }
        T Query<T>(Func<Database, T> func, bool? logEnabled = null);
        void Query(Action<Database> action, bool? logEnabled = null);
    }
}