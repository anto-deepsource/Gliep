namespace GeminiLab.Glos {
    public class GlosFunction {
        public GlosFunction(GlosFunctionPrototype prototype, GlosContext parentContext, IGlosUnit unit) {
            Prototype = prototype;
            ParentContext = parentContext;
            Unit = unit;
        }

        public GlosFunctionPrototype Prototype     { get; set; }
        public GlosContext           ParentContext { get; set; }
        public IGlosUnit             Unit          { get; set; }
    }
}
