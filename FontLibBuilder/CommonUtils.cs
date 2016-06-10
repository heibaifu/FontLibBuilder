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

        static byte[] table = new byte[256];
        // x8 + x7 + x6 + x4 + x2 + 1
        const byte poly = 0xd5;

        public static byte ComputeCrc8(params byte[] bytes)
        {
            byte crc = 0;
            if (bytes != null && bytes.Length > 0)
            {
                foreach (byte b in bytes)
                {
                    crc = table[crc ^ b];
                }
            }
            return crc;
        }

        static CommonUtils()
        {
            for (int i = 0; i < 256; ++i)
            {
                int temp = i;
                for (int j = 0; j < 8; ++j)
                {
                    if ((temp & 0x80) != 0)
                    {
                        temp = (temp << 1) ^ poly;
                    }
                    else
                    {
                        temp <<= 1;
                    }
                }
                table[i] = (byte)temp;
            }
        }

        public static string ToHexString(this byte[] bytes)
        {
            return string.Concat(bytes.Select(b => b.ToString("X2")).ToArray());
        }

        public static string ToCStyleArray(this byte[] bytes)
        {
            return string.Concat(bytes.Select(b => $"0x{b.ToString("X2")}, "));
        }

        public static string ReplaceAll(this string source, Replacement[] replacements)
        {
            string result = source;
            foreach (var replace in replacements)
            {
                if (source.Contains(replace.OldValue))
                {
                    result = result.Replace(replace.OldValue, replace.NewValue());
                }
            }
            return result;
        }
    }

    public class Replacement
    {
        public string OldValue { get; set; }
        public Func<string> NewValue { get; set; }

        public Replacement(string oldValue, Func<string> newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    public class Pair<T1, T2>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }

        public static Pair<T1, T2> Create(T1 item1, T2 item2)
        {
            return new Pair<T1, T2>() { Item1 = item1, Item2 = item2 };
        }
    }
}
