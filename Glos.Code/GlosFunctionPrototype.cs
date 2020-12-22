using System;
using System.Collections.Generic;

namespace GeminiLab.Glos {
    public class GlosFunctionPrototype {
        public GlosFunctionPrototype(string name, ReadOnlySpan<byte> code, int localVariableSize, IReadOnlyCollection<string> variableInContext) {
            Name = name;
            _code = code.ToArray();
            LocalVariableSize = localVariableSize;
            VariableInContext = new HashSet<string>(variableInContext);
        }

        public string Name { get; }

        private readonly byte[] _code;
        public ReadOnlySpan<byte> CodeSpan => new ReadOnlySpan<byte>(_code);
        public ReadOnlyMemory<byte> CodeMemory => new ReadOnlyMemory<byte>(_code);

        public int LocalVariableSize { get; }

        public IReadOnlyCollection<string> VariableInContext { get; }
    }
}
