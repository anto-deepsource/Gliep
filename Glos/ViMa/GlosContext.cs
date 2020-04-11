using System.Collections.Generic;

namespace GeminiLab.Glos.ViMa {
    public class GlosContext {
        protected class GlosValueReferenceWrapper {
            private GlosValue _value;

            public GlosValueReferenceWrapper() => _value.SetNil();
            public GlosValueReferenceWrapper(in GlosValue value) => _value = value;

            public ref GlosValue GetReference() => ref _value;
        }

        private readonly Dictionary<string, GlosValueReferenceWrapper> _variables = new Dictionary<string, GlosValueReferenceWrapper>();
        private readonly Dictionary<string, GlosContext> _location = new Dictionary<string, GlosContext>();

        public GlosContext(GlosContext? parent) => Root = (Parent = parent)?.Root ?? this;

        public virtual GlosContext? Parent { get; }
        public virtual GlosContext Root { get; }
        
        protected GlosValueReferenceWrapper GetWrapper(string name) => GetWrapper(name, out _);

        protected virtual GlosValueReferenceWrapper GetWrapper(string name, out GlosContext location) {
            if (_variables.TryGetValue(name, out var wrapper)) {
                location = this;
                return wrapper;
            }

            if (_location.TryGetValue(name, out location)) return location.GetWrapper(name);

            if (Parent == null) {
                location = this;
                return _variables[name] = new GlosValueReferenceWrapper();
            }

            var rv = Parent.GetWrapper(name, out location);
            _location[name] = location;
            return rv;
        }

        public ref GlosValue GetVariableReference(string name) => ref GetWrapper(name).GetReference();

        public void CreateVariable(string name) => _variables[name] = new GlosValueReferenceWrapper();
        public void CreateVariable(string name, in GlosValue value) => _variables[name] = new GlosValueReferenceWrapper(value);
    }
}
