using System.Diagnostics.CodeAnalysis;

namespace GeminiLab.Glos.ViMa {
    public partial struct GlosValue {
        public static void ThrowIfOperandsDelimiter(in GlosValue x) {
            if (x.Type == GlosValueType.Delimiter) throw new InvalidOperandTypeException();
        }

        public static void ThrowIfOperandsDelimiter(in GlosValue x, in GlosValue y) {
            if (x.Type == GlosValueType.Delimiter || y.Type == GlosValueType.Delimiter) throw new InvalidOperandTypeException();
        }

        public static bool BothNil(in GlosValue x, in GlosValue y) {
            return x.Type == GlosValueType.Nil && y.Type == GlosValueType.Nil;
        }

        public static bool BothInteger(in GlosValue x, in GlosValue y, out long xv, out long yv) {
            if (x.Type == GlosValueType.Integer && y.Type == GlosValueType.Integer) {
                xv = x.ValueNumber.Integer;
                yv = y.ValueNumber.Integer;

                return true;
            }

            xv = yv = default;
            return false;
        }

        public static bool BothNumeric(in GlosValue x, in GlosValue y, out double xv, out double yv) {
            bool xflag = false, yflag = false;
            xv = yv = default;

            if (x.Type == GlosValueType.Integer) {
                xflag = true;
                xv = x.ValueNumber.Integer;
            }

            if (x.Type == GlosValueType.Float) {
                xflag = true;
                xv = x.ValueNumber.Float;
            }

            if (y.Type == GlosValueType.Integer) {
                yflag = true;
                yv = y.ValueNumber.Integer;
            }

            if (y.Type == GlosValueType.Float) {
                yflag = true;
                yv = y.ValueNumber.Float;
            }

            if (xflag && yflag) return true;

            xv = yv = default;
            return false;
        }

        public static bool BothBoolean(in GlosValue x, in GlosValue y, out bool xv, out bool yv) {
            if (x.Type == GlosValueType.Boolean && y.Type == GlosValueType.Boolean) {
                xv = x.Truthy();
                yv = y.Truthy();

                return true;
            }

            xv = yv = false;
            return false;
        }

        public static bool BothString(in GlosValue x, in GlosValue y, [NotNullWhen(true)] out string? xv,
            [NotNullWhen(true)] out string? yv) {
            if (x.Type == GlosValueType.String && y.Type == GlosValueType.String) {
                xv = x.AssertString();
                yv = y.AssertString();

                return true;
            }

            xv = yv = default;
            return false;
        }

        public static bool BothFunction(in GlosValue x, in GlosValue y, [NotNullWhen(true)] out GlosFunction? xv,
            [NotNullWhen(true)] out GlosFunction? yv) {
            if (x.Type == GlosValueType.Function && y.Type == GlosValueType.Function) {
                xv = x.AssertFunction();
                yv = y.AssertFunction();

                return true;
            }

            xv = yv = null;
            return false;
        }

        public static bool BothExternalFunction(in GlosValue x, in GlosValue y,
            [NotNullWhen(true)] out GlosExternalFunction? xv, [NotNullWhen(true)] out GlosExternalFunction? yv) {
            if (x.Type == GlosValueType.Function && y.Type == GlosValueType.Function) {
                xv = x.AssertExternalFunction();
                yv = y.AssertExternalFunction();

                return true;
            }

            xv = yv = null;
            return false;
        }
    }
}