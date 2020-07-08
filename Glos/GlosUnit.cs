using System;
using System.Collections.Generic;

namespace GeminiLab.Glos {
    public interface IGlosUnit : IDisposable {
        public IReadOnlyList<GlosFunctionPrototype> FunctionTable { get; }
        public int Entry { get; }
        public IReadOnlyList<string> StringTable { get; }
    }
    
    public class GlosUnit : IGlosUnit {
        public IReadOnlyList<GlosFunctionPrototype> FunctionTable { get; }
        public int Entry { get; }
        public IReadOnlyList<string> StringTable { get; }

        internal GlosUnit(IEnumerable<GlosFunctionPrototype> functionTable, int entry, IEnumerable<string> stringTable) {
            FunctionTable = new List<GlosFunctionPrototype>(functionTable);
            Entry = entry;
            StringTable = new List<string>(stringTable);

            foreach (var prototype in FunctionTable) {
                prototype.Unit = this;
            }
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) { }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
 