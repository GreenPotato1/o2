using System.Collections.Generic;
using Com.O2Bionics.ChatService.DataModel;

namespace Com.O2Bionics.ChatService
{
    public class DbUpdate<T>
        where T : class
    {
        public DbUpdate(T original, T changed)
        {
            Original = original;
            Changed = changed;
            Success = true;
        }

        public T Original { get; private set; }
        public T Changed { get; private set; }
        public bool Success { get; private set; }

        public void SetFailed()
        {
            Success = false;
        }
    }

    public interface IDbUpdaterStorage<T>
        where T : class
    {
        void Load(ChatDatabase db);

        List<DbUpdate<T>> GetDbUpdates();
        void ApplyDbUpdateResult(List<DbUpdate<T>> updates);
        void UpdateDb(IChatDatabaseFactory dbFactory, List<DbUpdate<T>> updates);
    }
}