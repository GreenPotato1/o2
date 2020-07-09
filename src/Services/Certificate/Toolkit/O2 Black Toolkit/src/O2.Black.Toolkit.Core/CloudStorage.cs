using System.Collections.Generic;
using System.Linq;

namespace O2.Black.Toolkit.Core
{
    public class CloudStorage: Singleton<CloudStorage>
    {
        public List<AccountCloudStorage> AccountCloudStorages { get; set; } = new List<AccountCloudStorage>();
        
        protected CloudStorage()
        {
            
        }

        public AccountCloudStorage GetAccountCloudStorage(TypeTable typeTable)
        {
            return AccountCloudStorages.Single(x => x.TypeTable == typeTable);
        }

        public void Clear()
        {
            AccountCloudStorages.Clear();
        }
    }
}