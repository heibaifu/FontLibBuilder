#define PRINT_TO_CONSOLE

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
        /// <param name="anchorToByte">指示是否要将每行 / 列对齐到字节。</param>
        /// <param name="orientation">指示生成字符的方向。此选项暂时无效，所有结果都垂直排列！</param>
        /// <returns>一个二元组，第一项表示生成的数据，第二项表示字符的实际宽度。</returns>
        public static Tuple<byte[], int> GenerateImageForChar(char ch, Font font, int size, bool anchorToByte, OrderOrientation orientation)
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
            int actualWidth;
            if (len == 0)
            {
                actualWidth = size / 2;
            }
            else
            {
                // Ported from http://bbs.csdn.net/topics/370061244
                // 生成结果中每行的字节数。
                int nByteCount = ((metrics.gmBlackBoxX + 31) >> 5) << 2;

                int bitCount = nByteCount * 8;

                // 以下代码可以原样将文字打入控制台，供参考：
                // （若要取消，仅需去除文件头的#define）
#if PRINT_TO_CONSOLE
                Console.WriteLine($"=== Begin printing character '{ch}' ===");
                for (int i = 0; i < metrics.gmBlackBoxY && i < size; i++)
                {
                    for (int j = 0; j < bitCount && j < size; j++)
                    {
                        Console.Write(orig[i * bitCount + j] ? " *" : "  ");
                    }
                    Console.WriteLine("|");
                }
                Console.WriteLine($"=== End ===");
#endif

                int currentResultBitIndex = 0;

                // 目前的代码仅支持以垂直方式对齐。
                for (int j = 0; j < bitCount && j < size; j++)
                {
                    for (int i = 0; i < size; i++, currentResultBitIndex++)
                    {
                        // 如果已经超出实际大小
                        if (i >= metrics.gmBlackBoxY)
                        {
                            continue;
                        }
                        int index = currentResultBitIndex;

                        /* switch (orientation)
                        {
                            case OrderOrientation.Vertical:
                                //index = i * size + j;
                                break;
                            case OrderOrientation.Horizontal:
                                throw new NotImplementedException("Not supported yet.");
                            default:
                                throw new Exception();
                        }*/

                        int byteIndex = index / 8;
                        int bitInByteIndex = index % 8;
                        byte mask = (byte)(1 << bitInByteIndex);

                        int sourceIndex = i * bitCount + j;
                        if (orig[sourceIndex])
                            result[byteIndex] |= mask;
                        else
                            result[byteIndex] &= (byte)~mask;
                    }

                    if(currentResultBitIndex % 8 != 0)
                    {
                        // 对齐到字节
                        currentResultBitIndex = (currentResultBitIndex / 8 + 1) * 8;
                    }
                }
                if (metrics.gmBlackBoxX > (size / 2))
                {
                    actualWidth = size;
                }
                else
                {
                    actualWidth = size / 2;
                }
            }
            return Tuple.Create(result, actualWidth);
        }

        public static int GetImageSize(int size, bool anchorToByte)
        {
            int dataSize;
            if (anchorToByte)
            {
                dataSize = size * size / 8;
                if (size * size % 8 != 0)
                {
                    dataSize += 1;
                }
            }
            else
            {
                int lineSize = size / 8;
                if (size % 8 != 0)
                {
                    lineSize += 1;
                }
                dataSize = lineSize * size;
            }
            return dataSize;
        }
    }

    /// <summary>
    /// 指示图像的方向。
    /// </summary>
    enum OrderOrientation
    {
        /// <summary>
        /// 表示图像将被水平（横向）填充至数组。
        /// 例如，数组第一位为第一行第一列，第二位为第一行第二列，依此类推。
        /// </summary>
        Vertical,
        /// <summary>
        /// 表示图像将被竖直（纵向）填充至数组。
        /// 例如，数组第一位为第一行第一列，第二位为第二行第一列，依此类推。
        /// </summary>
        Horizontal
    }
}
