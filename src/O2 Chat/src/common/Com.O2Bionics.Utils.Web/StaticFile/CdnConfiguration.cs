using System;
using System.Web.Configuration;

namespace Com.O2Bionics.Utils.Web.StaticFile
{
    public static class CdnConfiguration
    {
        public static readonly Lazy<string> CdnDomain =
            new Lazy<string>(() => WebConfigurationManager.AppSettings["CdnDomain"]);

        public static bool UseCdn
        {
            get { return !string.IsNullOrEmpty(CdnDomain.Value); }
        }
    }
}