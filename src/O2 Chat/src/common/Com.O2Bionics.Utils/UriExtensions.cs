using System;

namespace Com.O2Bionics.Utils
{
    public static class UriExtensions
    {
        public static string GetFolder(this Uri uri)
        {
            if (uri == null) return "/";
            return new Uri(uri, ".").AbsolutePath;
        }
    }
}