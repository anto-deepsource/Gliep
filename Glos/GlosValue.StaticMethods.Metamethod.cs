namespace GeminiLab.Glos {
    public partial struct GlosValue {
        public static bool TryGetMetamethodOfOperand(in GlosValue v, string name, out GlosValue fun) {
            fun = default;
            fun.SetNil();

            return v.Type == GlosValueType.Table && v.AssumeTable().TryGetMetamethod(name, out fun);
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
    }
}
