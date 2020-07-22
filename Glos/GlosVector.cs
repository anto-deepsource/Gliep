using System;

namespace GeminiLab.Glos {
    public class GlosVector {
        private readonly GlosStack<GlosValue> _list;

        private const int DefaultInitSize = 4;

        public GlosVector() : this(DefaultInitSize) { }

        public GlosVector(int len) {
            _list = new GlosStack<GlosValue>(len);
        }

        public ref GlosValue this[int index] => ref _list[index];

        public ref GlosValue Push(in GlosValue value) {
            ref var rv = ref _list.PushStack();
            rv = value;
            return ref rv;
        }

        public void Pop() {
            _list.PopStack().SetNil();
        }

        public ref GlosValue PopRef() {
            return ref _list.PopStack();
        }

        internal GlosStack<GlosValue> Container() => _list;

        public Memory<GlosValue> AsMemory() => _list.AsMemory();
    }
}
