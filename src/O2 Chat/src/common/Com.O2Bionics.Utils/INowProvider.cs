using System;

namespace Com.O2Bionics.Utils
{
    public interface INowProvider
    {
        DateTime UtcNow { get; }
    }
}