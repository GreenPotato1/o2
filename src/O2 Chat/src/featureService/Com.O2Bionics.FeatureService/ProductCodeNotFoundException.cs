using System;

namespace Com.O2Bionics.FeatureService
{
    [Serializable]
    public class ProductCodeNotFoundException : Exception
    {
        public ProductCodeNotFoundException(string message)
            : base(message)
        {
        }
    }
}