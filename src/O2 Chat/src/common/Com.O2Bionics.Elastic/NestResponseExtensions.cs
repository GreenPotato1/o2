using System.Diagnostics;
using Elasticsearch.Net;
using JetBrains.Annotations;
using Nest;

namespace Com.O2Bionics.Elastic
{
    internal static class NestResponseExtensions
    {
        public static string BuildErrorMessage([NotNull] this IResponse response)
        {
            var result = response.DebugInformation;
            var error = response.ServerError?.Error;
            if (null != error)
                result += $"\n\nServerError.Error='{error}'.";
            Debug.Assert(!string.IsNullOrEmpty(result));
            return result;
        }

        public static string BuildErrorMessage([NotNull] this StringResponse response)
        {
            var result = response.DebugInformation;
            if (response.TryGetServerError(out var serverError) && serverError?.Error != null)
                result += $"\n\nServerError.Error='{serverError.Error}'.";
            Debug.Assert(!string.IsNullOrEmpty(result));
            return result;
        }
    }
}