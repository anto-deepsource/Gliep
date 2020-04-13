using System.Collections.Generic;

namespace GeminiLab.Glos {
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

            public int GetHashCode(GlosValue obj) => unchecked((int)obj.getHash(_vm));
        }
    }
}
