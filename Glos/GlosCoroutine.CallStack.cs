using System;

namespace GeminiLab.Glos {
    public partial class GlosCoroutine {
        private readonly GlosStack<GlosStackFrame> _callStack = new GlosStack<GlosStackFrame>();

        private int _cptr => _callStack.Count;

        private ref GlosStackFrame callStackTop() => ref _callStack.StackTop();

        private ref GlosStackFrame callStackTop(int count) {
            return ref _callStack[_cptr - count - 1];
        }

        private ref GlosStackFrame pushCallStackFrame() => ref _callStack.PushStack();

        private void popCallStackFrame() {
            _callStack.PopStack().Function = null!;
        }

        public ReadOnlySpan<GlosStackFrame> CallStackFrames => _callStack.AsSpan(0, _cptr);

        public IGlosUnit? CurrentExecutingUnit => callStackTop().Function?.Unit;
    }
}
