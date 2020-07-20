using System;
using System.Net;
using Com.O2Bionics.PageTracker.Contract;
using JetBrains.Annotations;

namespace Com.O2Bionics.PageTracker
{
    public interface IGeoIpAddressResolver : IDisposable
    {
        [CanBeNull]
        GeoLocation ResolveAddress([NotNull] IPAddress ip);
    }
}