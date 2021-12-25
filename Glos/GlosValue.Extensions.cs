using System;
using System.Runtime.CompilerServices;

namespace GeminiLab.Glos {
    public static class GlosValueExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref GlosValue SetNil(this ref GlosValue v) {
            v.ValueNumber.Integer = 0;
            v.ValueObject = null;
            v.Type = GlosValueType.Nil;

            return ref v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref GlosValue SetInteger(this ref GlosValue v, long value) {
            v.ValueNumber.Integer = value;
            v.ValueObject = null;
            v.Type = GlosValueType.Integer;

            return ref v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref GlosValue SetFloat(this ref GlosValue v, double value) {
            v.ValueNumber.Float = value;
            v.ValueObject = null;
            v.Type = GlosValueType.Float;

            return ref v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref GlosValue SetFloatByBinaryPresentation(this ref GlosValue v, ulong representation) {
            v.ValueNumber.Integer = unchecked((long) representation);
            v.ValueObject = null;
            v.Type = GlosValueType.Float;

            return ref v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref GlosValue SetBoolean(this ref GlosValue v, bool value) {
            v.ValueNumber.Integer = value ? -1L : 0L;
            v.ValueObject = null;
            v.Type = GlosValueType.Boolean;

            return ref v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref GlosValue SetTable(this ref GlosValue v, GlosTable value) {
            v.ValueNumber.Integer = 0;
            v.ValueObject = value;
            v.Type = GlosValueType.Table;

            return ref v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref GlosValue SetString(this ref GlosValue v, string value) {
            v.ValueNumber.Integer = 0;
            v.ValueObject = value;
            v.Type = GlosValueType.String;

            return ref v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref GlosValue SetFunction(this ref GlosValue v, GlosFunction fun) {
            v.ValueNumber.Integer = 0;
            v.ValueObject = fun;
            v.Type = GlosValueType.Function;

            return ref v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref GlosValue SetEFunction(this ref GlosValue v, GlosEFunction value) {
            v.ValueNumber.Integer = 0;
            v.ValueObject = value;
            v.Type = GlosValueType.EFunction;

            return ref v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref GlosValue SetVector(this ref GlosValue v, GlosVector value) {
            v.ValueNumber.Integer = 0;
            v.ValueObject = value;
            v.Type = GlosValueType.Vector;

            return ref v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref GlosValue SetPureEFunction(this ref GlosValue v, GlosPureEFunction value) {
            v.ValueNumber.Integer = 0;
            v.ValueObject = value;
            v.Type = GlosValueType.PureEFunction;

            return ref v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref GlosValue SetAsyncEFunction(this ref GlosValue v, IGlosAsyncEFunction value) {
            v.ValueNumber.Integer = 0;
            v.ValueObject = value;
            v.Type = GlosValueType.AsyncEFunction;

            return ref v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref GlosValue SetCoroutine(this ref GlosValue v, GlosCoroutine value) {
            v.ValueNumber.Integer = 0;
            v.ValueObject = value;
            v.Type = GlosValueType.Coroutine;

            return ref v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref GlosValue SetException(this ref GlosValue v, Exception value) {
            v.ValueNumber.Integer = 0;
            v.ValueObject = value;
            v.Type = GlosValueType.Exception;

            return ref v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AssertNil(this in GlosValue v) {
            if (v.Type != GlosValueType.Nil) throw new GlosValueTypeAssertionFailedException(v, GlosValueType.Nil);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long AssertInteger(this in GlosValue v) {
            if (v.Type == GlosValueType.Integer) return v.AssumeInteger();

            throw new GlosValueTypeAssertionFailedException(v, GlosValueType.Integer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double AssertFloat(this in GlosValue v) {
            if (v.Type == GlosValueType.Float) return v.AssumeFloat();

            throw new GlosValueTypeAssertionFailedException(v, GlosValueType.Float);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AssertBoolean(this in GlosValue v) {
            if (v.Type == GlosValueType.Boolean) return v.AssumeBoolean();

            throw new GlosValueTypeAssertionFailedException(v, GlosValueType.Boolean);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GlosTable AssertTable(this in GlosValue v) {
            if (v.Type == GlosValueType.Table) return v.AssumeTable(); // this method no longer checks ill-formed GlosValue, so do following methods

            throw new GlosValueTypeAssertionFailedException(v, GlosValueType.Table);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string AssertString(this in GlosValue v) {
            if (v.Type == GlosValueType.String) return v.AssumeString();

            throw new GlosValueTypeAssertionFailedException(v, GlosValueType.String);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GlosFunction AssertFunction(this in GlosValue v) {
            if (v.Type == GlosValueType.Function) return v.AssumeFunction();

            throw new GlosValueTypeAssertionFailedException(v, GlosValueType.Function);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GlosEFunction AssertEFunction(this in GlosValue v) {
            if (v.Type == GlosValueType.EFunction) return v.AssumeEFunction();

            throw new GlosValueTypeAssertionFailedException(v, GlosValueType.EFunction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GlosVector AssertVector(this in GlosValue v) {
            if (v.Type == GlosValueType.Vector) return v.AssumeVector();

            throw new GlosValueTypeAssertionFailedException(v, GlosValueType.Vector);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GlosPureEFunction AssertPureEFunction(this in GlosValue v) {
            if (v.Type == GlosValueType.PureEFunction) return v.AssumePureEFunction();

            throw new GlosValueTypeAssertionFailedException(v, GlosValueType.PureEFunction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IGlosAsyncEFunction AssertAsyncEFunction(this in GlosValue v) {
            if (v.Type == GlosValueType.AsyncEFunction) return v.AssumeAsyncEFunction();

            throw new GlosValueTypeAssertionFailedException(v, GlosValueType.AsyncEFunction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GlosCoroutine AssertCoroutine(this in GlosValue v) {
            if (v.Type == GlosValueType.Coroutine) return v.AssumeCoroutine();

            throw new GlosValueTypeAssertionFailedException(v, GlosValueType.Coroutine);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Exception AssertException(this in GlosValue v) {
            if (v.Type == GlosValueType.Exception) return v.AssumeException();

            throw new GlosValueTypeAssertionFailedException(v, GlosValueType.Exception);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long AssumeInteger(this in GlosValue v) => v.ValueNumber.Integer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double AssumeFloat(this in GlosValue v) => v.ValueNumber.Float;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AssumeBoolean(this in GlosValue v) => v.ValueNumber.Integer != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GlosTable AssumeTable(this in GlosValue v) => (GlosTable) v.ValueObject!;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string AssumeString(this in GlosValue v) => (string) v.ValueObject!;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GlosFunction AssumeFunction(this in GlosValue v) => (GlosFunction) v.ValueObject!;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GlosEFunction AssumeEFunction(this in GlosValue v) => (GlosEFunction) v.ValueObject!;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GlosVector AssumeVector(this in GlosValue v) => (GlosVector) v.ValueObject!;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GlosPureEFunction AssumePureEFunction(this in GlosValue v) => (GlosPureEFunction) v.ValueObject!;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IGlosAsyncEFunction AssumeAsyncEFunction(this in GlosValue v) => (IGlosAsyncEFunction) v.ValueObject!;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GlosCoroutine AssumeCoroutine(this in GlosValue v) => (GlosCoroutine) v.ValueObject!;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Exception AssumeException(this in GlosValue v) => (Exception) v.ValueObject!;

        public static double ToFloat(this in GlosValue v) {
            if (v.Type == GlosValueType.Float) return v.ValueNumber.Float;
            if (v.Type == GlosValueType.Integer) return v.ValueNumber.Integer;

            throw new GlosValueTypeAssertionFailedException(v, GlosValueType.Float);
        }

        public static bool Truthy(this in GlosValue v) {
            if (v.Type == GlosValueType.Nil) return false;
            if (v.Type == GlosValueType.Boolean) return v.ValueNumber.Integer != 0;

            return true;
        }

        public static bool Falsey(this in GlosValue v) {
            if (v.Type == GlosValueType.Nil) return true;
            if (v.Type == GlosValueType.Boolean) return v.ValueNumber.Integer == 0;

            return false;
        }

        public static bool IsNil(this in GlosValue v) {
            return v.Type == GlosValueType.Nil;
        }

        public static bool IsNonNil(this in GlosValue v) {
            return v.Type != GlosValueType.Nil;
        }

        public static bool IsInvokable(this in GlosValue v) {
            return v.Type == GlosValueType.Function
                || v.Type == GlosValueType.EFunction
                || v.Type == GlosValueType.PureEFunction
                || v.Type == GlosValueType.AsyncEFunction;
        }

        public static void AssertInvokable(this in GlosValue v) {
            if (!v.IsInvokable()) throw new GlosValueNotCallableException(v);
        }

        public static GlosValue ToGlosValue(this IGlosAsyncEFunction fun) {
            return GlosValue.NewAsyncEFunction(fun);
        }
    }
}
