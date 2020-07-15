using System.Collections.Generic;
using System.Threading.Tasks;
using PFRCenterGlobal.Models.Permissions;

namespace PFRCenterGlobal.Services.Permissions
{
    public interface IPermissionsService
    {
        Task<PermissionStatus> CheckPermissionStatusAsync(Permission permission);
        Task<Dictionary<Permission, PermissionStatus>> RequestPermissionsAsync(params Permission[] permissions);
    }
}
