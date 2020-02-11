namespace GeminiLab.Glos.ViMa {
    public class GlosFunction {
        public GlosFunction(GlosFunctionPrototype prototype, GlosTable environment) {
            Prototype = prototype;
            Environment = environment;
        }

        public GlosFunctionPrototype Prototype { get; set; }
        public GlosTable Environment { get; set; }
    }
}
