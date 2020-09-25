using System;
using System.Collections.Generic;

namespace GeminiLab.Glos {
    public interface IGlosFunctionDebugInfo {
        public IReadOnlyList<string>            ParameterName { get; }
        public IReadOnlyDictionary<int, string> OpPosition    { get; }
    }

    public interface IGlosUnit : IDisposable {
        public IReadOnlyList<GlosFunctionPrototype>    FunctionTable     { get; }
        public int                                     Entry             { get; }
        public IReadOnlyList<string>                   StringTable       { get; }
        public IReadOnlyList<IGlosFunctionDebugInfo?>? FunctionDebugInfo { get; }
    }
}
