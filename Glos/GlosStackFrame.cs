namespace GeminiLab.Glos {
    public struct GlosStackFrame {
        public int          StackBase;
        public int          ArgumentsBase;
        public int          ArgumentsCount;
        public int          LocalVariablesBase;
        public int          PrivateStackBase;
        public int          InstructionPointer;
        public int          Phase;
        public int          PhaseCount;
        public GlosOp       LastOp;
        public long         LastImm;
        public int          DelimiterStackBase;
        public int          ReturnSize;
        public GlosFunction Function;
        public GlosContext  Context;
    }
}
