using System;

namespace GeminiLab.Glos {
    public class GlosVector {
        private readonly GlosStack<GlosValue> _list;

        private const int DefaultInitSize = 4;

        public GlosVector() : this(DefaultInitSize) { }

        public GlosVector(int cap) {
            _list = new GlosStack<GlosValue>(cap);
        }

        public ref GlosValue this[int index] => ref _list[index];

        public ref GlosValue Push(in GlosValue value) {
            ref var rv = ref _list.PushStack();
            rv = value;
            return ref rv;
        }

        public ref GlosValue PushNil() {
            return ref _list.PushStack().SetNil();
        }

        public void Pop() {
            _list.PopStack().SetNil();
        }

        public ref GlosValue PopRef() {
            return ref _list.PopStack();
        }

        internal GlosStack<GlosValue> Container() => _list;

        public Memory<GlosValue> AsMemory() => _list.AsMemory();

        public int Count => _list.Count;
    }
}
