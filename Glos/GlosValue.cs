using System.Runtime.InteropServices;

namespace GeminiLab.Glos {
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct IntegerFloatUnion {
        [FieldOffset(0)] public long   Integer;
        [FieldOffset(0)] public double Float;
    }

    [StructLayout(LayoutKind.Auto)]
    public partial struct GlosValue {
        public GlosValueType     Type;
        public IntegerFloatUnion ValueNumber;
        public object?           ValueObject;

        // hash assigned or calculated
        private bool _hashAssigned;
        private ulong _hashCode;

        public override string ToString() {
            return Calculator.DebugStringify(this);
        }
    }
}
