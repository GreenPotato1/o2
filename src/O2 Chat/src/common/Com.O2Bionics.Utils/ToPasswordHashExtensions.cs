using System;
using System.Security.Cryptography;
using System.Text;

namespace Com.O2Bionics.Utils
{
    public static class ToPasswordHashExtensions
    {
        public static string ToPasswordHash(this string password)
        {
            if (password == null) throw new ArgumentNullException("password");
            using (var sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.Unicode.GetBytes(password));
                var formatted = new StringBuilder(2 * hash.Length);
                foreach (var b in hash) formatted.AppendFormat("{0:X2}", b);
                return formatted.ToString();
            }
        }
    }
}