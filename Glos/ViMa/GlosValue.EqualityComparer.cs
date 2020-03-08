using System.Collections.Generic;

namespace GeminiLab.Glos.ViMa {
    public partial struct GlosValue {
        public class EqualityComparer : IEqualityComparer<GlosValue> {
            private readonly GlosViMa _vm;
            private readonly Calculator _cal;

            public EqualityComparer(GlosViMa vm) {
                _vm = vm;
                _cal = new Calculator(vm);
            }

            public bool Equals(GlosValue x, GlosValue y) {
                GlosValue cache = default;
                _cal.Equ(ref cache, x, y);
                return cache.Truthy();
            }

            public int GetHashCode(GlosValue obj) {
                if (obj._hashCodeCalculated) {
                    return obj._hashCode;
                }

                if (obj.Type == GlosValueType.Table) {
                    var t = obj.AssertTable();
                    if (t.TryGetMetamethod(GlosMetamethodNames.Hash, out var metaHash)) {
                        var res = metaHash.Invoke(_vm, new[] { obj });
                        if (res.Length < 1) {
                            obj._hashCode = 0;
                        } else {
                            obj._hashCode = (int) res[0].AssertInteger();
                        }
                    } else {
                        obj._hashCode = t.GetHashCode();
                    }
                } else {
                    obj._hashCode = 0;
                }

                obj._hashCodeCalculated = true;
                return obj._hashCode;
            }
        }
    }
}