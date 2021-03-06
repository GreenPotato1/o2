﻿using System.Collections.Generic;
using System.Threading.Tasks;
using ArenaSApp.Models.Permissions;

namespace ArenaSApp.Services.Permissions
{
    public interface IPermissionsService
    {
        Task<PermissionStatus> CheckPermissionStatusAsync(Permission permission);
        Task<Dictionary<Permission, PermissionStatus>> RequestPermissionsAsync(params Permission[] permissions);
    }
}
