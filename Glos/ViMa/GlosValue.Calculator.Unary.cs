using System;

namespace GeminiLab.Glos.ViMa {
    public partial struct GlosValue {
        public partial class Calculator {
            public void Not(ref GlosValue dest, in GlosValue v) {
                if (v.Type == GlosValueType.Integer) dest.SetInteger(unchecked((long)~(ulong)v.AssumeInteger()));
                else if (v.Type == GlosValueType.Boolean) dest.SetBoolean(!v.AssumeBoolean());
                else throw new GlosInvalidUnaryOperandTypeException(GlosOp.Not, v);
            }

            public void Neg(ref GlosValue dest, in GlosValue v) {
                if (v.Type == GlosValueType.Integer) dest.SetInteger(-v.AssumeInteger());
                else if (v.Type == GlosValueType.Float) dest.SetFloat(-v.AssertFloat());
                else throw new GlosInvalidUnaryOperandTypeException(GlosOp.Neg, v);
            }

            public void Typeof(ref GlosValue dest, in GlosValue v) {
                dest.SetString(v.Type.GetName());
            }

            protected delegate void GlosUnaryOperationHandler(ref GlosValue dest, in GlosValue v);

            public void ExecuteUnaryOperation(ref GlosValue dest, in GlosValue v, GlosOp op) {
                (op switch {
                    GlosOp.Not => (GlosUnaryOperationHandler)Not,
                    GlosOp.Neg => Neg,
                    GlosOp.Typeof => Typeof,
                    _ => throw new ArgumentOutOfRangeException(nameof(GlosOp))
                })(ref dest, v);
            }
        }
    }
}
