using System;

namespace Com.O2Bionics.FeatureService
{
    [Serializable]
    public class FeatureValueFormatException : Exception
    {
        public FeatureValueFormatException(int userId, string featureCode, FormatException inner)
            : base(string.Format("Feature value can't be parsed as a number. userId={0}, featureCode={1}", userId, featureCode), inner)
        {
        }

        public FeatureValueFormatException(string message)
            : base(message)
        {
        }
    }
}