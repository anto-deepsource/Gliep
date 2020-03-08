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
                GlosValueType.Boolean => this.AssumeBoolean() ? "true" : "false",
                _ => ""
            };
        }

        // TODO: find a better place for following method(s)
        public static bool TryGetMetamethodOfOperand(in GlosValue x, in GlosValue y, string name, bool lookupMetatableOfLatterFirst, out GlosValue fun) {
            fun = default;
            ref var rf = ref fun;
            rf.SetNil();

            if (!lookupMetatableOfLatterFirst) {
                if (x.Type == GlosValueType.Table && x.AssertTable().TryGetMetamethod(name, out fun)) return true;
                if (y.Type == GlosValueType.Table && y.AssertTable().TryGetMetamethod(name, out fun)) return true;
                return false;
            } else {
                if (y.Type == GlosValueType.Table && y.AssertTable().TryGetMetamethod(name, out fun)) return true;
                if (x.Type == GlosValueType.Table && x.AssertTable().TryGetMetamethod(name, out fun)) return true;
                return false;
            }
        }

        public static bool TryInvokeMetamethod(ref GlosValue dest, in GlosValue x, in GlosValue y, GlosViMa viMa, string name, bool lookupMetatableOfLatterFirst) {
            if (!TryGetMetamethodOfOperand(x, y, name, lookupMetatableOfLatterFirst, out var fun)) return false;

            var res = fun.Invoke(viMa, new[] { x, y });
            if (res.Length > 0) dest = res[0];
            else dest.SetNil();

            return true;
        }
    }
}
