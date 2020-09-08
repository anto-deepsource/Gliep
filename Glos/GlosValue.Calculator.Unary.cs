using System;

namespace GeminiLab.Glos {
    public partial struct GlosValue {
        public partial class Calculator {
            public void Not(ref GlosValue dest, in GlosValue v) {
                if (v.Type == GlosValueType.Integer) dest.SetInteger(unchecked((long) ~(ulong) v.AssumeInteger()));
                else if (v.Type == GlosValueType.Boolean) dest.SetBoolean(!v.AssumeBoolean());
                else if (TryInvokeMetamethod(ref dest, v, _viMa, GlosMetamethodNames.Not)) ;
                else throw new GlosInvalidUnaryOperandTypeException(GlosOp.Not, v);
            }

            public void Neg(ref GlosValue dest, in GlosValue v) {
                if (v.Type == GlosValueType.Integer) dest.SetInteger(-v.AssumeInteger());
                else if (v.Type == GlosValueType.Float) dest.SetFloat(-v.AssertFloat());
                else if (TryInvokeMetamethod(ref dest, v, _viMa, GlosMetamethodNames.Neg)) ;
                else throw new GlosInvalidUnaryOperandTypeException(GlosOp.Neg, v);
            }

            public void Typeof(ref GlosValue dest, in GlosValue v) {
                dest.SetString(v.Type.GetName());
            }

            public void IsNil(ref GlosValue dest, in GlosValue v) {
                dest.SetBoolean(v.IsNil());
            }

            public void ExecuteUnaryOperation(ref GlosValue dest, in GlosValue v, GlosOp op) {
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
}
