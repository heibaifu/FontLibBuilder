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
            // Ported from http://download.csdn.net/download/yuanyunzhu/5513441
            uint len;
            byte[] buffer;
            GLYPHMETRICS metrics;

            Bitmap bmp = new Bitmap(1, 1);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                IntPtr hdc = g.GetHdc();
                IntPtr fontHandle = font.ToHfont();
                NativeMethods.SelectObject(hdc, fontHandle);

                MAT2 mat2 = new MAT2()
                {
                    eM11 = new FIXED(0, 1),
                    eM12 = new FIXED(0, 0),
                    eM21 = new FIXED(0, 0),
                    eM22 = new FIXED(0, 1)
                };

                len = NativeMethods.GetGlyphOutline(hdc,
                    (uint)ch,
                    1,  // GGO_BITMAP
                    out metrics,
                    0,
                    IntPtr.Zero,
                    ref mat2
                );

                buffer = new byte[len];

                if (len != 0)
                {
                    GCHandle bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

                    NativeMethods.GetGlyphOutline(
                        hdc,
                        (uint)ch,
                        1,
                        out metrics,
                        len,
                        bufferHandle.AddrOfPinnedObject(),
                        ref mat2);

                    bufferHandle.Free();
                }

                g.ReleaseHdc(hdc);
                g.Flush();
            }

            uint rows = len / 4;

            int dataSize = size * size / 8;
            if (size * size % 8 != 0)
            {
                dataSize += 1;
            }

            BitArray orig = new BitArray(buffer);
            //BitArray arr = new BitArray(dataSize * 8, false);

            byte[] result = new byte[dataSize];

            // 空格没有字模数据，直接跳过，并取其高度一半作为宽度。
            if (len == 0)
            {
                metrics.gmBlackBoxX = size / 2;
            }
            else
            {
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
                        int sourceIndex = i * bitCount + j;
                        // 有时会出问题
                        if (sourceIndex < orig.Length)
                        {
                            if (orig[sourceIndex])
                                result[byteIndex] |= mask;
                            else
                                result[byteIndex] &= (byte)~mask;
                        }
                        else
                        {
                            Log.C($"出现问题，字模数据长度不够。当前位置：{sourceIndex}，实际长度：{orig.Length}");
                        }
                    }
                }
            }
            return Tuple.Create(result, metrics.gmBlackBoxX);
        }

    }
}
