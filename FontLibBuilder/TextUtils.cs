using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FontLibBuilder
{
    class TextUtils
    {
        /// <summary>
        /// 生成指定字符的图像数据。
        /// </summary>
        /// <param name="ch">要生成的字符。</param>
        /// <param name="font">要使用的字体。</param>
        /// <param name="size">目标字符大小。</param>
        /// <returns>一个二元组，第一项表示生成的数据，第二项表示字符的实际宽度。</returns>
        public static Tuple<byte[], int> GenerateImageForChar(char ch, Font font, int size)
        {
            Bitmap bmp = new Bitmap(100, 100);
            Graphics g = Graphics.FromImage(bmp);
            // Ported from http://download.csdn.net/download/yuanyunzhu/5513441
            IntPtr hdc = g.GetHdc();
            IntPtr fontHandle = font.ToHfont();
            NativeMethods.SelectObject(hdc, fontHandle);

            GLYPHMETRICS metrics;
            MAT2 mat2 = new MAT2()
            {
                eM11 = new FIXED(0, 1),
                eM12 = new FIXED(0, 0),
                eM21 = new FIXED(0, 0),
                eM22 = new FIXED(0, 1)
            };

            uint len = NativeMethods.GetGlyphOutline(hdc,
                (uint)ch,
                1,  // GGO_BITMAP
                out metrics,
                0,
                IntPtr.Zero,
                ref mat2
            );

            byte[] buffer = new byte[len];
            GCHandle bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            NativeMethods.GetGlyphOutline(
                hdc,
                (uint)ch,
                1,
                out metrics,
                len,
                bufferHandle.AddrOfPinnedObject(),
                ref mat2);

            g.ReleaseHdc(hdc);
            g.Flush();

            uint rows = len / 4;

            int dataSize = size * size / 8;
            if (size * size % 8 != 0)
            {
                dataSize += 1;
            }

            BitArray orig = new BitArray(buffer);
            //BitArray arr = new BitArray(dataSize * 8, false);

            byte[] result = new byte[dataSize];

            // Ported from http://bbs.csdn.net/topics/370061244
            int nByteCount = ((metrics.gmBlackBoxX + 31) >> 5) << 2;

            int bitCount = nByteCount * 8;
            for (int i = 0; i < metrics.gmBlackBoxY && i < size; i++)
            {
                for (int j = 0; j < bitCount && j < size; j++)
                {
                    int index = i * size + j;
                    int byteIndex = index / 8;
                    int bitInByteIndex = index % 8;
                    byte mask = (byte)(1 << bitInByteIndex);
                    if (orig[i * bitCount + j])
                        result[byteIndex] |= mask;
                    else
                        result[byteIndex] &= (byte)~mask;
                }
            }
            return new Tuple<byte[], int>(result, metrics.gmBlackBoxX);
        }

    }
}
