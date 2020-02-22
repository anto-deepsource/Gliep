using System;

namespace GeminiLab.Glos.ViMa {
    public partial class GlosViMa {
        // TODO: add metamethod support
        private void binaryArithmeticOperator(GlosOp op, ref GlosValue dest, in GlosValue x, in GlosValue y) {
            if (GlosValue.BothInteger(x, y, out var xint, out var yint)) unchecked {
                dest.SetInteger(op switch {
                    GlosOp.Add => xint + yint,
                    GlosOp.Sub => xint - yint,
                    GlosOp.Mul => xint * yint,
                    GlosOp.Div => xint / yint,
                    GlosOp.Mod => xint % yint,
                    _ => throw new InvalidOperandTypeException(),
                });
            } else if (GlosValue.BothNumeric(x, y, out var xfloat, out var yfloat)) {
                dest.SetFloat(op switch {
                    GlosOp.Add => xfloat + yfloat,
                    GlosOp.Sub => xfloat - yfloat,
                    GlosOp.Mul => xfloat * yfloat,
                    GlosOp.Div => xfloat / yfloat,
                    _ => throw new InvalidOperandTypeException(),
                });
            } else if (op == GlosOp.Add && GlosValue.BothString(x, y, out var xstr, out var ystr)) {
                dest.SetString(xstr + ystr);
            } else {
                throw new InvalidOperandTypeException();
            }
        }

        private void binaryBitwiseOperator(GlosOp op, ref GlosValue dest, in GlosValue x, in GlosValue y) {
            if (GlosValue.BothInteger(x, y, out var xint, out var yint)) {
                dest.SetInteger(op switch {
                    GlosOp.Lsh => (long)((ulong)xint << (int)yint),
                    GlosOp.Rsh => (long)((ulong)xint >> (int)yint),
                    GlosOp.And => (long)((ulong)xint & (ulong)yint),
                    GlosOp.Orr => (long)((ulong)xint | (ulong)yint),
                    GlosOp.Xor => (long)((ulong)xint ^ (ulong)yint),
                    _ => throw new InvalidOperandTypeException(),
                });
            } else if (GlosValue.BothBoolean(x, y, out var xbool, out var ybool)) {
                dest.SetBoolean(op switch {
                    GlosOp.And => xbool && ybool,
                    GlosOp.Orr => xbool || ybool,
                    GlosOp.Xor => xbool ^ ybool,
                    _ => throw new InvalidOperandTypeException(),
                });
            } else {
                throw new InvalidOperandTypeException();
            }
        }

        private void binaryComparisonOperator(GlosOp op, ref GlosValue dest, in GlosValue x, in GlosValue y) {
            dest.SetBoolean(op switch {
                GlosOp.Gtr => Comparer.GreaterThan(x, y),
                GlosOp.Lss => Comparer.LessThan(x, y),
                GlosOp.Geq => Comparer.GreaterThanOrEqualTo(x, y),
                GlosOp.Leq => Comparer.LessThanOrEqualTo(x, y),
                GlosOp.Equ => Comparer.EqualTo(x, y),
                GlosOp.Neq => Comparer.UnequalTo(x, y),
                _ => throw new InvalidOperandTypeException(),
            });
        }

        private void unaryOperator(GlosOp op, ref GlosValue o) {
            if (op == GlosOp.Not && o.Type == GlosValueType.Integer) {
                o.SetInteger(unchecked((long)~(ulong)o.ValueNumber.Integer));
            } else if (op == GlosOp.Neg && o.Type == GlosValueType.Integer) {
                o.SetInteger(unchecked(-o.ValueNumber.Integer));
            } else if (op == GlosOp.Neg && o.Type == GlosValueType.Float) {
                o.SetFloat(-o.ValueNumber.Float);
            } else {
                throw new InvalidOperandTypeException();
            }
        }
    }
}