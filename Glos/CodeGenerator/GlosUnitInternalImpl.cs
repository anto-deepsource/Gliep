using System;
using System.Collections.Generic;

namespace GeminiLab.Glos.CodeGenerator {
    internal class GlosUnitInternalImpl : IGlosUnit {
        public IReadOnlyList<GlosFunctionPrototype> FunctionTable => RealFunctionTable;
        public int Entry { get; set; }
        public IReadOnlyList<string> StringTable => RealStringTable;

        public readonly List<GlosFunctionPrototype> RealFunctionTable = new List<GlosFunctionPrototype>();
        public readonly List<string> RealStringTable = new List<string>();

        protected virtual void Dispose(bool disposing) {
            if (disposing) { }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
