using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontLibBuilder
{
    public static class CommonUtils
    {
        // Byte reverse code ported from
        // https://stackoverflow.com/questions/2602823/in-c-c-whats-the-simplest-way-to-reverse-the-order-of-bits-in-a-byte
        private static byte[] byteReverseLookup = {
            0x0, 0x8, 0x4, 0xc, 0x2, 0xa, 0x6, 0xe,
            0x1, 0x9, 0x5, 0xd, 0x3, 0xb, 0x7, 0xf, };

        public static byte ReverseByte(byte n)
        {
            // Reverse the top and bottom nibble then swap them.
            return (byte)((byteReverseLookup[n & 0xf] << 4) | byteReverseLookup[n >> 4]);
        }

        public static byte[] ToByteArray(this BitArray bits)
        {
            byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }
    }
}
