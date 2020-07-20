using System;

namespace Com.O2Bionics.FeatureService
{
    [Serializable]
    public class FeatureInfoNotFoundException : Exception
    {
        public FeatureInfoNotFoundException(string message)
            : base(message)
        {
        }
    }
}