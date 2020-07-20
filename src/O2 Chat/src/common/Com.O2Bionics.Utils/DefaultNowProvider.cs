using System;

namespace Com.O2Bionics.Utils
{
    public sealed class DefaultNowProvider : INowProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}