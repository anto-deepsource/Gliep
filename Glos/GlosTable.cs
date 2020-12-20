using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Thank you M$!
// https://source.dot.net/#System.Private.CoreLib/Dictionary.cs
// https://source.dot.net/#System.Private.CoreLib/HashHelpers.cs
namespace GeminiLab.Glos {
    public class GlosTable {
        private int[]   _buckets;
        private Entry[] _entries;
        private int     _size;
        private int     _count;

        public GlosTable() {
            resize(3);
        }
        
        public int Count => _count;

        private KeyCollection? _keys;

        public IEnumerable<GlosValue> Keys => _keys ??= new KeyCollection(this);
        
        // e.g.
        // 
        // var eid = -1;
        // while ((eid = NextEntry(hash, eid)) >= 0) { /* your code */ }
        public int NextEntry(ulong hash, int entryId) {
            var next = -1;
            
            if (entryId == -1) {
                next = getBucket(hash) - 1;
            } else {
                next = _entries[entryId].Next;
            }

            while (next >= 0) {
                if (_entries[next].Hash == hash) break;

                next = _entries[next].Next;
            }

            return next;
        }

        public ref Entry GetEntryAt(int entryId) {
            return ref _entries[entryId];
        }

        public void NewEntry(ulong hash, in GlosValue key, in GlosValue value) {
            if (_count >= _size) {
                expandSize();
            }

            ref var bucket = ref getBucket(hash);
            ref var entry = ref _entries[_count];
                
            entry.Hash = hash;
            entry.Next = bucket - 1;
            entry.Key = key;
            entry.Value = value;

            _count = bucket = _count + 1;
        }
        
        public bool TryReadEntry(in GlosValue key, out GlosValue value) {
            var hash = key.Hash();
            
            var eid = -1;
            while ((eid = NextEntry(hash, eid)) >= 0) {
                ref var entry = ref GetEntryAt(eid);

                if (GlosValueStaticCalculator.Equals(key, entry.Key)) {
                    value = entry.Value;
                    return true;
                }
            }

            value = default;
            value.SetNil();
            return false;
        }

        public void UpdateEntry(in GlosValue key, in GlosValue value) {
            var hash = key.Hash();
            
            var eid = -1;
            while ((eid = NextEntry(hash, eid)) >= 0) {
                ref var entry = ref GetEntryAt(eid);

                if (GlosValueStaticCalculator.Equals(key, entry.Key)) {
                    entry.Value = value;
                    return;
                }
            }

            NewEntry(hash, in key, in value);
        }

        private void expandSize() {
            resize(Hashing.GetPrime(_size * 2));
        }

        private void resize(int size) {
            _buckets = new int[size];
            Array.Resize(ref _entries, size);
            _size = size;

            for (int i = 0; i < _count; ++i) {
                ref int bucket = ref getBucket(_entries[i].Hash);
                _entries[i].Next = bucket - 1;
                bucket = i + 1;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref int getBucket(ulong hash) {
            return ref _buckets[hash % (uint) _size];
        }
        
        [StructLayout(LayoutKind.Auto)]
        public struct Entry {
            public ulong Hash;
            public int   Next;

            public GlosValue Key;
            public GlosValue Value;
        }
        
        private static class Hashing {
            private static readonly int[] Primes = {
                3, 5, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761,
                919, 1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
                17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
                187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
                1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369, 8639249, 10367101,
                12440537, 14928671, 17914409, 21497293, 25796759, 30956117, 37147349, 44576837, 53492207, 64190669,
                77028803, 92434613, 110921543, 133105859, 159727031, 191672443, 230006941, 276008387, 331210079,
                397452101, 476942527, 572331049, 686797261, 824156741, 988988137, 1186785773, 1424142949,
                1708971541, 2050765853, 2146435069,
            };
            
            public static int GetPrime(int n) {
                foreach (var prime in Primes) {
                    if (prime >= n) {
                        return prime;
                    }
                }

                return Primes[^0];
            }
        }
        
        public bool TryGetMetamethod(string name, out GlosValue fun) {
            fun = default;

            if (Metatable != null && Metatable.TryReadEntry(name, out fun) && fun.IsInvokable()) return true;

            fun.SetNil();
            return false;
        }

        public GlosTable? Metatable { get; set; }

        private class KeyEnumerator : IEnumerator<GlosValue> {
            // TODO: add version check
            public KeyEnumerator(GlosTable table) {
                _table = table;
            }
            
            private GlosTable _table;
            private int       _ptr = -1;


            public bool MoveNext() {
                if (_ptr + 1 < _table._count) {
                    _ptr++;
                    return true;
                }

                return false;
            }

            public void Reset() {
                _ptr = -1;
            }

            public GlosValue Current => _table._entries[_ptr].Key;

            object IEnumerator.Current => Current;

            public void Dispose() { }
        }
        
        private class KeyCollection : IEnumerable<GlosValue> {
            public KeyCollection(GlosTable table) {
                _table = table;
            }

            private GlosTable _table;
            
            public IEnumerator<GlosValue> GetEnumerator()  => new KeyEnumerator(_table);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
