namespace GeminiLab.Glos.CodeGenerator {
    public class Label {
        internal readonly GlosFunctionBuilder Builder;
        internal int TargetCounter;

        internal Label(GlosFunctionBuilder builder) {
            Builder = builder;
        }
    }
}
