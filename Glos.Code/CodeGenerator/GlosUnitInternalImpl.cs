using System;
using System.Collections.Generic;

namespace GeminiLab.Glos.CodeGenerator {
    internal class GlosUnitInternalImpl : IGlosUnit {
        public IReadOnlyList<GlosFunctionPrototype>    FunctionTable     => RealFunctionTable;
        public int                                     Entry             { get; set; }
        public IReadOnlyList<string>                   StringTable       => RealStringTable;
        public IReadOnlyList<IGlosFunctionDebugInfo?>? FunctionDebugInfo => null;

        public readonly List<GlosFunctionPrototype> RealFunctionTable = new List<GlosFunctionPrototype>();
        public readonly List<string>                RealStringTable   = new List<string>();

        public void Dispose() { }
    }
}
