using System.Collections.Generic;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Objects;

namespace Com.O2Bionics.ChatService
{
    public interface IDepartmentStorage
    {
        Department Get(ChatDatabase db, uint customerId, uint departmentId);
        List<Department> GetAll(ChatDatabase db, uint customerId, bool skipPrivate);
        HashSet<uint> GetPublicIds(ChatDatabase db, uint customerId, HashSet<uint> onlineDepartmentIds);

        Department CreateNew(ChatDatabase db, Department department);
        Department Update(ChatDatabase db, uint customerId, uint departmentId, Department.UpdateInfo update);
    }
}