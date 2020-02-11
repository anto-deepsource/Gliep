using System;
using System.Collections.Generic;
using GeminiLab.Glos.ViMa;

namespace GeminiLab.Glos.CodeGenerator {
    public class GlosUnitBuilder {
        #region unit
        private readonly GlosUnit _unit;
        #endregion

        #region string table
        private readonly Dictionary<string, int> _stringTable;
        private readonly List<string> _stringList;

        public bool TryGetStringId(string str, out int id) => _stringTable.TryGetValue(str, out id);

        public int AddOrGetString(string str) {
            if (_stringTable.TryGetValue(str, out var id)) return id;
            _stringList.Add(str);
            return _stringTable[str] = _stringTable.Count;
        }
        #endregion

        #region function table
        private int _func = 0;
        private readonly List<FunctionBuilder> _builders;
        private readonly List<GlosFunctionPrototype> _functions;
        
        public FunctionBuilder AddFunction() {
            var fun = new FunctionBuilder(this, _func++);
            _builders.Add(fun);
            _functions.Add(new GlosFunctionPrototype(_unit, 0, Array.Empty<byte>()));
            return fun;
        }

        public int AddFunctionRaw(int localVariableSize, ReadOnlySpan<byte> op) {
            _functions.Add(new GlosFunctionPrototype(_unit, localVariableSize, op));
            return _func++;
        }

        public int Entry {
            get => _unit.Entry;
            set => _unit.Entry = value;
        }
        #endregion

        public GlosUnitBuilder() {
            _stringTable = new Dictionary<string, int>();
            _stringList = new List<string>();
            _builders = new List<FunctionBuilder>();
            _functions = new List<GlosFunctionPrototype>();
            _unit = new GlosUnit { Entry = 0, FunctionTable = _functions, StringTable = _stringList };
        }

        public GlosUnit GetResult() {
            foreach (var builder in _builders) {
                _functions[builder.Id].Op = builder.GetOpArray();
                _functions[builder.Id].LocalVariableSize = builder.LocalVariableCount;
            }
            return _unit;
        }
    }
}
