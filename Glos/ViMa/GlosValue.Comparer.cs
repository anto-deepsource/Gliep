using System;
using System.Diagnostics.CodeAnalysis;

namespace GeminiLab.Glos.ViMa {

    // Contracts here:
    //  In this class we provide:
    //    - operator >(gtr), <(lss), >=(geq), <=(leq), which
    //      - throws when unable to compare.
    //    - operator ==(equ), !=(neq), which
    //      - never throws (unless metamethods used throw).
    //      - claim inequality when unable to compare.
    //    - operator #(hash), which
    //      - is unimplemented yet. TODO
    //      - calculate only once.
    //  We check the metatable of the right operand if the left operand has none.
    public partial struct GlosValue {
        public class Comparer {
            private readonly GlosViMa _vm;

            public Comparer(GlosViMa vm) {
                _vm = vm;
            }
            
            #region Basic methods

            public static bool TryGetMetamethodOfOperand(in GlosValue x, in GlosValue y, string name, bool lookupMetatableOfLatterFirst, out GlosValue fun) {
                fun = default;
                ref var rf = ref fun;
                rf.SetNil();

                if (!lookupMetatableOfLatterFirst) {
                    if (x.Type == GlosValueType.Table && x.AssertTable().TryGetMetamethod(name, out fun)) return true;
                    if (y.Type == GlosValueType.Table && y.AssertTable().TryGetMetamethod(name, out fun)) return true;
                    return false;
                } else {
                    if (y.Type == GlosValueType.Table && y.AssertTable().TryGetMetamethod(name, out fun)) return true;
                    if (x.Type == GlosValueType.Table && x.AssertTable().TryGetMetamethod(name, out fun)) return true;
                    return false;
                }
            }

            private bool tryInvokeMetaLessThan(in GlosValue x, in GlosValue y, bool lookupMetatableOfLatterFirst, out bool result) {
                if (TryGetMetamethodOfOperand(in x, in y, GlosMetamethodNames.Lss, lookupMetatableOfLatterFirst, out var metaLss)) {
                    var res = metaLss.Invoke(_vm, new[] { x, y });
                    result = res.Length >= 1 && res[0].Truthy();
                    return true;
                }

                result = false;
                return false;
            }

            private bool tryInvokeMetaEqualTo(in GlosValue x, in GlosValue y, bool lookupMetatableOfLatterFirst, out bool result) {
                if (TryGetMetamethodOfOperand(in x, in y, GlosMetamethodNames.Equ, lookupMetatableOfLatterFirst, out var metaEqu)) {
                    var res = metaEqu.Invoke(_vm, new[] { x, y });
                    result = res.Length >= 1 && res[0].Truthy();
                    return true;
                }

                result = false;
                return false;
            }
            #endregion

            public bool LessThan(in GlosValue x, in GlosValue y) {
                if (BothInteger(x, y, out var xint, out var yint)) return xint < yint;
                if (BothNumeric(x, y, out var xfloat, out var yfloat)) return xfloat < yfloat;
                if (BothString(x, y, out var xstr, out var ystr)) return string.Compare(xstr, ystr, StringComparison.Ordinal) < 0;

                if (tryInvokeMetaLessThan(x, y, false, out var metaResult)) return metaResult; 
                
                throw new GlosInvalidBinaryOperandTypeException(_vm, GlosOp.Lss, x, y);
            }

            public bool GreaterThan(in GlosValue x, in GlosValue y) {
                if (BothInteger(x, y, out var xint, out var yint)) return xint > yint;
                if (BothNumeric(x, y, out var xfloat, out var yfloat)) return xfloat > yfloat;
                if (BothString(x, y, out var xstr, out var ystr)) return string.Compare(xstr, ystr, StringComparison.Ordinal) > 0;

                if (tryInvokeMetaLessThan(y, x, true, out var metaResult)) return metaResult;

                throw new GlosInvalidBinaryOperandTypeException(_vm, GlosOp.Gtr, x, y);
            }

            public bool EqualTo(in GlosValue x, in GlosValue y) {
                if (BothNil(x, y)) return true;
                if (BothInteger(x, y, out var xint, out var yint)) return xint == yint;
                if (BothNumeric(x, y, out var xfloat, out var yfloat)) return xfloat == yfloat;
                if (BothString(x, y, out var xstring, out var ystring)) return xstring == ystring;
                if (BothBoolean(x, y, out var xbool, out var ybool)) return xbool == ybool;

                if (BothFunction(x, y, out var xfun, out var yfun)) return xfun.Prototype == yfun.Prototype && xfun.ParentContext == yfun.ParentContext;
                if (tryInvokeMetaEqualTo(x, y, false, out var metaResult)) return metaResult;

                return x.Type == y.Type && object.ReferenceEquals(x.ValueObject, y.ValueObject);
            }

            public bool UnequalTo(in GlosValue x, in GlosValue y) {
                if (BothNil(x, y)) return false;
                if (BothInteger(x, y, out var xint, out var yint)) return xint != yint;
                if (BothNumeric(x, y, out var xfloat, out var yfloat)) return xfloat != yfloat;
                if (BothString(x, y, out var xstring, out var ystring)) return xstring != ystring;
                if (BothBoolean(x, y, out var xbool, out var ybool)) return xbool != ybool;

                if (BothFunction(x, y, out var xfun, out var yfun)) return xfun.Prototype != yfun.Prototype || xfun.ParentContext != yfun.ParentContext;
                if (tryInvokeMetaEqualTo(x, y, false, out var metaResult)) return !metaResult;

                return x.Type != y.Type || !object.ReferenceEquals(x.ValueObject, y.ValueObject);
            }

            public bool LessThanOrEqualTo(in GlosValue x, in GlosValue y) => LessThan(x, y) || EqualTo(x, y);

            public bool GreaterThanOrEqualTo(in GlosValue x, in GlosValue y) => GreaterThan(x, y) || EqualTo(x, y);
        }
    }
}