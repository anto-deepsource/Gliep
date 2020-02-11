using System.Collections.Generic;

namespace GeminiLab.Glos.ViMa {
    public class GlosUnit {
        public IReadOnlyList<GlosFunctionPrototype?> FunctionTable { get; internal set; }
        public IReadOnlyList<string> StringTable { get; internal set; }
        public int Entry { get; internal set; }

        internal GlosUnit() { }
    }
}
