using System;
using System.Collections.Generic;

namespace GeminiLab.Glos.ViMa {
    public class GlosFunctionPrototype {
        // making this ctor internal here is not quite a good choice. TODO: reconsider it 
        internal GlosFunctionPrototype(ReadOnlySpan<byte> op, int localVariableSize, IReadOnlyCollection<string> variableInContext) {
            _op = op.ToArray();
            LocalVariableSize = localVariableSize;
            VariableInContext = new HashSet<string>(variableInContext);
        }

        public GlosUnit Unit { get; internal set; }

        private readonly byte[] _op;
        public ReadOnlySpan<byte> Op => new ReadOnlySpan<byte>(_op);

        public int LocalVariableSize { get; }

        public IReadOnlyCollection<string> VariableInContext { get; }
    }
}
