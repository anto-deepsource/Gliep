namespace GeminiLab.Glos {
    public struct GlosStackFrame {
        public GlosFunction            Function;
        public IGlosAsyncEFunctionCall Call;
        public int                     StackBase;
        public int                     ArgumentsCount;
        public int                     LocalVariablesBase;
        public int                     PrivateStackBase;
        public int                     InstructionPointer;
        public int                     NextInstructionPointer;
        public GlosOp                  LastOp;
        public byte                    Phase;
        public byte                    NextPhase;
        public byte                    PhaseCount;
        public long                    LastImm;
        public int                     DelimiterStackBase;
        public int                     ReturnSize;
        public GlosContext             Context;
    }
}
