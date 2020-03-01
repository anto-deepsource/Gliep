namespace GeminiLab.Glos.CodeGenerator {
    public class LocalVariable {
        public GlosFunctionBuilder Builder { get; }
        public long LocalVariableId { get; }

        internal LocalVariable(GlosFunctionBuilder builder, long localVariableId) {
            Builder = builder;
            LocalVariableId = localVariableId;
        }
    }
}
