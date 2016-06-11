using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FontLibBuilder
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    class CharacterProperties
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Program.CharCodeLength)]
        public byte[] CharCode;
        public ushort NextOccurence;
        public byte Width;
    }
}
