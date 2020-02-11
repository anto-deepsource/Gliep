using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GeminiLab.Glos.ViMa {
    public class GlosTable {
        private readonly GlosViMa _vm;
        private readonly Dictionary<GlosValue, GlosValue> _values;

        public GlosTable(GlosViMa vm) {
            _vm = vm;
            _values = new Dictionary<GlosValue, GlosValue>(new GlosValue.EqualityComparer(vm));
        }

        public int Count => _values.Count;

        public bool ContainsKey(GlosValue key) => _values.ContainsKey(key);

        public ICollection<GlosValue> Keys => _values.Keys;

        public bool TryReadEntryLocally(GlosValue key, out GlosValue result) {
            return _values.TryGetValue(key, out result);
        }

        public bool TryReadEntry(GlosValue key, out GlosValue result) {
            if (TryGetMetamethod(GlosMetamethodNames.Ren, out var metaRen)) {
                var res = metaRen.Invoke(_vm, new[] { GlosValue.NewTable(this), key });

                if (res.Length == 0 || res[0].Type == GlosValueType.Nil) {
                    result = default;
                    return false;
                } else {
                    result = res[0];
                    return true;
                }
            } else {
                return TryReadEntryLocally(key, out result);
            }
        }

        public void UpdateEntryLocally(GlosValue key, GlosValue value) {
            _values[key] = value;
        }

        // it's named 'Update' but it can also create new index
        public void UpdateEntry(GlosValue key, GlosValue value) {
            if (TryGetMetamethod(GlosMetamethodNames.Uen, out var metaUen)) {
                metaUen.Invoke(_vm, new[] {GlosValue.NewTable(this), key, value});
            } else {
                _values[key] = value;
            }
        }

        public bool TryGetMetamethod(string name, out GlosValue fun) {
            fun = default;
            ref var rf = ref fun;
            
            if (Metatable != null && Metatable.TryReadEntryLocally(name, out fun))
                return fun.Type == GlosValueType.Function || fun.Type == GlosValueType.ExternalFunction;

            rf.SetNil();
            return false;
        }

        public GlosTable Metatable { get; set; }
    }
}
