using System;

namespace O2.Certificate.API.Helper
{
    public static class HelperCertificate
    {
        public static int GetShortNumber(string certificationNumber)
        {
            return int.Parse(certificationNumber.Substring(0, 4));
        }

        public static DateTime GetDateCert(string certificationNumber)
        {
            var str = certificationNumber.Substring(4, 6);
            var stringData = str.Insert(2, ".").Insert(5, ".20");
            return DateTime.Parse(stringData);
        }
    }
}