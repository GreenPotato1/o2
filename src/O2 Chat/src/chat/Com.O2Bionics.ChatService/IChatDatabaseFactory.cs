using System;
using Com.O2Bionics.ChatService.DataModel;

namespace Com.O2Bionics.ChatService
{
    public interface IChatDatabaseFactory
    {
        T Query<T>(Func<ChatDatabase, T> func);
        void Query(Action<ChatDatabase> action);
        IDataContext CreateContext();
    }
}