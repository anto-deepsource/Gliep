using System;
using System.Runtime.InteropServices;

namespace GeminiLab.Glos.Serialization {
    [StructLayout(LayoutKind.Explicit)]
    public struct GlosUnitHeader {
        public const uint MagicValue = 0x44614c47;

        [FieldOffset(0x00)] public uint   Magic;
        [FieldOffset(0x04)] public ushort FileVersionMinor;
        [FieldOffset(0x06)] public ushort FileVersionMajor;
        [FieldOffset(0x08)] public uint   Reserved;
        [FieldOffset(0x0c)] public uint   FirstSectionOffset;
    }

    public enum GlosUnitSectionType : ushort {
        FunctionSection = 0x0001,
        StringSection   = 0x0002,
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct GlosUnitSectionHeader {
        [FieldOffset(0x00)] public uint   SectionHeaderLength;
        [FieldOffset(0x04)] public uint   SectionLength;
        [FieldOffset(0x08)] public uint   NextOffset;
        [FieldOffset(0x0c)] public ushort SectionType;
        [FieldOffset(0x0e)] public ushort Reserved;
    }

    [Flags]
    public enum GlosUnitFunctionFlags : uint {
        Default = 0,
        
        Entry = 0b1,
    }
    
    [StructLayout(LayoutKind.Explicit)]
    public struct GlosUnitFunctionHeader {
        [FieldOffset(0x00)] public uint FunctionHeaderSize;
        [FieldOffset(0x04)] public uint CodeLength;
        [FieldOffset(0x08)] public uint Flags;
        [FieldOffset(0x0c)] public uint VariableInContextCount;
        [FieldOffset(0x10)] public uint LocalVariableCount;
    }
}
