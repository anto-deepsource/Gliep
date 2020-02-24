using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security;
using System.Text;
using GeminiLab.Glos.ViMa;

namespace GeminiLab.Glos {
    /*
    enable this class if someday we need some "non-runtime" glos exception

    public abstract class GlosException : Exception {
        public GlosException() : base() { }
        public GlosException(string message) : base(message) { }
        public GlosException(string message, Exception? innerException) : base(message, innerException) { }
    }
    */

    public class GlosRuntimeException : /* Glos */ Exception {
        public GlosViMa ViMa { get; }
        
        public GlosRuntimeException(GlosViMa viMa, string? message = null, Exception? innerException = null) : base(message ?? i18n.Strings.DefaultMessageGlosRuntimeException, innerException) {
            ViMa = viMa;
        }
    }

    public class GlosUnknownOpException : GlosRuntimeException {
        public byte Op;

        public GlosUnknownOpException(GlosViMa viMa, byte op, string? message = null) : base(viMa, message ?? string.Format(i18n.Strings.DefaultMessageGlosUnknownOpException, op)) {
            Op = op;
        }
    }

    public class GlosUnexpectedEndOfCodeException : GlosRuntimeException {
        public GlosUnexpectedEndOfCodeException(GlosViMa viMa, string? message = null) : base(viMa, message ?? i18n.Strings.DefaultMessageGlosUnexpectedEndOfCodeException) { }
    }

    public class GlosLocalVariableIndexOutOfRangeException : GlosRuntimeException {
        public int Index { get; }

        public GlosLocalVariableIndexOutOfRangeException(GlosViMa viMa, int index, string? message = null) : base(viMa, message ?? string.Format(i18n.Strings.DefaultMessageGlosLocalVariableIndexOutOfRangeException, index)) {
            Index = index;
        }
    }

    public class GlosInvalidProgramCounterException : GlosRuntimeException {
        public int ProgramCounter { get; }

        public GlosInvalidProgramCounterException(GlosViMa viMa, int pc, string? message = null) : base(viMa, message ?? string.Format(i18n.Strings.DefaultMessageGlosInvalidProgramCounterException, pc)) {
            ProgramCounter = pc;
        }
    }

    public class GlosInvalidBinaryOperandTypeException : GlosRuntimeException {
        public GlosOp Op { get; }
        public GlosValue Left { get; }
        public GlosValue Right { get; }

        public GlosInvalidBinaryOperandTypeException(GlosViMa viMa, GlosOp op, GlosValue left, GlosValue right, string? message = null) : base(viMa, message ?? string.Format(i18n.Strings.DefaultMessageGlosInvalidBinaryOperandTypeException, op, left.Type, right.Type)) {
            Op = op;
            Left = left;
            Right = right;
        }
    }

    public class GlosInvalidUnaryOperandTypeException : GlosRuntimeException {
        public GlosOp Op { get; }
        public GlosValue Operand { get; }

        public GlosInvalidUnaryOperandTypeException(GlosViMa viMa, GlosOp op, GlosValue operand, string? message = null) : base(viMa, message ?? string.Format(i18n.Strings.DefaultMessageGlosInvalidUnaryOperandTypeException, op, operand.Type)) {
            Op = op;
            Operand = operand;
        }
    }

}
