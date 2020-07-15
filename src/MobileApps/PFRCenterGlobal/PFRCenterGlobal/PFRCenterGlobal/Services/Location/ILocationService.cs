﻿using System.Threading.Tasks;

 namespace PFRCenterGlobal.Services.Location
{    
    public interface ILocationService
    {
        Task UpdateUserLocation(Models.Location.Location newLocReq, string token);
    }
}