using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FontLibBuilder
{

    class NativeMethods
    {
        [DllImport("gdi32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "SelectObject")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern uint GetGlyphOutline(IntPtr hdc, uint uChar, uint uFormat,
            out GLYPHMETRICS lpgm, uint cbBuffer, IntPtr lpvBuffer, ref MAT2 lpmat2);

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        [DllImport("gdi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateFont(int nHeight, int nWidth, int nEscapement,
            int nOrientation, int fnWeight, uint fdwItalic, uint fdwUnderline,
            uint fdwStrikeOut, uint fdwCharSet, uint fdwOutputPrecision, uint
            fdwClipPrecision, uint fdwQuality, uint fdwPitchAndFamily, string lpszFace);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }

    public struct MAT2
    {
        [MarshalAs(UnmanagedType.Struct)]
        public FIXED eM11;
        [MarshalAs(UnmanagedType.Struct)]
        public FIXED eM12;
        [MarshalAs(UnmanagedType.Struct)]
        public FIXED eM21;
        [MarshalAs(UnmanagedType.Struct)]
        public FIXED eM22;
    }

    [StructLayout(LayoutKind.Sequential)]

    public struct POINTFX
    {
        [MarshalAs(UnmanagedType.Struct)]
        public FIXED x;
        [MarshalAs(UnmanagedType.Struct)]
        public FIXED y;
    }

    [StructLayout(LayoutKind.Sequential)]

    public struct FIXED
    {
        public FIXED(short fract, short value)
        {
            this.fract = fract;
            this.value = value;
        }
        public short fract;
        public short value;
    }

    [StructLayout(LayoutKind.Sequential)]

    public struct GLYPHMETRICS
    {
        public int gmBlackBoxX;
        public int gmBlackBoxY;
        [MarshalAs(UnmanagedType.Struct)]
        public POINT gmptGlyphOrigin;
        public short gmCellIncX;
        public short gmCellIncY;
    }
}

