using System.Runtime.InteropServices;

namespace GeminiLab.Glos.ViMa {
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct IntegerFloatUnion {
        [FieldOffset(0)]
        public long Integer;
        [FieldOffset(0)]
        public double Float;
    }

    public partial struct GlosValue {
        private GlosValueType _type;
        public IntegerFloatUnion ValueNumber;
        public object? ValueObject;

        // setter triggers hash pre-calculation
        public GlosValueType Type {
            get => _type;
            set {
                _type = value;
                _hashCodeCalculated = value != GlosValueType.Table;
                if (_hashCodeCalculated) {
                    _hashCode = value switch {
                        GlosValueType.Integer => (int)ValueNumber.Integer,
                        GlosValueType.Float => (int)ValueNumber.Integer,
                        GlosValueType.Boolean => (int)ValueNumber.Integer,
                        GlosValueType.String => this.AssertString().GetHashCode(),
                        GlosValueType.Function => this.AssertFunction().GetHashCode(),
                        // Nil
                        _ => 0,
                    };
                }
            }
        }

        private bool _hashCodeCalculated;
        private int _hashCode;

        public override string ToString() {
            return Type switch {
                GlosValueType.Nil => "nil",
                GlosValueType.Integer => ValueNumber.Integer.ToString(),
                GlosValueType.String => (ValueObject as string)!,
                _ => ""
            };
        }
    }
}
