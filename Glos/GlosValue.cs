using System.Runtime.InteropServices;

namespace GeminiLab.Glos {
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
                preEvalHash();
            }
        }

        public override string ToString() {
            return Calculator.DebugStringify(this);
        }

        

        // TODO: find a better place for following method(s)
        public static bool TryGetMetamethodOfOperand(in GlosValue v, string name, out GlosValue fun) {
            fun = default;
            fun.SetNil();

            return v.Type == GlosValueType.Table && v.AssumeTable().TryGetMetamethod(name, out fun);
        }

        public static bool TryInvokeMetamethod(ref GlosValue dest, in GlosValue v, GlosViMa viMa, string name) {
            if (!TryGetMetamethodOfOperand(v, name, out var fun)) return false;

            var res = fun.Invoke(viMa, new[] { v });
            if (res.Length > 0) dest = res[0];
            else dest.SetNil();

            return true;
        }

        public static bool TryGetMetamethodOfOperand(in GlosValue x, in GlosValue y, string name, bool lookupMetatableOfLatterFirst, out GlosValue fun) {
            fun = default;
            fun.SetNil();

            if (!lookupMetatableOfLatterFirst) {
                if (x.Type == GlosValueType.Table && x.AssumeTable().TryGetMetamethod(name, out fun)) return true;
                if (y.Type == GlosValueType.Table && y.AssumeTable().TryGetMetamethod(name, out fun)) return true;
                return false;
            } else {
                if (y.Type == GlosValueType.Table && y.AssumeTable().TryGetMetamethod(name, out fun)) return true;
                if (x.Type == GlosValueType.Table && x.AssumeTable().TryGetMetamethod(name, out fun)) return true;
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
