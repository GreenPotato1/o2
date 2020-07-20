using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Com.O2Bionics.Utils.Network
{
    public sealed class HttpClientWrap : IHttpClientWrap
    {
        public readonly HttpClient HttpClient = new HttpClient();

        public async Task<HttpResponseMessage> GetAsync(Uri uri)
        {
            var result = await HttpClient.GetAsync(uri).ConfigureAwait(false);
            return result;
        }

        public void Dispose()
        {
            HttpClient.Dispose();
        }
    }
}