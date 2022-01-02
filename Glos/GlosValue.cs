using System.Runtime.InteropServices;

namespace GeminiLab.Glos {
    [StructLayout(LayoutKind.Sequential)]
    public struct GlosValueNumber {
        public long   Integer;
        public double Float;
    }

    [StructLayout(LayoutKind.Sequential)]
    public partial struct GlosValue {
        public GlosValueType   Type;
        public GlosValueNumber ValueNumber;
        public object?         ValueObject;

        public override string ToString() {
            return Calculator.DebugStringify(this);
        }
    }
}
