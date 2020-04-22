using System;

namespace GeminiLab.Glos {
    public class GlosException : Exception {
        // public GlosException() : base() { }
        public GlosException(string message) : base(message) { }
        public GlosException(string message, Exception? innerException) : base(message, innerException) { }
    }

    public class GlosRuntimeException : GlosException {
        public GlosRuntimeException(GlosViMa viMa, Exception innerException)
            : base(string.Format(i18n.Strings.DefaultMessageGlosRuntimeExceptionWithInner, innerException.GetType().Name, innerException.Message), innerException) {
            ViMa = viMa;
        }

        public GlosRuntimeException(GlosViMa viMa, string? message = null)
            : base(message ?? i18n.Strings.DefaultMessageGlosRuntimeException) {
            ViMa = viMa;
        }

        public GlosRuntimeException(GlosViMa viMa, string message, Exception innerException)
            : base(message, innerException) {
            ViMa = viMa;
        }

        public GlosViMa ViMa { get; }

        private GlosStackFrame[]? _callStack = null;
        public GlosStackFrame[] CallStack => _callStack ??= ViMa.CallStackFrames.ToArray();
    }

    public class GlosUnknownOpException : GlosException {
        public GlosUnknownOpException(byte op, string? message = null)
            : base(message ?? string.Format(i18n.Strings.DefaultMessageGlosUnknownOpException, op)) {
            Op = op;
        }

        public byte Op;
    }

    public class GlosUnexpectedEndOfCodeException : GlosException {
        public GlosUnexpectedEndOfCodeException(string? message = null)
            : base(message ?? i18n.Strings.DefaultMessageGlosUnexpectedEndOfCodeException) {
        }
    }

    public class GlosLocalVariableIndexOutOfRangeException : GlosException {
        public GlosLocalVariableIndexOutOfRangeException(int index, string? message = null)
            : base(message ?? string.Format(i18n.Strings.DefaultMessageGlosLocalVariableIndexOutOfRangeException, index)) {
            Index = index;
        }

        public int Index { get; }
    }

    public class GlosInvalidInstructionPointerException : GlosException {
        public GlosInvalidInstructionPointerException(string? message = null)
            : base(message ?? string.Format(i18n.Strings.DefaultMessageGlosInvalidInstructionPointerException)) {
        }
    }

    public class GlosInvalidBinaryOperandTypeException : GlosException {
        public GlosInvalidBinaryOperandTypeException(GlosOp op, GlosValue left, GlosValue right, string? message = null)
            : base(message ?? string.Format(i18n.Strings.DefaultMessageGlosInvalidBinaryOperandTypeException, op, left.Type, right.Type)) {
            Op = op;
            Left = left;
            Right = right;
        }

        public GlosOp Op { get; }
        public GlosValue Left { get; }
        public GlosValue Right { get; }
    }

    public class GlosInvalidUnaryOperandTypeException : GlosException {
        public GlosInvalidUnaryOperandTypeException(GlosOp op, GlosValue operand, string? message = null)
            : base(message ?? string.Format(i18n.Strings.DefaultMessageGlosInvalidUnaryOperandTypeException, op, operand.Type)) {
            Op = op;
            Operand = operand;
        }

        public GlosOp Op { get; }
        public GlosValue Operand { get; }
    }

    public class GlosValueTypeAssertionFailedException : GlosException {
        public GlosValueTypeAssertionFailedException(GlosValue value, GlosValueType expected, string? message = null)
            : base(message ?? string.Format(i18n.Strings.DefaultMessageGlosValueTypeAssertionFailedException, expected, value.Type)) {
            Value = value;
            Expected = expected;
        }

        public GlosValue Value { get; }
        public GlosValueType Expected { get; }
    }

    public class GlosValueNotCallableException : GlosException {
        public GlosValueNotCallableException(GlosValue value, string? message = null) 
            : base(message ?? string.Format(i18n.Strings.DefaultMessageGlosValueNotCallableException, value.ToString())) {
            Value = value;
        }

        public GlosValue Value { get; }
    }
}
