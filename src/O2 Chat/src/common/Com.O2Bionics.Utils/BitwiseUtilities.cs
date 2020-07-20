using System.Diagnostics;

namespace Com.O2Bionics.Utils
{
    public static class BitwiseUtilities
    {
        public static bool IsSet(byte b, int pos)
        {
            Debug.Assert(0 <= pos && pos < 8);
            var bit = (b >> pos) & 1;
            var result = 0 != bit;
            return result;
        }

        //public static byte SetByte(byte b, int pos)
        //{
        //    var result = b | (1 << pos);
        //    return (byte)result;
        //}

        public static byte SetByte(byte b, int pos, bool value)
        {
            Debug.Assert(0 <= pos && pos < 8);
            var inv = b ^ (value ? -1 : 0);
            var result = b ^ (inv & (1 << pos));
            return (byte)result;
        }

        //public static byte ClearBit(byte b, int pos)
        //{
        //    var result = b & ~(1 << pos);
        //    return (byte)result;
        //}
    }
}