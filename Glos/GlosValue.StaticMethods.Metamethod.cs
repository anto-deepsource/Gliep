namespace GeminiLab.Glos {
    public partial struct GlosValue {
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

        public static bool TryGetMetamethodOfOperand(in GlosValue x, in GlosValue y, string name,
            bool lookupMetatableOfLatterFirst, out GlosValue fun) {
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

        public static bool TryInvokeMetamethod(ref GlosValue dest, in GlosValue x, in GlosValue y, GlosViMa viMa,
            string name, bool lookupMetatableOfLatterFirst) {
            if (!TryGetMetamethodOfOperand(x, y, name, lookupMetatableOfLatterFirst, out var fun)) return false;

            var res = fun.Invoke(viMa, new[] { x, y });
            if (res.Length > 0) dest = res[0];
            else dest.SetNil();

            return true;
        }
    }
}