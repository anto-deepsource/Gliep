using System.Collections.Generic;

namespace GeminiLab.Glos.CodeGenerator {
    public class Label {
        internal readonly FunctionBuilder Builder;
        internal int TargetCounter;

        internal Label(FunctionBuilder builder) {
            Builder = builder;
        }
    }
}
