namespace GeminiLab.Glos {
    public partial struct GlosValue {
        public static implicit operator GlosValue(long v) => NewInteger(v);
        public static implicit operator GlosValue(double v) => NewFloat(v);
        public static implicit operator GlosValue(bool v) => NewBoolean(v);
        public static implicit operator GlosValue(GlosExternalPureFunction v) => NewExternalFunction(v);
        public static implicit operator GlosValue(GlosTable v) => NewTable(v);
        public static implicit operator GlosValue(string v) => NewString(v);
        public static implicit operator GlosValue(GlosFunction v) => NewFunction(v);
        public static implicit operator GlosValue(GlosVector v) => NewVector(v);

        public static GlosValue NewNil() {
            var rv = new GlosValue();
            rv.SetNil();
            return rv;
        }

        public static GlosValue NewInteger(long value) {
            var rv = new GlosValue();
            rv.SetInteger(value);
            return rv;
        }

        public static GlosValue NewFloat(double value) {
            var rv = new GlosValue();
            rv.SetFloat(value);
            return rv;
        }

        public static GlosValue NewBoolean(bool value) {
            var rv = new GlosValue();
            rv.SetBoolean(value);
            return rv;
        }

        public static GlosValue NewExternalFunction(GlosExternalPureFunction value) {
            var rv = new GlosValue();
            rv.SetExternalFunction(value);
            return rv;
        }

        public static GlosValue NewTable(GlosTable value) {
            var rv = new GlosValue();
            rv.SetTable(value);
            return rv;
        }

        public static GlosValue NewString(string value) {
            var rv = new GlosValue();
            rv.SetString(value);
            return rv;
        }

        public static GlosValue NewFunction(GlosFunction value) {
            var rv = new GlosValue();
            rv.SetFunction(value);
            return rv;
        }

        public static GlosValue NewVector(GlosVector value) {
            var rv = new GlosValue();
            rv.SetVector(value);
            return rv;
        }

        public static void Swap(ref GlosValue a, ref GlosValue b) {
            GlosValue temp = a;
            a = b;
            b = temp;
        }
    }
}
