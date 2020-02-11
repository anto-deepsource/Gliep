 using GeminiLab.Glos;
 using GeminiLab.Glos.CodeGenerator;
 using GeminiLab.Glos.ViMa;

 namespace XUnitTester {
    public abstract class GlosTestBase {
        protected readonly GlosViMa ViMa;
        protected readonly GlosUnitBuilder Builder;

        protected GlosUnit Unit => Builder.GetResult();

        protected GlosTestBase() {
            ViMa = new GlosViMa();
            Builder = new GlosUnitBuilder();
        }
    }
}
