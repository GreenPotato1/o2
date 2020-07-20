using System;
using System.Collections.Generic;
using Com.O2Bionics.FeatureService.Impl.DataModel;

namespace Com.O2Bionics.FeatureService.Impl
{
    public interface IDatabaseFactory
    {
        bool LogEnabled { get; set; }
        IReadOnlyList<string> ProductCodes { get; }
        T Query<T>(string productCode, Func<Database, T> func, bool? logEnabled = null);
        void Query(string productCode, Action<Database> action, bool? logEnabled = null);
    }
}