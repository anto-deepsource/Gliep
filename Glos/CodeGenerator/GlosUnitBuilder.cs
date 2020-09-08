using System;
using System.Collections.Generic;

namespace GeminiLab.Glos.CodeGenerator {
    public class GlosUnitBuilder {
        private readonly GlosUnitInternalImpl _impl = new GlosUnitInternalImpl();

#region string table

        private readonly Dictionary<string, int> _stringTable = new Dictionary<string, int>();

        public bool TryGetStringId(string str, out int id) => _stringTable.TryGetValue(str, out id);

        public int AddOrGetString(string str) {
            if (_stringTable.TryGetValue(str, out var id)) return id;
            _impl.RealStringTable.Add(str);
            return _stringTable[str] = _stringTable.Count;
        }

        public int StringCount => _stringTable.Count;

#endregion

#region function table

        private int _func = 0;
        private readonly List<GlosFunctionBuilder> _builders = new List<GlosFunctionBuilder>();

        public GlosFunctionBuilder AddFunction() {
            var fun = new GlosFunctionBuilder(this, _func++);
            _builders.Add(fun);
            _impl.RealFunctionTable.Add(null!);
            return fun;
        }

        public int AddFunctionRaw(ReadOnlySpan<byte> op, int localVariableSize, IReadOnlyCollection<string>? variableInContext = null, string? name = null) {
            _impl.RealFunctionTable.Add(new GlosFunctionPrototype(name ?? $"function#{_func}", op, localVariableSize, variableInContext ?? Array.Empty<string>(), _impl));
            return _func++;
        }

        public int Entry {
            get => _impl.Entry;
            set => _impl.Entry = value;
        }

        public int FunctionCount => _func;

#endregion

        public IGlosUnit GetResult() {
            foreach (var builder in _builders) {
                _impl.RealFunctionTable[builder.Id] = new GlosFunctionPrototype(builder.Name, builder.GetOpArray(), builder.LocalVariableCount, builder.VariableInContext ?? Array.Empty<string>(), _impl);
            }

            return _impl;
        }
    }
}
