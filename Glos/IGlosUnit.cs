using System;
using System.Collections.Generic;

namespace GeminiLab.Glos {
    public interface IGlosUnit : IDisposable {
        public IReadOnlyList<GlosFunctionPrototype> FunctionTable { get; }
        public int Entry { get; }
        public IReadOnlyList<string> StringTable { get; }
    }
}
