﻿using System.Threading.Tasks;

 namespace ArenaSApp.Services.Location
{    
    public interface ILocationService
    {
        Task UpdateUserLocation(ArenaSApp.Models.Location.Location newLocReq, string token);
    }
}