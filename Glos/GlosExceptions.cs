using System;

namespace GeminiLab.Glos {
    // Exceptions here carries no human-readable string message
    public class GlosException : Exception {
        public GlosException(Exception? innerException = null) : base(null, innerException) { }
    }

    public class GlosRuntimeException : GlosException {
        public GlosRuntimeException(GlosCoroutine coroutine, Exception innerException)
            : base(innerException) {
            ViMa = coroutine.Parent;
            Coroutine = coroutine;
        }

        public GlosViMa      ViMa      { get; }
        public GlosCoroutine Coroutine { get; }

        private GlosStackFrame[]? _callStack = null;
        public GlosStackFrame[] CallStack => _callStack ??= Coroutine.CallStackFrames.ToArray();
    }

    public class GlosUnknownOpException : GlosException {
        public GlosUnknownOpException(byte op) {
            Op = op;
        }

        public byte Op;
    }

    public class GlosUnexpectedEndOfCodeException : GlosException {
        public GlosUnexpectedEndOfCodeException(string? message = null) { }
    }

    public class GlosLocalVariableIndexOutOfRangeException : GlosException {
        public GlosLocalVariableIndexOutOfRangeException(int index) {
            Index = index;
        }

        public int Index { get; }
    }

    public class GlosStringIndexOutOfRangeException : GlosException {
        public GlosStringIndexOutOfRangeException(int index) {
            Index = index;
        }

        public int Index { get; }
    }

    public class GlosFunctionIndexOutOfRangeException : GlosException {
        public GlosFunctionIndexOutOfRangeException(int index) {
            Index = index;
        }

        public int Index { get; }
    }

    public class GlosInvalidInstructionPointerException : GlosException {
        public GlosInvalidInstructionPointerException() { }
    }

    public class GlosInvalidBinaryOperandTypeException : GlosException {
        public GlosInvalidBinaryOperandTypeException(GlosOp op, GlosValue left, GlosValue right) {
            Op = op;
            Left = left;
            Right = right;
        }

        public GlosOp Op { get; }
        public GlosValue Left { get; }
        public GlosValue Right { get; }
    }

    public class GlosInvalidUnaryOperandTypeException : GlosException {
        public GlosInvalidUnaryOperandTypeException(GlosOp op, GlosValue operand) {
            Op = op;
            Operand = operand;
        }

        public GlosOp Op { get; }
        public GlosValue Operand { get; }
    }

    public class GlosValueTypeAssertionFailedException : GlosException {
        public GlosValueTypeAssertionFailedException(GlosValue value, params GlosValueType[] expected) {
            Value = value;
            Expected = expected;
        }

        public GlosValue Value { get; }
        public GlosValueType[] Expected { get; }
    }

    public class GlosValueNotCallableException : GlosException {
        public GlosValueNotCallableException(GlosValue value) {
            Value = value;
        }

        public GlosValue Value { get; }
    }
}
