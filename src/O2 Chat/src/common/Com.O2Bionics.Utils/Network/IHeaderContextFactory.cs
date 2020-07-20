using System;
using System.ServiceModel;
using JetBrains.Annotations;

namespace Com.O2Bionics.Utils.Network
{
    public interface IHeaderContextFactory
    {
        /// <summary>
        /// Add the WCF headers such as client IP address.
        /// The returned value, if not null, must be disposed.
        /// </summary>
        [CanBeNull]
        IDisposable Create([NotNull] IClientChannel channel);
    }
}