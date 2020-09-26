namespace GeminiLab.Glos.CodeGenerator {
    public class Label {
        public   FunctionBuilder Builder { get; }
        internal int             TargetCounter;

        internal Label(FunctionBuilder builder) {
            Builder = builder;
        }
    }
}
