using System;
using System.Collections.Generic;

namespace GeminiLab.Glos.ViMa {
    public class GlosFunctionPrototype {
        // making this ctor internal here is not quite a good choice. TODO: reconsider it 
        internal GlosFunctionPrototype(string name, ReadOnlySpan<byte> code, int localVariableSize, IReadOnlyCollection<string> variableInContext) {
            Name = name;
            _code = code.ToArray();
            LocalVariableSize = localVariableSize;
            VariableInContext = new HashSet<string>(variableInContext);
        }

        public GlosUnit Unit { get; internal set; } = null!;

        public string Name { get; }

        private readonly byte[] _code;
        public ReadOnlySpan<byte> Code => new ReadOnlySpan<byte>(_code);

        public int LocalVariableSize { get; }

        public IReadOnlyCollection<string> VariableInContext { get; }
    }
}
