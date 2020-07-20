using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Jil;

namespace Com.O2Bionics.Utils.Network
{
    public static class HttpHelper
    {
        public static async Task<string> PostFirstSuccessfulString<T>(
            [NotNull] HttpClient httpClient,
            [NotNull] string[] uris,
            [NotNull] string path,
            T entity,
            [NotNull] Action<string, Exception> logAction,
            [NotNull] string failFormat)
        {
            var serialized = ReferenceEquals(null, entity) ? "null" : JSON.Serialize(entity, JsonSerializerBuilder.SkipNullJilOptions);
            using (var content = new StringContent(serialized))
            {
                var result = await PostFirstSuccessfulImpl(
                    httpClient,
                    uris,
                    path,
                    content,
                    logAction,
                    failFormat);
                return result;
            }
        }

        public static async Task<string> PostFirstSuccessfulForm<T>(
            [NotNull] HttpClient httpClient,
            [NotNull] string[] uris,
            [NotNull] string path,
            T entity,
            [NotNull] Action<string, Exception> logAction,
            [NotNull] string failFormat) where T : class
        {
            using (var content = entity.CreateContent())
            {
                var result = await PostFirstSuccessfulImpl(
                    httpClient,
                    uris,
                    path,
                    content,
                    logAction,
                    failFormat);
                return result;
            }
        }

        private static async Task<string> PostFirstSuccessfulImpl(
            [NotNull] HttpClient httpClient,
            [NotNull] string[] uris,
            [NotNull] string path,
            [NotNull] HttpContent content,
            [NotNull] Action<string, Exception> logAction,
            [NotNull] string failFormat)
        {
            if (null == httpClient)
                throw new ArgumentException(nameof(httpClient));
            if (null == uris || 0 == uris.Length)
                throw new ArgumentException(nameof(uris));
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException(nameof(path));
            if (null == content)
                throw new ArgumentException(nameof(content));
            if (null == logAction)
                throw new ArgumentException(nameof(logAction));
            if (string.IsNullOrEmpty(failFormat))
                throw new ArgumentException(nameof(failFormat));

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < uris.Length; i++)
                try
                {
                    var url = Path.Combine(uris[i], path);
                    var response = await PostImpl(httpClient, url, content);
                    return response;
                }
                catch (PostException
#if DEBUG
                    e
#endif
                )
                {
#if DEBUG
                    var report = $"Debug. Error='{e}'.";
                    Console.WriteLine(report);
#endif
                    throw;
                }
                catch (Exception e)
                {
#if DEBUG
                    var report = $"Debug. Uri='{uris[i]}', Error='{e}'.";
                    Console.WriteLine(report);
#endif
                    logAction(uris[i], e);
                }

            throw new Exception(string.Format(failFormat, uris.Length));
        }

        /// <summary>
        ///     Throw <seealso cref="PostException" /> when the returned value does not match the <paramref name="successCode" />
        ///     Unlike the other overloaded method, the <paramref name="content"/> is not serialized.
        /// </summary>
        public static async Task<string> PostString(
            [NotNull] HttpClient httpClient,
            [NotNull] string url,
            [NotNull] string content,
            HttpStatusCode successCode = HttpStatusCode.OK,
            bool readResponseData = true)
        {
            if (string.IsNullOrEmpty(content))
                throw new ArgumentNullException(nameof(content));

            using (var stringContent = new StringContent(content))
            {
                var result = await PostImpl(httpClient, url, stringContent, successCode, readResponseData);
                return result;
            }
        }

        /// <summary>
        ///     Throw <seealso cref="PostException" /> when the returned value does not match the <paramref name="successCode" />
        /// </summary>
        private static async Task<string> PostImpl(
            [NotNull] HttpClient httpClient,
            [NotNull] string url,
            [NotNull] HttpContent content,
            HttpStatusCode successCode = HttpStatusCode.OK,
            bool readResponseData = true)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            var responseMessage = await httpClient.PostAsync(url, content).ConfigureAwait(false);
            var data = readResponseData ? await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false) : null;
            if (successCode == responseMessage.StatusCode)
                return data;

            throw new PostException((int)responseMessage.StatusCode, data ?? string.Empty);
        }
    }
}