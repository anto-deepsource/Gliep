using System;

namespace GeminiLab.Glos {
    // Contracts here:
    //   - op equ and neq never throws itself (but metamethods may)
    //   - metatables of left operands are checked first
    public partial struct GlosValue {
        public partial class Calculator {
            private readonly GlosViMa _viMa;

            public Calculator(GlosViMa viMa) {
                _viMa = viMa;
            }

// disable empty statement warning
#pragma warning disable CS0642

            public void Add(ref GlosValue dest, in GlosValue x, in GlosValue y) {
                unchecked {
                    if (BothInteger(x, y, out var xint, out var yint)) dest.SetInteger(xint + yint);
                    else if (BothNumeric(x, y, out var xfloat, out var yfloat)) dest.SetFloat(xfloat + yfloat);
                    else if (BothString(x, y, out var xstr, out var ystr)) dest.SetString(xstr + ystr);
                    else if (TryInvokeMetamethod(ref dest, x, y, _viMa, GlosMetamethodNames.Add, false)) ;
                    else throw new GlosInvalidBinaryOperandTypeException(GlosOp.Add, x, y);
                }
            }

            public void Sub(ref GlosValue dest, in GlosValue x, in GlosValue y) {
                unchecked {
                    if (BothInteger(x, y, out var xint, out var yint)) dest.SetInteger(xint - yint);
                    else if (BothNumeric(x, y, out var xfloat, out var yfloat)) dest.SetFloat(xfloat - yfloat);
                    else if (TryInvokeMetamethod(ref dest, x, y, _viMa, GlosMetamethodNames.Sub, false)) ;
                    else throw new GlosInvalidBinaryOperandTypeException(GlosOp.Sub, x, y);
                }
            }

            public void Mul(ref GlosValue dest, in GlosValue x, in GlosValue y) {
                unchecked {
                    if (BothInteger(x, y, out var xint, out var yint)) dest.SetInteger(xint * yint);
                    else if (BothNumeric(x, y, out var xfloat, out var yfloat)) dest.SetFloat(xfloat * yfloat);
                    else if (TryInvokeMetamethod(ref dest, x, y, _viMa, GlosMetamethodNames.Mul, false)) ;
                    else throw new GlosInvalidBinaryOperandTypeException(GlosOp.Mul, x, y);
                }
            }

            public void Div(ref GlosValue dest, in GlosValue x, in GlosValue y) {
                unchecked {
                    if (BothInteger(x, y, out var xint, out var yint)) dest.SetInteger(xint / yint);
                    else if (BothNumeric(x, y, out var xfloat, out var yfloat)) dest.SetFloat(xfloat / yfloat);
                    else if (TryInvokeMetamethod(ref dest, x, y, _viMa, GlosMetamethodNames.Div, false)) ;
                    else throw new GlosInvalidBinaryOperandTypeException(GlosOp.Div, x, y);
                }
            }

            public void Mod(ref GlosValue dest, in GlosValue x, in GlosValue y) {
                unchecked {
                    if (BothInteger(x, y, out var xint, out var yint)) dest.SetInteger(xint % yint);
                    else if (TryInvokeMetamethod(ref dest, x, y, _viMa, GlosMetamethodNames.Mod, false)) ;
                    else throw new GlosInvalidBinaryOperandTypeException(GlosOp.Mod, x, y);
                }
            }

            public void Lsh(ref GlosValue dest, in GlosValue x, in GlosValue y) {
                unchecked {
                    if (BothInteger(x, y, out var xint, out var yint)) dest.SetInteger((long)((ulong)xint << (int)(0x3f & (uint)yint)));
                    else if (TryInvokeMetamethod(ref dest, x, y, _viMa, GlosMetamethodNames.Lsh, false)) ;
                    else throw new GlosInvalidBinaryOperandTypeException(GlosOp.Lsh, x, y);
                }
            }

            public void Rsh(ref GlosValue dest, in GlosValue x, in GlosValue y) {
                unchecked {
                    if (BothInteger(x, y, out var xint, out var yint)) dest.SetInteger((long)((ulong)xint >> (int)(0x3f & (uint)yint)));
                    else if (TryInvokeMetamethod(ref dest, x, y, _viMa, GlosMetamethodNames.Rsh, false)) ;
                    else throw new GlosInvalidBinaryOperandTypeException(GlosOp.Rsh, x, y);
                }
            }

            public void And(ref GlosValue dest, in GlosValue x, in GlosValue y) {
                unchecked {
                    if (BothInteger(x, y, out var xint, out var yint)) dest.SetInteger((long)((ulong)xint & (ulong)yint));
                    else if (BothBoolean(x, y, out var xbool, out var ybool)) dest.SetBoolean(xbool && ybool);
                    else if (TryInvokeMetamethod(ref dest, x, y, _viMa, GlosMetamethodNames.And, false)) ;
                    else throw new GlosInvalidBinaryOperandTypeException(GlosOp.And, x, y);
                }
            }

            public void Orr(ref GlosValue dest, in GlosValue x, in GlosValue y) {
                unchecked {
                    if (BothInteger(x, y, out var xint, out var yint)) dest.SetInteger((long)((ulong)xint | (ulong)yint));
                    else if (BothBoolean(x, y, out var xbool, out var ybool)) dest.SetBoolean(xbool || ybool);
                    else if (TryInvokeMetamethod(ref dest, x, y, _viMa, GlosMetamethodNames.Orr, false)) ;
                    else throw new GlosInvalidBinaryOperandTypeException(GlosOp.Orr, x, y);
                }
            }

            public void Xor(ref GlosValue dest, in GlosValue x, in GlosValue y) {
                unchecked {
                    if (BothInteger(x, y, out var xint, out var yint)) dest.SetInteger((long)((ulong)xint ^ (ulong)yint));
                    else if (BothBoolean(x, y, out var xbool, out var ybool)) dest.SetBoolean(xbool ^ ybool);
                    else if (TryInvokeMetamethod(ref dest, x, y, _viMa, GlosMetamethodNames.Xor, false)) ;
                    else throw new GlosInvalidBinaryOperandTypeException(GlosOp.Xor, x, y);
                }
            }

            public void Lss(ref GlosValue dest, in GlosValue x, in GlosValue y) {
                if (BothInteger(x, y, out var xint, out var yint)) dest.SetBoolean(xint < yint);
                else if (BothNumeric(x, y, out var xfloat, out var yfloat)) dest.SetBoolean(xfloat < yfloat);
                else if (BothString(x, y, out var xstr, out var ystr)) dest.SetBoolean(string.Compare(xstr, ystr, StringComparison.Ordinal) < 0);
                else if (TryInvokeMetamethod(ref dest, x, y, _viMa, GlosMetamethodNames.Lss, false)) dest.SetBoolean(dest.Truthy());
                else throw new GlosInvalidBinaryOperandTypeException(GlosOp.Lss, x, y);
            }

            public void Gtr(ref GlosValue dest, in GlosValue x, in GlosValue y) {
                if (BothInteger(x, y, out var xint, out var yint)) dest.SetBoolean(xint > yint);
                else if (BothNumeric(x, y, out var xfloat, out var yfloat)) dest.SetBoolean(xfloat > yfloat);
                else if (BothString(x, y, out var xstr, out var ystr)) dest.SetBoolean(string.Compare(xstr, ystr, StringComparison.Ordinal) > 0);
                else if (TryInvokeMetamethod(ref dest, y, x, _viMa, GlosMetamethodNames.Lss, true)) dest.SetBoolean(dest.Truthy());
                else throw new GlosInvalidBinaryOperandTypeException(GlosOp.Gtr, x, y);
            }

            public void Leq(ref GlosValue dest, in GlosValue x, in GlosValue y) {
                GlosValue temp = default;

                if (BothInteger(x, y, out var xint, out var yint)) dest.SetBoolean(xint <= yint);
                else if (BothNumeric(x, y, out var xfloat, out var yfloat)) dest.SetBoolean(xfloat <= yfloat);
                else if (BothString(x, y, out var xstr, out var ystr)) dest.SetBoolean(string.Compare(xstr, ystr, StringComparison.Ordinal) <= 0);
                else if (TryInvokeMetamethod(ref temp, x, y, _viMa, GlosMetamethodNames.Lss, false)) {
                    if (temp.Falsey()) {
                        if (!TryInvokeMetamethod(ref dest, x, y, _viMa, GlosMetamethodNames.Equ, false)) {
                            throw new GlosInvalidBinaryOperandTypeException(GlosOp.Lss, x, y);
                        }
                    } else {
                        dest.SetBoolean(true);
                    }
                } else throw new GlosInvalidBinaryOperandTypeException(GlosOp.Leq, x, y);
            }

            public void Geq(ref GlosValue dest, in GlosValue x, in GlosValue y) {
                GlosValue temp = default;

                if (BothInteger(x, y, out var xint, out var yint)) dest.SetBoolean(xint >= yint);
                else if (BothNumeric(x, y, out var xfloat, out var yfloat)) dest.SetBoolean(xfloat >= yfloat);
                else if (BothString(x, y, out var xstr, out var ystr)) dest.SetBoolean(string.Compare(xstr, ystr, StringComparison.Ordinal) >= 0);
                else if (TryInvokeMetamethod(ref temp, y, x, _viMa, GlosMetamethodNames.Lss, true)) {
                    if (temp.Falsey()) {
                        if (!TryInvokeMetamethod(ref dest, y, x, _viMa, GlosMetamethodNames.Equ, true)) {
                            throw new GlosInvalidBinaryOperandTypeException(GlosOp.Lss, x, y);
                        }
                    } else {
                        dest.SetBoolean(true);
                    }
                } else throw new GlosInvalidBinaryOperandTypeException(GlosOp.Geq, x, y);
            }

            public void Equ(ref GlosValue dest, in GlosValue x, in GlosValue y) {
                if (BothNil(x, y)) dest.SetBoolean(true);
                else if (BothInteger(x, y, out var xint, out var yint)) dest.SetBoolean(xint == yint);
                else if (BothNumeric(x, y, out var xfloat, out var yfloat)) dest.SetBoolean(xfloat == yfloat);
                else if (BothString(x, y, out var xstring, out var ystring)) dest.SetBoolean(xstring == ystring);
                else if (BothBoolean(x, y, out var xbool, out var ybool)) dest.SetBoolean(xbool == ybool);
                else if (BothFunction(x, y, out var xfun, out var yfun)) dest.SetBoolean(xfun.Prototype == yfun.Prototype && xfun.ParentContext == yfun.ParentContext);
                else if (TryInvokeMetamethod(ref dest, x, y, _viMa, GlosMetamethodNames.Equ, false)) dest.SetBoolean(dest.Truthy());

                else dest.SetBoolean(x.Type == y.Type && ReferenceEquals(x.ValueObject, y.ValueObject));
            }

            public void Neq(ref GlosValue dest, in GlosValue x, in GlosValue y) {
                if (BothNil(x, y)) dest.SetBoolean(false);
                else if (BothInteger(x, y, out var xint, out var yint)) dest.SetBoolean(xint != yint);
                else if (BothNumeric(x, y, out var xfloat, out var yfloat)) dest.SetBoolean(xfloat != yfloat);
                else if (BothString(x, y, out var xstring, out var ystring)) dest.SetBoolean(xstring != ystring);
                else if (BothBoolean(x, y, out var xbool, out var ybool)) dest.SetBoolean(xbool != ybool);
                else if (BothFunction(x, y, out var xfun, out var yfun)) dest.SetBoolean(xfun.Prototype != yfun.Prototype || xfun.ParentContext != yfun.ParentContext);
                else if (TryInvokeMetamethod(ref dest, x, y, _viMa, GlosMetamethodNames.Equ, false)) dest.SetBoolean(dest.Falsey());

                else dest.SetBoolean(x.Type != y.Type || !ReferenceEquals(x.ValueObject, y.ValueObject));
            }
#pragma warning restore CS0642

            public void ExecuteBinaryOperation(ref GlosValue dest, in GlosValue x, in GlosValue y, GlosOp op) {
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
}
