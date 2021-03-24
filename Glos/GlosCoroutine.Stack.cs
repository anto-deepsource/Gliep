using System;

namespace GeminiLab.Glos {
    public partial class GlosCoroutine {
        private readonly GlosStack<GlosValue> _stack = new GlosStack<GlosValue>();

        private int _sptr => _stack.Count;

        private ref GlosValue stackTop() => ref _stack.StackTop();

        private ref GlosValue stackTop(int count) => ref _stack.StackTop(count);

        // TODO: add stack underflow check
        private void popStack() {
            if (_sptr - 1 < _slmt) {
                throw new InvalidOperationException();
            }
            
            _stack.PopStack().SetNil();
        }

        private void popStack(int count) {
            while (count-- > 0) popStack();
        }

        private void popUntil(int newSptr) {
            while (_stack.Count > newSptr) popStack();
        }

        private void pushStack(in GlosValue value) {
            _stack.PushStack() = value;
        }

        private ref GlosValue pushStack() {
            return ref _stack.PushStack();
        }

        private ref GlosValue pushNil() => ref pushStack();

        private void pushUntil(int newSptr) {
            while (_stack.Count < newSptr) pushStack();
        }
    }
}
