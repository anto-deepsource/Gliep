namespace GeminiLab.Glos.CodeGenerator {
    public class LocalVariable {
        public FunctionBuilder Builder { get; }
        public long LocalVariableId { get; }

        internal LocalVariable(FunctionBuilder builder, long localVariableId) {
            Builder = builder;
            LocalVariableId = localVariableId;
        }
    }
}
