using System;

namespace GeminiLab.Glos.ViMa {
    public class GlosFunctionPrototype {
        private byte[] _op = null!;
        internal GlosFunctionPrototype(GlosUnit unit, int localVariableSize, ReadOnlySpan<byte> op) {
            Unit = unit;
            Op = op;
            LocalVariableSize = localVariableSize;
        }

        public GlosUnit Unit { get; set; }

        public ReadOnlySpan<byte> Op {
            get => new ReadOnlySpan<byte>(_op);
            set => _op = value.ToArray();
        }

        public int LocalVariableSize { get; set; }
    }
}
