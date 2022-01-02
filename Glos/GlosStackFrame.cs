using System;
using System.Diagnostics.CodeAnalysis;

namespace GeminiLab.Glos {
    public struct GlosStackFrame {
        public GlosFunction?            Function;
        public IGlosAsyncEFunctionCall? Call;
        public GlosFunctionPrototype?   Prototype;
        public ReadOnlyMemory<byte>     Code;
        public int                      CodeLen;
        public int                      StackBase;
        public int                      ArgumentsCount;
        public int                      LocalVariablesBase;
        public int                      PrivateStackBase;
        public int                      InstructionPointer;
        public int                      NextInstructionPointer;
        public GlosOp                   LastOp;
        public byte                     Phase;
        public byte                     NextPhase;
        public byte                     PhaseCount;
        public long                     LastImm;
        public int                      DelimiterStackBase;
        public int                      TryStackBase;
        public int                      ReturnSize;
        public GlosContext              Context;
    }

    public static class GlosStackFrameExtension {
        // we use ip: -1 and nip: -1 to mark a async external function
        public static bool InvalidIpOrAsyncEFunctionCall(this in GlosStackFrame f) {
            return f.InstructionPointer < 0
                || f.InstructionPointer > f.CodeLen
                || f.NextInstructionPointer < 0
                || f.NextInstructionPointer > f.CodeLen;
        }

        public static bool IsAsyncEFunctionCall(this in GlosStackFrame f, [NotNullWhen(true)] out IGlosAsyncEFunctionCall? call) {
            if (f.InstructionPointer < 0 && f.NextInstructionPointer < 0 && f.Call is {} c) {
                call = c;
                return true;
            }

            call = null;
            return false;
        }
    }
}
