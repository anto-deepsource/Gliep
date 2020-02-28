using System;

namespace GeminiLab.Glos.ViMa {
    public partial class GlosViMa {
        private void unaryOperator(GlosOp op, ref GlosValue o) {
            if (op == GlosOp.Not && o.Type == GlosValueType.Integer) {
                o.SetInteger(unchecked((long)~(ulong)o.ValueNumber.Integer));
            } else if(op == GlosOp.Not && o.Type == GlosValueType.Boolean) {
                o.SetBoolean(!o.AssertBoolean());
            } else if (op == GlosOp.Neg && o.Type == GlosValueType.Integer) {
                o.SetInteger(unchecked(-o.ValueNumber.Integer));
            } else if (op == GlosOp.Neg && o.Type == GlosValueType.Float) {
                o.SetFloat(-o.ValueNumber.Float);
            } else {
                throw new GlosInvalidUnaryOperandTypeException(op, o);
            }
        }
    }
}
