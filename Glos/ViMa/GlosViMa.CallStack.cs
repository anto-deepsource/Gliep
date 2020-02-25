using System;

namespace GeminiLab.Glos.ViMa {
    public struct GlosStackFrame {
        public int StackBase;
        public int ArgumentsBase;
        public int ArgumentsCount;
        public int LocalVariablesBase;
        public int PrivateStackBase;
        public int InstructionPointer;
        public int DelimiterStackBase;
        public GlosFunction Function;
    }

    public partial class GlosViMa {
        // TODO: make call stack flexible and remove this constant
        internal const int MaxCallStack = 0x1000;
        private readonly GlosStackFrame[] _callStack = new GlosStackFrame[MaxCallStack];
        private long _cptr = 0;

        public ReadOnlySpan<GlosStackFrame> CurrentCallStack => new ReadOnlySpan<GlosStackFrame>(_callStack, 0, (int)_cptr);

        private ref GlosStackFrame callStackTop() {
            return ref _callStack[_cptr - 1];
        }

        private ref GlosStackFrame callStackTop(int count) {
            return ref _callStack[_cptr - count - 1];
        }

        private ref GlosStackFrame pushCallStackFrame() {
            return ref _callStack[_cptr++];
        }

        private void popCallStackFrame() {
            --_cptr;
            _callStack[_cptr].Function = null!;
        }
    }
}