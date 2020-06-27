/*using System.Collections.Generic;
using System.IO.Abstractions;
using GeminiLab.Glos;

namespace GeminiLab.Gliep {
    public delegate IFile UnitFinder(string Key, )
    
    public class UnitManager {
        internal class LoadedUnit {
            public string Key;
            public IFile Source;
            public GlosUnit Unit;
            public GlosValue Result;
            
            public LoadedUnit(string key, GlosUnit unit, GlosValue result) {
                Key = key;
                Unit = unit;
                Result = result;
            }
        }
        
        private readonly Dictionary<string, LoadedUnit> _units = new Dictionary<string, LoadedUnit>();

        private IFileSystem _fs;

        public UnitManager(IFileSystem fs) {
            _fs = fs;
        }
        
        
        public bool Loaded(string key) => _units.ContainsKey(key);

        public GlosUnit Load(string key) {
            return null;
        }
        
        public void Unload(string key) {
            
        }
        
        public void Clear() {
            _units.Clear();
        }
    }
}
*/
