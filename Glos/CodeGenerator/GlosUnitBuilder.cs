using System;
using System.Collections.Generic;

using GeminiLab.Glos.ViMa;

namespace GeminiLab.Glos.CodeGenerator {
    public class GlosUnitBuilder {
        #region string table
        private readonly Dictionary<string, int> _stringTable = new Dictionary<string, int>();
        private readonly List<string> _stringList = new List<string>();

        public bool TryGetStringId(string str, out int id) => _stringTable.TryGetValue(str, out id);

        public int AddOrGetString(string str) {
            if (_stringTable.TryGetValue(str, out var id)) return id;
            _stringList.Add(str);
            return _stringTable[str] = _stringTable.Count;
        }
        #endregion

        #region function table
        private int _func = 0;
        private readonly List<GlosFunctionBuilder> _builders = new List<GlosFunctionBuilder>();
        private readonly List<GlosFunctionPrototype> _functions = new List<GlosFunctionPrototype>();
        
        public GlosFunctionBuilder AddFunction() {
            var fun = new GlosFunctionBuilder(this, _func++);
            _builders.Add(fun);
            _functions.Add(null!);
            return fun;
        }

        public int AddFunctionRaw(ReadOnlySpan<byte> op, int localVariableSize) => AddFunctionRaw(op, localVariableSize, Array.Empty<string>());

        public int AddFunctionRaw(ReadOnlySpan<byte> op, int localVariableSize, IReadOnlyCollection<string> variableInContext) {
            _functions.Add(new GlosFunctionPrototype(op, localVariableSize, variableInContext));
            return _func++;
        }

        public int Entry { get; set; }
        #endregion
        
        public GlosUnit GetResult() {
            foreach (var builder in _builders) {
                _functions[builder.Id] = new GlosFunctionPrototype(builder.GetOpArray(), builder.LocalVariableCount, builder.VariableInContext ?? Array.Empty<string>());
            }

            return new GlosUnit(_functions, Entry, _stringList);
        }
    }
}
