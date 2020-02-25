using System;
using System.Collections.Generic;

namespace GeminiLab.Glos.ViMa {
    public partial class GlosViMa {
        #region unit
        // Answer: ViMa should not manage units.
        #endregion

        #region stack
        private readonly GlosStack<GlosValue> _stack = new GlosStack<GlosValue>();
        private int _sptr => _stack.Count;

        private ref GlosValue stackTop() => ref _stack.StackTop();

        private ref GlosValue stackTop(int count) => ref _stack.StackTop(count);

        // TODO: add stack underflow check
        private void popStack() {
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
        #endregion

        #region delimiter stack
        internal const int MaxStack = 0x10000;

        private readonly int[] _delStack = new int[MaxStack];
        private int _dptr = 0;

        private bool hasDelimiter() {
            return _dptr > callStackTop().DelimiterStackBase;
        }

        private int peekDelimiter() {
            return hasDelimiter() ? _delStack[_dptr - 1] : callStackTop().PrivateStackBase;
        }

        private int popDelimiter() {
            int rv;
            
            if (hasDelimiter()) {
                rv = _delStack[_dptr - 1];
                --_dptr;
            } else {
                rv = callStackTop().PrivateStackBase;
            }

            return rv;
        }

        private void pushDelimiter() {
            _delStack[_dptr++] = _sptr;
        }

        private void pushDelimiter(int pos) {
            _delStack[_dptr++] = pos;
        }
        #endregion                                                                                                                                                     

        public GlosValue.Comparer Comparer => new GlosValue.Comparer(this);
    }
}
