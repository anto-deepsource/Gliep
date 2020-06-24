using System;

namespace GeminiLab.Glos {
    public class GlosStack<T> {
        public const int DefaultInitialCapacity = 128;

        public GlosStack() : this(DefaultInitialCapacity) { }

        public GlosStack(int cap) {
            _items = new T[cap];
            Capacity = cap;
            Count = 0;
        }

        public int Capacity {
            get => _items.Length;
            set {
                if (value < Count) throw new ArgumentOutOfRangeException(nameof(value));
                if (value == _items.Length) return;

                var newItems = new T[value];
                Array.Copy(_items, newItems, Count);
                _items = newItems;
            }
        }

        public int Count { get; private set; }

        protected T[] _items;

        public Span<T> AsSpan() => _items.AsSpan(0, Count);

        public Span<T> AsSpan(Index startIndex) => _items.AsSpan(startIndex..Count);

        public Span<T> AsSpan(int start) => _items.AsSpan(start..Count);

        public Span<T> AsSpan(Range range) {
            if (!range.End.IsFromEnd) {
                if (range.End.Value > Capacity) {
                    EnsureCapacity(range.End.Value);
                }

                if (range.End.Value > Count) {
                    Count = range.End.Value;
                }
            }

            return _items.AsSpan(range);
        }

        public Span<T> AsSpan(int start, int length) => AsSpan(new Range(start, start + length));
        
        public void EnsureCapacity(int cap) {
            if (Capacity > cap) return;

            var newCap = Capacity;
            while (newCap < cap) newCap *= 2;
            Capacity = newCap;
        }

        public void PreparePush(int count) {
            EnsureCapacity(Count + count);
        }
        
        // Warning: push to stack may invalid existing references.
        public ref T PushStack() {
            EnsureCapacity(Count + 1);

            return ref _items[Count++];
        }

        public ref T PopStack() {
            if (Count > 0) return ref _items[--Count];
            throw new InvalidOperationException();
        }

        public ref T this[int index] => ref _items[index];

        public ref T StackTop() => ref _items[Count - 1];

        public ref T StackTop(int skip) => ref _items[Count - skip - 1];
    }
}
