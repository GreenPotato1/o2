using System;
using System.Collections.Generic;
using Com.O2Bionics.Utils;

namespace Com.O2Bionics.FeatureService
{
    public class FeatureServiceCallResult : Dictionary<string, string>
    {
        public FeatureServiceCallResult(IDictionary<string, string> values)
            : base(values, StringComparer.InvariantCultureIgnoreCase)
        {
        }

        public FeatureServiceCallResult(Exception e)
            : base(StringComparer.InvariantCultureIgnoreCase)
        {
            var exceptionType = e.GetType();
            var errorTypeText = "unexpectedError";

            if (exceptionType == typeof(FeatureInfoNotFoundException))
            {
                errorTypeText = "featureInfoNotFound";
            }
            else if (exceptionType == typeof(FeatureValueFormatException))
            {
                errorTypeText = "invalidValueFormat";
            }
            else if (exceptionType == typeof(ParameterValidationException))
            {
                errorTypeText = "invalidParameter";
            }
            else if (exceptionType == typeof(ProductCodeNotFoundException))
            {
                errorTypeText = "productCodeNotFound";
            }

            AddError(errorTypeText, e.Message);
        }

        private void AddError(string errorType, string errorMessage)
        {
            Add("error", errorType);
            Add("errorMessage", errorMessage);
        }

        public static Exception CreateException(string errorType, string errorMessage)
        {
            switch (errorType)
            {
                case "featureInfoNotFound":
                    return new FeatureInfoNotFoundException(errorMessage);
                case "invalidValueFormat":
                    throw new FeatureValueFormatException(errorMessage);
                case "invalidParameter":
                    throw new ParameterValidationException(errorMessage);
                case "productCodeNotFound":
                    throw new ProductCodeNotFoundException(errorMessage);
                default:
                    throw new Exception(errorMessage);
            }
        }
    }
}