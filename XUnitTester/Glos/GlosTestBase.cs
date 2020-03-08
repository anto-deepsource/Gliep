using GeminiLab.Glos.CodeGenerator;
using GeminiLab.Glos.ViMa;

namespace XUnitTester.Glos {
    public abstract class GlosTestBase {
        protected readonly GlosViMa ViMa;
        protected readonly GlosUnitBuilder Builder;

        protected GlosUnit Unit => Builder.GetResult();

        protected GlosTestBase() {
            ViMa = new GlosViMa();
            Builder = new GlosUnitBuilder();
        }

        protected GlosValue[] Execute(GlosValue[]? args = null, GlosContext? parentContext = null) {
            return ViMa.ExecuteUnit(Unit, args, parentContext);
        }
    }
}
