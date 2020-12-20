using System.Diagnostics.CodeAnalysis;

namespace GeminiLab.Glos {
    public partial struct GlosValue {
        public static bool BothNil(in GlosValue x, in GlosValue y) {
            return x.Type == GlosValueType.Nil && y.Type == GlosValueType.Nil;
        }

        public static bool BothInteger(in GlosValue x, in GlosValue y, out long xv, out long yv) {
            if (x.Type == GlosValueType.Integer && y.Type == GlosValueType.Integer) {
                xv = x.AssumeInteger();
                yv = y.AssumeInteger();

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
                xv = x.AssumeInteger();
            }

            if (x.Type == GlosValueType.Float) {
                xflag = true;
                xv = x.AssumeFloat();
            }

            if (y.Type == GlosValueType.Integer) {
                yflag = true;
                yv = y.AssumeInteger();
            }

            if (y.Type == GlosValueType.Float) {
                yflag = true;
                yv = y.AssumeFloat();
            }

            if (xflag && yflag) return true;

            xv = yv = default;
            return false;
        }

        public static bool BothBoolean(in GlosValue x, in GlosValue y, out bool xv, out bool yv) {
            if (x.Type == GlosValueType.Boolean && y.Type == GlosValueType.Boolean) {
                xv = x.AssumeBoolean();
                yv = y.AssumeBoolean();

                return true;
            }

            xv = yv = false;
            return false;
        }

        public static bool BothString(in GlosValue x, in GlosValue y,
                                      [NotNullWhen(true)] out string? xv,
                                      [NotNullWhen(true)] out string? yv) {
            if (x.Type == GlosValueType.String && y.Type == GlosValueType.String) {
                xv = x.AssumeString();
                yv = y.AssumeString();

                return true;
            }

            xv = yv = default;
            return false;
        }

        public static bool BothFunction(in GlosValue x, in GlosValue y,
                                        [NotNullWhen(true)] out GlosFunction? xv,
                                        [NotNullWhen(true)] out GlosFunction? yv) {
            if (x.Type == GlosValueType.Function && y.Type == GlosValueType.Function) {
                xv = x.AssumeFunction();
                yv = y.AssumeFunction();

                return true;
            }

            xv = yv = null;
            return false;
        }

        public static bool BothExternalPureFunction(in GlosValue x, in GlosValue y,
                                                [NotNullWhen(true)] out GlosExternalPureFunction? xv,
                                                [NotNullWhen(true)] out GlosExternalPureFunction? yv) {
            if (x.Type == GlosValueType.Function && y.Type == GlosValueType.Function) {
                xv = x.AssumeExternalPureFunction();
                yv = y.AssumeExternalPureFunction();

                return true;
            }

            xv = yv = null;
            return false;
        }
    }
}
