using System;
using Com.O2Bionics.ChatService.DataModel;

namespace Com.O2Bionics.ChatService
{
    public interface IDataContext : IDisposable
    {
        ChatDatabase Db { get; }
        void Commit();
        void Rollback();
    }
}