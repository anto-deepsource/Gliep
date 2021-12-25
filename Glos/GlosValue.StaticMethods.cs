using System;

namespace GeminiLab.Glos {
    public partial struct GlosValue {
        public static implicit operator GlosValue(long v) => NewInteger(v);
        public static implicit operator GlosValue(double v) => NewFloat(v);
        public static implicit operator GlosValue(bool v) => NewBoolean(v);
        public static implicit operator GlosValue(GlosTable v) => NewTable(v);
        public static implicit operator GlosValue(string v) => NewString(v);
        public static implicit operator GlosValue(GlosFunction v) => NewFunction(v);
        public static implicit operator GlosValue(GlosEFunction v) => NewEFunction(v);
        public static implicit operator GlosValue(GlosVector v) => NewVector(v);
        public static implicit operator GlosValue(GlosPureEFunction v) => NewPureEFunction(v);
        public static implicit operator GlosValue(GlosCoroutine v) => NewCoroutine(v);
        public static implicit operator GlosValue(Exception v) => NewException(v);

        // Unfortunately, C# do not allow this.
        // public static implicit operator GlosValue(IGlosAsyncEFunction v) => NewAsyncEFunction(v);
        // So in GlosValue.Extensions.cs we have an extension method instead.

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

        public static GlosValue NewEFunction(GlosEFunction value) {
            var rv = new GlosValue();
            rv.SetEFunction(value);
            return rv;
        }

        public static GlosValue NewVector(GlosVector value) {
            var rv = new GlosValue();
            rv.SetVector(value);
            return rv;
        }

        public static GlosValue NewPureEFunction(GlosPureEFunction value) {
            var rv = new GlosValue();
            rv.SetPureEFunction(value);
            return rv;
        }

        public static GlosValue NewAsyncEFunction(IGlosAsyncEFunction value) {
            var rv = new GlosValue();
            rv.SetAsyncEFunction(value);
            return rv;
        }

        public static GlosValue NewCoroutine(GlosCoroutine value) {
            var rv = new GlosValue();
            rv.SetCoroutine(value);
            return rv;
        }

        public static GlosValue NewException(Exception value) {
            var rv = new GlosValue();
            rv.SetException(value);
            return rv;
        }

        public static void Swap(ref GlosValue a, ref GlosValue b) {
            GlosValue temp = a;
            a = b;
            b = temp;
        }
    }
}
