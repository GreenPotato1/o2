using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace O2.Black.Toolkit.Core
{
    public static class ContentHelper
    {
        public static StringContent GetStringContent(object obj)
        {
            var str = JsonConvert.SerializeObject(obj);
            return
                new StringContent(str, Encoding.UTF8, "application/json");
        }
    }
}