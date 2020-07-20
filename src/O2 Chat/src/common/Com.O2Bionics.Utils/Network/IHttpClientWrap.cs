using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Com.O2Bionics.Utils.Network
{
    public interface IHttpClientWrap : IDisposable
    {
        Task<HttpResponseMessage> GetAsync(Uri requestUri);
    }
}