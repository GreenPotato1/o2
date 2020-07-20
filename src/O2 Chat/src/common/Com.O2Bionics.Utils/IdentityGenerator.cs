using System.Threading;

namespace Com.O2Bionics.Utils
{
    public static class IdentityGenerator
    {
        private static long m_identity = -1;

        public static decimal GetNext()
        {
            return Interlocked.Decrement(ref m_identity);
        }

        public static bool IsNew(decimal identity)
        {
            return identity < 0;
        }
    }
}