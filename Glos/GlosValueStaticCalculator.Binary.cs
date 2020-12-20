using System;

namespace GeminiLab.Glos {
    public static partial class GlosValueStaticCalculator {
        public static void Add(ref GlosValue dest, in GlosValue x, in GlosValue y) {
            unchecked {
                if (GlosValue.BothInteger(x, y, out var xint, out var yint)) dest.SetInteger(xint + yint);
                else if (GlosValue.BothNumeric(x, y, out var xfloat, out var yfloat)) dest.SetFloat(xfloat + yfloat);
                else if (GlosValue.BothString(x, y, out var xstr, out var ystr)) dest.SetString(xstr + ystr);
                else throw new GlosInvalidBinaryOperandTypeException(GlosOp.Add, x, y);
            }
        }

        public static void Sub(ref GlosValue dest, in GlosValue x, in GlosValue y) {
            unchecked {
                if (GlosValue.BothInteger(x, y, out var xint, out var yint)) dest.SetInteger(xint - yint);
                else if (GlosValue.BothNumeric(x, y, out var xfloat, out var yfloat)) dest.SetFloat(xfloat - yfloat);
                else throw new GlosInvalidBinaryOperandTypeException(GlosOp.Sub, x, y);
            }
        }

        public static void Mul(ref GlosValue dest, in GlosValue x, in GlosValue y) {
            unchecked {
                if (GlosValue.BothInteger(x, y, out var xint, out var yint)) dest.SetInteger(xint * yint);
                else if (GlosValue.BothNumeric(x, y, out var xfloat, out var yfloat)) dest.SetFloat(xfloat * yfloat);

                else throw new GlosInvalidBinaryOperandTypeException(GlosOp.Mul, x, y);
            }
        }

        public static void Div(ref GlosValue dest, in GlosValue x, in GlosValue y) {
            unchecked {
                if (GlosValue.BothInteger(x, y, out var xint, out var yint)) dest.SetInteger(xint / yint);
                else if (GlosValue.BothNumeric(x, y, out var xfloat, out var yfloat)) dest.SetFloat(xfloat / yfloat);
                else throw new GlosInvalidBinaryOperandTypeException(GlosOp.Div, x, y);
            }
        }

        public static void Mod(ref GlosValue dest, in GlosValue x, in GlosValue y) {
            unchecked {
                if (GlosValue.BothInteger(x, y, out var xint, out var yint)) dest.SetInteger(xint % yint);
                else throw new GlosInvalidBinaryOperandTypeException(GlosOp.Mod, x, y);
            }
        }

        public static void Lsh(ref GlosValue dest, in GlosValue x, in GlosValue y) {
            unchecked {
                if (GlosValue.BothInteger(x, y, out var xint, out var yint)) dest.SetInteger((long) ((ulong) xint << (int) (0x3f & (uint) yint)));
                else throw new GlosInvalidBinaryOperandTypeException(GlosOp.Lsh, x, y);
            }
        }

        public static void Rsh(ref GlosValue dest, in GlosValue x, in GlosValue y) {
            unchecked {
                if (GlosValue.BothInteger(x, y, out var xint, out var yint)) dest.SetInteger((long) ((ulong) xint >> (int) (0x3f & (uint) yint)));
                else throw new GlosInvalidBinaryOperandTypeException(GlosOp.Rsh, x, y);
            }
        }

        public static void And(ref GlosValue dest, in GlosValue x, in GlosValue y) {
            unchecked {
                if (GlosValue.BothInteger(x, y, out var xint, out var yint)) dest.SetInteger((long) ((ulong) xint & (ulong) yint));
                else if (GlosValue.BothBoolean(x, y, out var xbool, out var ybool)) dest.SetBoolean(xbool && ybool);
                else throw new GlosInvalidBinaryOperandTypeException(GlosOp.And, x, y);
            }
        }

        public static void Orr(ref GlosValue dest, in GlosValue x, in GlosValue y) {
            unchecked {
                if (GlosValue.BothInteger(x, y, out var xint, out var yint)) dest.SetInteger((long) ((ulong) xint | (ulong) yint));
                else if (GlosValue.BothBoolean(x, y, out var xbool, out var ybool)) dest.SetBoolean(xbool || ybool);
                else throw new GlosInvalidBinaryOperandTypeException(GlosOp.Orr, x, y);
            }
        }

        public static void Xor(ref GlosValue dest, in GlosValue x, in GlosValue y) {
            unchecked {
                if (GlosValue.BothInteger(x, y, out var xint, out var yint)) dest.SetInteger((long) ((ulong) xint ^ (ulong) yint));
                else if (GlosValue.BothBoolean(x, y, out var xbool, out var ybool)) dest.SetBoolean(xbool ^ ybool);
                else throw new GlosInvalidBinaryOperandTypeException(GlosOp.Xor, x, y);
            }
        }

        public static void Lss(ref GlosValue dest, in GlosValue x, in GlosValue y) {
            if (GlosValue.BothInteger(x, y, out var xint, out var yint)) dest.SetBoolean(xint < yint);
            else if (GlosValue.BothNumeric(x, y, out var xfloat, out var yfloat)) dest.SetBoolean(xfloat < yfloat);
            else if (GlosValue.BothString(x, y, out var xstr, out var ystr)) dest.SetBoolean(string.Compare(xstr, ystr, StringComparison.Ordinal) < 0);
            else throw new GlosInvalidBinaryOperandTypeException(GlosOp.Lss, x, y);
        }

        public static void Gtr(ref GlosValue dest, in GlosValue x, in GlosValue y) {
            if (GlosValue.BothInteger(x, y, out var xint, out var yint)) dest.SetBoolean(xint > yint);
            else if (GlosValue.BothNumeric(x, y, out var xfloat, out var yfloat)) dest.SetBoolean(xfloat > yfloat);
            else if (GlosValue.BothString(x, y, out var xstr, out var ystr)) dest.SetBoolean(string.Compare(xstr, ystr, StringComparison.Ordinal) > 0);
            else throw new GlosInvalidBinaryOperandTypeException(GlosOp.Gtr, x, y);
        }

        public static void Leq(ref GlosValue dest, in GlosValue x, in GlosValue y) {
            GlosValue temp = default;

            if (GlosValue.BothInteger(x, y, out var xint, out var yint)) dest.SetBoolean(xint <= yint);
            else if (GlosValue.BothNumeric(x, y, out var xfloat, out var yfloat)) dest.SetBoolean(xfloat <= yfloat);
            else if (GlosValue.BothString(x, y, out var xstr, out var ystr)) dest.SetBoolean(string.Compare(xstr, ystr, StringComparison.Ordinal) <= 0);
            else throw new GlosInvalidBinaryOperandTypeException(GlosOp.Leq, x, y);
        }

        public static void Geq(ref GlosValue dest, in GlosValue x, in GlosValue y) {
            GlosValue temp = default;

            if (GlosValue.BothInteger(x, y, out var xint, out var yint)) dest.SetBoolean(xint >= yint);
            else if (GlosValue.BothNumeric(x, y, out var xfloat, out var yfloat)) dest.SetBoolean(xfloat >= yfloat);
            else if (GlosValue.BothString(x, y, out var xstr, out var ystr)) dest.SetBoolean(string.Compare(xstr, ystr, StringComparison.Ordinal) >= 0);
            else throw new GlosInvalidBinaryOperandTypeException(GlosOp.Geq, x, y);
        }

        public static void Equ(ref GlosValue dest, in GlosValue x, in GlosValue y) {
            dest.SetBoolean(Equals(x, y));
        }

        public static void Neq(ref GlosValue dest, in GlosValue x, in GlosValue y) {
            dest.SetBoolean(!Equals(x, y));
        }

        public static bool Equals(in GlosValue x, in GlosValue y) {
            if (GlosValue.BothNil(x, y)) return true;
            if (GlosValue.BothInteger(x, y, out var xint, out var yint)) return xint == yint;
            if (GlosValue.BothNumeric(x, y, out var xfloat, out var yfloat)) return xfloat == yfloat;
            if (GlosValue.BothString(x, y, out var xstring, out var ystring)) return xstring == ystring;
            if (GlosValue.BothBoolean(x, y, out var xbool, out var ybool)) return xbool == ybool;
            if (GlosValue.BothFunction(x, y, out var xfun, out var yfun)) return xfun.Prototype == yfun.Prototype && xfun.ParentContext == yfun.ParentContext;
            
            return x.Type == y.Type && ReferenceEquals(x.ValueObject, y.ValueObject);
        }
        
        public static void ExecuteBinaryOperation(ref GlosValue dest, in GlosValue x, in GlosValue y, GlosOp op) {
            switch (op) {
            case GlosOp.Add:
                Add(ref dest, in x, in y);
                break;
            case GlosOp.Sub:
                Sub(ref dest, in x, in y);
                break;
            case GlosOp.Mul:
                Mul(ref dest, in x, in y);
                break;
            case GlosOp.Div:
                Div(ref dest, in x, in y);
                break;
            case GlosOp.Mod:
                Mod(ref dest, in x, in y);
                break;
            case GlosOp.Lsh:
                Lsh(ref dest, in x, in y);
                break;
            case GlosOp.Rsh:
                Rsh(ref dest, in x, in y);
                break;
            case GlosOp.And:
                And(ref dest, in x, in y);
                break;
            case GlosOp.Orr:
                Orr(ref dest, in x, in y);
                break;
            case GlosOp.Xor:
                Xor(ref dest, in x, in y);
                break;
            case GlosOp.Gtr:
                Gtr(ref dest, in x, in y);
                break;
            case GlosOp.Lss:
                Lss(ref dest, in x, in y);
                break;
            case GlosOp.Geq:
                Geq(ref dest, in x, in y);
                break;
            case GlosOp.Leq:
                Leq(ref dest, in x, in y);
                break;
            case GlosOp.Equ:
                Equ(ref dest, in x, in y);
                break;
            case GlosOp.Neq:
                Neq(ref dest, in x, in y);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(op));
            }
        }
    }
}
