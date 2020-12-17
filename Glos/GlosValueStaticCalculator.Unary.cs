using System;

namespace GeminiLab.Glos {
    public static partial class GlosValueStaticCalculator {
        public static void Not(ref GlosValue dest, in GlosValue v) {
            if (v.Type == GlosValueType.Integer) dest.SetInteger(unchecked((long) ~(ulong) v.AssumeInteger()));
            else if (v.Type == GlosValueType.Boolean) dest.SetBoolean(!v.AssumeBoolean());
            else throw new GlosInvalidUnaryOperandTypeException(GlosOp.Not, v);
        }

        public static void Neg(ref GlosValue dest, in GlosValue v) {
            if (v.Type == GlosValueType.Integer) dest.SetInteger(-v.AssumeInteger());
            else if (v.Type == GlosValueType.Float) dest.SetFloat(-v.AssertFloat());
            else throw new GlosInvalidUnaryOperandTypeException(GlosOp.Neg, v);
        }

        public static void Typeof(ref GlosValue dest, in GlosValue v) {
            dest.SetString(v.Type.GetName());
        }

        public static void IsNil(ref GlosValue dest, in GlosValue v) {
            dest.SetBoolean(v.IsNil());
        }

        public static void ExecuteUnaryOperation(ref GlosValue dest, in GlosValue v, GlosOp op) {
            switch (op) {
            case GlosOp.Not:
                Not(ref dest, in v);
                break;
            case GlosOp.Neg:
                Neg(ref dest, in v);
                break;
            case GlosOp.Typeof:
                Typeof(ref dest, in v);
                break;
            case GlosOp.IsNil:
                IsNil(ref dest, in v);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(op));
            }
        }
    }
}
