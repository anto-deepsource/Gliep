namespace GeminiLab.Glos.ViMa {
    public class GlosFunction {
        public GlosFunction(GlosFunctionPrototype prototype, GlosContext parentContext) {
            Prototype = prototype;
            ParentContext = parentContext;
        }

        public GlosFunctionPrototype Prototype { get; set; }
        public GlosContext ParentContext { get; set; }
    }
}
