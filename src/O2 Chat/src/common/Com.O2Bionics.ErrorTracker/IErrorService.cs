using System;

namespace Com.O2Bionics.ErrorTracker
{
    public interface IErrorService : IDisposable
    {
        void Save(params ErrorInfo[] errorInfos);
    }
}